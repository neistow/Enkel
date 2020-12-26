using System.Collections.Generic;

namespace Enkel.Core.Interpreter.Interfaces
{
    public interface ICallable
    {
        int Arity { get; }
        object Call(IInterpreter interpreter, IList<object> args);
    }
}