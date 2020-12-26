using Enkel.Core.Exceptions;
using Enkel.Core.Lexer;
using Enkel.Core.Lexer.Interfaces;

namespace Enkel.Parser
{
    public class ParseException : EnkelException
    {
        public ParseException(string message, IToken token) : base($"Line {token.Line}: {message}. Got: {token.Type}")
        {
        }
    }
}