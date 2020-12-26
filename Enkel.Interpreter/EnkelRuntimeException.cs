using Enkel.Core.Exceptions;

namespace Enkel.Interpreter
{
    public class EnkelRuntimeException : EnkelException
    {
        public EnkelRuntimeException(string message) : base(message)
        {
        }
    }
}