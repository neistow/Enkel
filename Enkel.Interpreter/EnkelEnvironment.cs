using System.Collections.Generic;
using Enkel.Core.Interpreter.Interfaces;

namespace Enkel.Interpreter
{
    public class EnkelEnvironment : IEnkelEnvironment
    {
        private readonly IEnkelEnvironment _outerEnvironment;
        private readonly Dictionary<string, object> _identifiers = new Dictionary<string, object>();

        public EnkelEnvironment()
        {
            _outerEnvironment = null;
        }

        public EnkelEnvironment(IEnkelEnvironment outerEnvironment)
        {
            _outerEnvironment = outerEnvironment;
        }

        public void Define(string identifier, object value)
        {
            if (!_identifiers.TryAdd(identifier, value))
            {
                throw new EnkelRuntimeException($"Attempt to redefine identifier: {identifier}");
            }
        }

        public object Get(string identifier)
        {
            if (_identifiers.TryGetValue(identifier, out var value))
            {
                return value;
            }

            if (_outerEnvironment != null)
            {
                return _outerEnvironment.Get(identifier);
            }

            throw new EnkelRuntimeException($"Undefined variable: {identifier}");
        }

        public void Assign(string identifier, object value)
        {
            if (_identifiers.ContainsKey(identifier))
            {
                _identifiers[identifier] = value;
                return;
            }

            if (_outerEnvironment == null)
            {
                throw new EnkelRuntimeException($"Variable '{identifier}' is not defined");
            }

            _outerEnvironment.Assign(identifier, value);
        }

        public void Clear()
        {
            _identifiers.Clear();
        }
    }
}