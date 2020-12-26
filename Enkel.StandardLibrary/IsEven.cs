using System;
using System.Collections.Generic;
using System.Linq;
using Enkel.Core.Interpreter.Interfaces;

namespace Enkel.StandardLibrary
{
    public class IsEven : ICallable
    {
        public int Arity => 1;

        public object Call(IInterpreter interpreter, IList<object> args)
        {
            return Convert.ToDouble(args.First()) % 2 == 0;
        }

        public override string ToString()
        {
            return "function IsEven";
        }
    }
}