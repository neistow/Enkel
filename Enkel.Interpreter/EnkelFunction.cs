using System.Collections.Generic;
using Enkel.Core.Exceptions;
using Enkel.Core.Interpreter.Interfaces;
using Enkel.Core.Parser.Statements;

namespace Enkel.Interpreter
{
    public class EnkelFunction : ICallable
    {
        private readonly FunctionStatement _declaration;
        private readonly IEnkelEnvironment _closure;

        public int Arity => _declaration.Params.Count;

        public EnkelFunction(FunctionStatement declaration, IEnkelEnvironment closure)
        {
            _declaration = declaration;
            _closure = closure;
        }

        public object Call(IInterpreter interpreter, IList<object> args)
        {
            var environment = new EnkelEnvironment(_closure);
            for (var i = 0; i < _declaration.Params.Count; i++)
            {
                environment.Define(_declaration.Params[i].Lexeme, args[i]);
            }

            try
            {
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (Return @return)
            {
                return @return.Value;
            }

            return null;
        }

        public override string ToString()
        {
            return $"[func {_declaration.Token.Lexeme}]";
        }
    }
}