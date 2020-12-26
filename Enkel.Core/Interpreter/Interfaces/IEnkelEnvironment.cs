namespace Enkel.Core.Interpreter.Interfaces
{
    public interface IEnkelEnvironment
    {
        void Define(string identifier, object value);
        object Get(string identifier);
        void Assign(string identifier, object value);
        void Clear();
    }
}