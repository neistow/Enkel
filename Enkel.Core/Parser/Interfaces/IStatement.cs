namespace Enkel.Core.Parser.Interfaces
{
    public interface IStatement
    {
        public T Accept<T>(IStatementVisitor<T> visitor);
    }
}