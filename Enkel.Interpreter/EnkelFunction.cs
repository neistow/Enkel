using System.Collections.Generic;
using Enkel.Core.Exceptions;
using Enkel.Core.Interpreter.Interfaces;
using Enkel.Core.Lexer;
using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Statements;

namespace Enkel.Interpreter
{
    public class DummyToken : IToken
    {
        public TokenType Type { get; }
        public string Lexeme { get; }
        public object Literal { get; }
        public int Line { get; }

        public DummyToken(TokenType type, string lexeme, object literal, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
        }
    }

    public class EnkelFunction : ICallable
    {
        private readonly FunctionStatement _declaration;
        private readonly IEnkelEnvironment _closure;
        private readonly bool _isConstructor;

        public int Arity => _declaration.Params.Count;

        public EnkelFunction(FunctionStatement declaration, IEnkelEnvironment closure, bool isConstructor)
        {
            _declaration = declaration;
            _closure = closure;
            _isConstructor = isConstructor;
        }

        public object Call(IInterpreter interpreter, IList<object> args)
        {
            var environment = new EnkelEnvironment(_closure);
            for (var i = 0; i < _declaration.Params.Count; i++)
            {
                environment.Define(_declaration.Params[i], args[i]);
            }

            try
            {
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (Return @return)
            {
                return @return.Value;
            }

            return _isConstructor ? _closure.GetAt(0, new DummyToken(TokenType.This, "this", null, 0)) : null;
        }

        public EnkelFunction Bind(EnkelInstance instance)
        {
            var env = new EnkelEnvironment(_closure);
            env.Define(new DummyToken(TokenType.This, "this", null, 0), instance);
            return new EnkelFunction(_declaration, env, _isConstructor);
        }

        public override string ToString()
        {
            return $"[func {_declaration.Token.Lexeme}]";
        }
    }
}