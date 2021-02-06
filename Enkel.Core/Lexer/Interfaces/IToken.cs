namespace Enkel.Core.Lexer.Interfaces
{
    public interface IToken
    {
        public TokenType Type { get; }
        public string Lexeme { get; }
        public object Literal { get; }
        public int Line { get; }
    }
}
