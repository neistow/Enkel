namespace Enkel.Core.Parser.Interfaces
{
    public interface IExpression
    {
        public T Accept<T>(IExpressionVisitor<T> visitor);
    }
}