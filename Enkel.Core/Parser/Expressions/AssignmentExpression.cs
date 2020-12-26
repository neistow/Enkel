using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Expressions
{
    public class AssignmentExpression : IExpression
    {
        public IToken Variable { get; }
        public IExpression Expression { get; }

        public AssignmentExpression(IToken variable, IExpression expression)
        {
            Variable = variable;
            Expression = expression;
        }

        public T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitAssignmentExpr(this);
        }
    }
}