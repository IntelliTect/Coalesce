import { Plugin } from 'vuepress'
import { path } from '@vuepress/utils'
import * as fs from 'fs';
import type { RuleBlock } from 'markdown-it/lib/parser_block'
import type { MarkdownEnv } from '@vuepress/markdown/lib/types'

interface ImportCodeTokenMeta {
  importPath: string;

  /// Substring of line at which to start including content (inclusive)
  start?: string;
  /// Substring of line to stop including content (inclusive)
  end?: string;

  /// Line after which to start including content (exclusive)
  after?: string;
  /// Line up until which content will be included (exclusive)
  before?: string;

  /// Markup to prepend to the extracted content
  prepend?: string;

  /// Markup to append to the extracted content
  append?: string;
}

// min length of the import code syntax, i.e. '@[import-md]()'
const MIN_LENGTH = 11

// char codes of '@[import-md'
const START_CODES = [64, 91, 105, 109, 112, 111, 114, 116, 45, 109, 100]

// regexp to match the import syntax
const SYNTAX_RE = /^@\[import-md (.+)\]\(([^)]*)\)/

const createImportCodeBlockRule = (): RuleBlock =>
  (state, startLine, endLine, silent): boolean => {
  // if it's indented more than 3 spaces, it should be a code block
  /* istanbul ignore if */
  if (state.sCount[startLine] - state.blkIndent >= 4) {
    return false
  }

  const pos = state.bMarks[startLine] + state.tShift[startLine]
  const max = state.eMarks[startLine]

  // return false if the length is shorter than min length
  if (pos + MIN_LENGTH > max) return false

  // check if it's matched the start
  for (let i = 0; i < START_CODES.length; i += 1) {
    if (state.src.charCodeAt(pos + i) !== START_CODES[i]) {
      return false
    }
  }

  // check if it's matched the syntax
  const match = state.src.slice(pos, max).match(SYNTAX_RE)
  if (!match) return false

  // return true as we have matched the syntax
  if (silent) return true

  const [, data, importPath] = match

  let jsonData;
  try {
    jsonData = JSON.parse('{' + data + '}')
  } catch {
    throw Error("Unable to parse import-md data: " + data)
  }
  const meta: ImportCodeTokenMeta = {
    ...jsonData,
    importPath
  }

  // Find the current heading that we're under
  let parentHeading = 0;
  for (let i = state.tokens.length - 1; i >= 0; i--) {
    const token = state.tokens[i];
    if (token.type == 'heading_close') {
      parentHeading = +token.tag.substring(1);
    }
  }

  
  const importedCode = resolveImportCode(meta, state.env);
  // TODO: If this is supposed to make HMR work... it doesnt.
  // if (importedCode.importFilePath) {
  //   const importedFiles = state.env.importedFiles || (state.env.importedFiles = [])
  //   importedFiles.push(importedCode.importFilePath)
  // }
  
  var tokens = state.md.parse(importedCode.importCode, state.env);
  for (let i = 0; i < tokens.length; i++) {
    const token = tokens[i];
    const newToken = state.push(token.type, token.tag, token.nesting)
    Object.assign(newToken, token);

    // Nest any headers found underneath the current header context
    if (token.type == 'heading_open' || token.type == 'heading_close') {
      const thisHeading = +token.tag.substring(1);
      newToken.tag = "h" + (thisHeading + parentHeading);
    }

    // Clear out the children since at this point in the parsing,
    // inline children haven't yet been populated.
    // See https://github.com/markdown-it/markdown-it/blob/master/docs/architecture.md
    newToken.children?.splice(0);

    newToken.map = [startLine, startLine + 1]
  }

  state.line = startLine + 1

  return true
}

function resolveImportCode (
  { importPath, start, end, after, before, prepend, append }: ImportCodeTokenMeta,
  { filePath }: MarkdownEnv
) {
  let importFilePath = importPath

  if (!path.isAbsolute(importPath)) {
    // if the importPath is relative path, we need to resolve it
    // according to the markdown filePath
    if (!filePath) {
      throw new Error(`import-md: Error when resolving path ${importPath}`);
    }
    importFilePath = path.resolve(filePath, '..', importPath)
  }

  // check file existence
  if (!fs.existsSync(importFilePath)) {
    throw new Error(`import-md: File ${importFilePath} not found`);
  }

  // read file content
  const fileContent = fs.readFileSync(importFilePath).toString()
  const lines = fileContent.split('\n')

  const match = (line: string, query?: string) => {
    if (!query) return;

    const matchStart = query.startsWith('\n')
    const matchEnd = query.endsWith('\n')
    query = matchStart ? query.substring(1) : query;
    query = matchEnd ? query.substring(0, query.length - 1) : query;

    if (matchStart && matchEnd) return line == query;
    if (matchStart) return line.startsWith(query);
    if (matchEnd) return line.endsWith(query);
    return line.includes(query);
  }

  let firstLine = lines.findIndex(l => match(l, start ?? after)) 
  if (firstLine == -1) {
    throw new Error(`import-md: start/after delimiter not found (importing ${importPath})`);
  }
  firstLine += (start ? 0 : 1)

  const lastLine = (before ?? end)
    ? firstLine + lines.slice(firstLine).findIndex(l => match(l, before ?? end)) + (end ? 1 : 0)
    : lines.length - 1

  const content = lines
    .slice(firstLine, lastLine)
    .join('\n')
    .replace(/\n?$/, '\n');
  
  if (!content.trim()) {
    throw new Error(`import-md: Imported content was empty Perhaps the delimiters are incorrect? (importing ${importPath})`);
  }

  // resolve partial import
  return {
    importFilePath,
    importCode: 
      (prepend ? prepend + "\n" : '') +
      content + 
      (append ? append : ''),
  }
}
  
export const importMdPlugin = () => ({
  name: 'import-md-plugin',
  extendsMarkdown: async (md) => {
    // add import_md block rule
    md.block.ruler.before(
      'paragraph',
      'import_md',
      createImportCodeBlockRule(),
      {
        alt: ['paragraph', 'reference', 'blockquote', 'list'],
      }
    )
  },
} as Plugin);

