using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Expressions
{
    public class GetExpression : IExpression
    {
        public IToken Token { get; }
        public IExpression Object { get; }

        public GetExpression(IToken token, IExpression o)
        {
            Token = token;
            Object = o;
        }

        public T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitGetExpression(this);
        }
    }
}