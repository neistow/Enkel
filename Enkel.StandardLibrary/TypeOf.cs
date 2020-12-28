using System;
using System.Collections.Generic;
using System.Linq;
using Enkel.Core.Exceptions;
using Enkel.Core.Interpreter.Interfaces;

namespace Enkel.StandardLibrary
{
    public class TypeOf : ICallable
    {
        public int Arity => 1;

        public object Call(IInterpreter interpreter, IList<object> args)
        {
            var argument = args.First();
            Console.WriteLine(GetEnkelTypeFromUnderlying(argument));
            return null;
        }

        private string GetEnkelTypeFromUnderlying(object obj)
        {
            return obj switch
            {
                string _ => "string",
                double _ => "number",
                bool _ => "bool",
                null => "none",
                ICallable _ => "function",
                _ => throw new EnkelRuntimeException($"Can't get type of {obj}")
            };
        }
    }
}