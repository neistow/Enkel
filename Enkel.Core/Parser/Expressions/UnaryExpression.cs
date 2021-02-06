using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Expressions
{
    public class UnaryExpression : IExpression
    {
        public IToken Operator { get; }
        public IExpression Right { get; }

        public UnaryExpression(IToken op, IExpression right)
        {
            Operator = op;
            Right = right;
        }

        public T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }
    }
}