using System.Collections.Generic;
using Enkel.Core.Interpreter.Interfaces;
using Enkel.Core.Lexer.Interfaces;

namespace Enkel.Interpreter
{
    public class EnkelClass : ICallable
    {
        public IToken Token { get; }
        public IDictionary<string, EnkelFunction> Methods { get; }
        public int Arity => !Methods.TryGetValue("constructor", out var constructor) ? 0 : constructor.Arity;

        public EnkelClass(IToken token, IDictionary<string, EnkelFunction> methods)
        {
            Token = token;
            Methods = methods;
        }

        public override string ToString()
        {
            return $"[class {Token}]";
        }

        public object Call(IInterpreter interpreter, IList<object> args)
        {
            var instance = new EnkelInstance(this);
            if (Methods.TryGetValue("constructor", out var constructor))
            {
                constructor.Bind(instance).Call(interpreter, args);
            }

            return instance;
        }
    }
}