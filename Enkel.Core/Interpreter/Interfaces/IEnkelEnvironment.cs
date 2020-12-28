using Enkel.Core.Lexer.Interfaces;

namespace Enkel.Core.Interpreter.Interfaces
{
    public interface IEnkelEnvironment
    {
        void Define(IToken token, object value);
        object Get(IToken token);
        object GetAt(int distance, IToken token);
        void Assign(IToken token, object value);
        void AssignAt(int distance, IToken token, object value);
        void Clear();
    }
}