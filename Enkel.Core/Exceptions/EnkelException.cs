using System;

namespace Enkel.Core.Exceptions
{
    public class EnkelException : Exception
    {
        public EnkelException(string message) : base(message)
        {
        }
    }
}