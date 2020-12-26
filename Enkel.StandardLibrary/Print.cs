using System;
using System.Collections.Generic;
using System.Linq;
using Enkel.Core.Interpreter.Interfaces;

namespace Enkel.StandardLibrary
{
    public class Print : ICallable
    {
        public int Arity => 1;

        public object Call(IInterpreter interpreter, IList<object> args)
        {
            Console.WriteLine(args.First());
            return null;
        }
    }
}