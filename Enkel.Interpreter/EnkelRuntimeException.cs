using Enkel.Core.Exceptions;
using Enkel.Core.Lexer.Interfaces;

namespace Enkel.Interpreter
{
    public class EnkelRuntimeException : EnkelException
    {
        public EnkelRuntimeException(string message) : base(message)
        {
        }

        public EnkelRuntimeException(string message, IToken token) : base($"Line {token.Line}: {message}")
        {
        }
    }
}