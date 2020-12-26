using Enkel.Core;
using Enkel.Core.Lexer;
using Enkel.Core.Lexer.Interfaces;

namespace Enkel.Lexer
{
    public class Token : IToken
    {
        public TokenType Type { get; }
        public string Lexeme { get; }
        public object Literal { get; }
        public int Line { get; }

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
        }

        public override string ToString()
        {
            return $"{Type} {Lexeme} {Literal}";
        }
    }
}