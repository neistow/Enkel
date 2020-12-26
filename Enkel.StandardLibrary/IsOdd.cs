using System;
using System.Collections.Generic;
using Enkel.Core;
using Enkel.Core.Interpreter.Interfaces;

namespace Enkel.StandardLibrary
{
    public class IsOdd : ICallable
    {
        public int Arity => 1;

        public object Call(IInterpreter interpreter, IList<object> args)
        {
            var even = new IsEven();
            return !Convert.ToBoolean(even.Call(interpreter, args));
        }

        public override string ToString()
        {
            return "function IsOdd";
        }
    }
}