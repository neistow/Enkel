using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Expressions
{
    public class GroupingExpression : IExpression
    {
        public IExpression Expression { get; }

        public GroupingExpression(IExpression expression)
        {
            Expression = expression;
        }

        public T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitGroupingExpression(this);
        }
    }
}