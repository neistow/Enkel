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

        public T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitGroupingExpr(this);
        }
    }
}