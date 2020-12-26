using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Statements
{
    public class ExpressionStatement : IStatement
    {
        public IExpression Expression { get; }

        public ExpressionStatement(IExpression expression)
        {
            Expression = expression;
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }
    }
}