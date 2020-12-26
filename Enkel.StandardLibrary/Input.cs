using System;
using System.Collections.Generic;
using Enkel.Core.Interpreter.Interfaces;

namespace Enkel.StandardLibrary
{
    public class Input : ICallable
    {
        public int Arity => 0;
        public object Call(IInterpreter interpreter, IList<object> args)
        {
            return Console.ReadLine();
        }
    }
}