import { Plugin } from 'vuepress'
import { path } from '@vuepress/utils'
import * as fs from 'fs';
import type { RuleBlock } from 'markdown-it/lib/parser_block'
import type { ImportCodeTokenMeta } from './types'
import type { MarkdownEnv } from '@vuepress/markdown/lib/types'

// min length of the import code syntax, i.e. '@[import-md]()'
const MIN_LENGTH = 11

// char codes of '@[import-md'
const START_CODES = [64, 91, 105, 109, 112, 111, 114, 116, 45, 109, 100]

// regexp to match the import syntax
const SYNTAX_RE = /^@\[import-md(?:{([^{}]+)?})?(?:{([^{}]+)?})?(?:([^\]]+))?\]\(([^)]*)\)/

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

  const [, lineStart, lineEnd, info, importPath] = match
  const meta: ImportCodeTokenMeta = {
    importPath: importPath,
    lineStart: lineStart,
    lineEnd: lineEnd,
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
  // TODO: If this is supposed to make HMR work... it isnt.
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
  { importPath, lineStart, lineEnd }: ImportCodeTokenMeta,
  { filePath }: MarkdownEnv
) {
  let importFilePath = importPath

  if (!path.isAbsolute(importPath)) {
    // if the importPath is relative path, we need to resolve it
    // according to the markdown filePath
    if (!filePath) {
      return {
        importFilePath: null,
        importCode: 'Error when resolving path',
      }
    }
    importFilePath = path.resolve(filePath, '..', importPath)
  }

  // check file existence
  if (!fs.existsSync(importFilePath)) {
    return {
      importFilePath,
      importCode: 'File not found',
    }
  }

  // read file content
  const fileContent = fs.readFileSync(importFilePath).toString()
  const lines = fileContent.split('\n')
  const firstLine = lines.findIndex(l => l.includes(lineStart)) + 1
  const lastLine = lineEnd ? lines.findIndex(l => l.includes(lineEnd)) : lines.length - 1

  // resolve partial import
  return {
    importFilePath,
    importCode: lines
      .slice(firstLine, lastLine - firstLine)
      .join('\n')
      .replace(/\n?$/, '\n'),
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

