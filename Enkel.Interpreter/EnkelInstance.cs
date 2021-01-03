using System.Collections.Generic;
using Enkel.Core.Lexer.Interfaces;

namespace Enkel.Interpreter
{
    public class EnkelInstance
    {
        private readonly EnkelClass _class;
        private readonly Dictionary<string, object> _fields = new Dictionary<string, object>();

        public EnkelInstance(EnkelClass @class)
        {
            _class = @class;
        }

        public object Get(IToken property)
        {
            if (_fields.TryGetValue(property.Lexeme, out var value))
            {
                return value;
            }

            if (_class.Methods.TryGetValue(property.Lexeme, out var method))
            {
                return method.Bind(this);
            }

            throw new EnkelRuntimeException($"Can't access the property {property.Lexeme} of class", property);
        }

        public void Set(IToken property, object value)
        {
            if (_fields.ContainsKey(property.Lexeme))
            {
                _fields[property.Lexeme] = value;
            }

            _fields.Add(property.Lexeme, value);
        }

        public override string ToString()
        {
            return $"[{_class.Token.Lexeme} class instance]";
        }
    }
}