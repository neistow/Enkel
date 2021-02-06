using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Expressions
{
    public class LogicalExpression : IExpression
    {
        public IExpression Left { get; }
        public IToken Operator { get; }
        public IExpression Right { get; }

        public LogicalExpression(IExpression left, IToken op, IExpression right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitLogicalExpression(this);
        }
    }
}