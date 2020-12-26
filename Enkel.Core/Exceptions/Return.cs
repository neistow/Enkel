using System;

namespace Enkel.Core.Exceptions
{
    public class Return : Exception
    {
        public object Value { get; }
        
        public Return(object value)
        {
            Value = value;
        }
    }
}