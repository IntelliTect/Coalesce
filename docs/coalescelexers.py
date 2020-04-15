
from pygments.lexers.javascript import TypeScriptLexer

class VueLexer(TypeScriptLexer):
    name = 'Vue'

class KnockoutLexer(TypeScriptLexer):
    name = 'Knockout'

__all__ = ['KnockoutLexer', 'VueLexer']