using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Expressions
{
    public class LiteralExpression : IExpression
    {
        public object Value { get; }

        public LiteralExpression(object value)
        {
            Value = value;
        }

        public T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpression(this);
        }

        public override string ToString()
        {
            return Value == null ? "none" : Value.ToString();
        }
    }
}