
from pygments.lexers.javascript import TypeScriptLexer
from pygments.lexers.html import HtmlLexer

from pygments.lexer import include, using

class VueLexer(TypeScriptLexer):
    name = 'Vue'

class KnockoutLexer(TypeScriptLexer):
    name = 'Knockout'

def updateHtmlTokenizers(dict):
    ret = { **dict }
    
    for key in ret:
        for i, tokenizer in enumerate(ret[key]):
            # Add '#' and '.' as valid attribute name characters to regexes that look like they're trying to parse attribute characters.
            ret[key][i] = (tokenizer[0].replace(r'\w:-', r'\w#.:-'), *tokenizer[1:],)

    # Lex scripts as TS, not JS (supports decorators)
    ret["script-content"][1] = (ret["script-content"][1][0], using(TypeScriptLexer))
    return ret

class VueSfcLexer(HtmlLexer):
    name = 'VueSfc'
    tokens = updateHtmlTokenizers(HtmlLexer.tokens)
    
__all__ = ['KnockoutLexer', 'VueLexer', 'VueSfcLexer']