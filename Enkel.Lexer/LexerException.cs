using Enkel.Core.Exceptions;

namespace Enkel.Lexer
{
    public class LexerException : EnkelException
    {
        public LexerException(string message) : base(message)
        {
        }
    }
}