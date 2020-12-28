using System.Collections.Generic;
using Enkel.Core.Interpreter.Interfaces;
using Enkel.Core.Lexer.Interfaces;
using Enkel.StandardLibrary;

namespace Enkel.Interpreter
{
    public class EnkelEnvironment : IEnkelEnvironment
    {
        private readonly IEnkelEnvironment _outerEnvironment;
        private readonly Dictionary<string, object> _identifiers = new Dictionary<string, object>();

        public static readonly EnkelEnvironment GlobalEnvironment;

        static EnkelEnvironment()
        {
            GlobalEnvironment = new EnkelEnvironment(new Dictionary<string, object>
            {
                {"IsOdd", new IsOdd()},
                {"IsEven", new IsEven()},
                {"Print", new Print()},
                {"Input", new Input()},
                {"TypeOf", new TypeOf()}
            });
        }

        public EnkelEnvironment(IEnkelEnvironment outerEnvironment)
        {
            _outerEnvironment = outerEnvironment;
        }

        private EnkelEnvironment(Dictionary<string, object> identifiers)
        {
            _outerEnvironment = null;
            _identifiers = identifiers;
        }

        public void Define(IToken token, object value)
        {
            if (!_identifiers.TryAdd(token.Lexeme, value))
            {
                throw new EnkelRuntimeException($"Attempt to redefine identifier: {token.Lexeme}", token);
            }
        }

        public object Get(IToken token)
        {
            if (_identifiers.TryGetValue(token.Lexeme, out var value))
            {
                return value;
            }

            if (_outerEnvironment != null)
            {
                return _outerEnvironment.Get(token);
            }

            throw new EnkelRuntimeException($"Undefined variable: {token.Lexeme}", token);
        }

        public object GetAt(int distance, IToken token)
        {
            var env = EnvironmentAtDistance(distance);
            if (!env._identifiers.TryGetValue(token.Lexeme, out var value))
            {
                throw new EnkelRuntimeException($"Undefinied variable: {token.Lexeme}", token);
            }

            return value;
        }

        public void Assign(IToken token, object value)
        {
            if (_identifiers.ContainsKey(token.Lexeme))
            {
                _identifiers[token.Lexeme] = value;
                return;
            }

            if (_outerEnvironment == null)
            {
                throw new EnkelRuntimeException($"Variable '{token.Lexeme}' is not defined", token);
            }

            _outerEnvironment.Assign(token, value);
        }

        public void AssignAt(int distance, IToken token, object value)
        {
            var env = EnvironmentAtDistance(distance);
            if (!env._identifiers.ContainsKey(token.Lexeme))
            {
                throw new EnkelRuntimeException($"Can't assign an undefined variable: {token.Lexeme}", token);
            }

            env._identifiers[token.Lexeme] = value;
        }

        public void Clear()
        {
            _identifiers.Clear();
        }

        private EnkelEnvironment EnvironmentAtDistance(int distance)
        {
            var env = this;
            for (var i = 0; i < distance; i++)
            {
                env = env._outerEnvironment as EnkelEnvironment;
            }

            return env;
        }
    }
}