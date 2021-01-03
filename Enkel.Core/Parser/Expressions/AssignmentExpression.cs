using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Expressions
{
    public class AssignmentExpression : IExpression
    {
        public IToken Target { get; }
        public IExpression Expression { get; }

        public AssignmentExpression(IToken target, IExpression expression)
        {
            Target = target;
            Expression = expression;
        }

        public T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitAssignmentExpression(this);
        }
    }
}