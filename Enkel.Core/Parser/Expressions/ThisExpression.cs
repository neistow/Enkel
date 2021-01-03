using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Expressions
{
    public class ThisExpression : IExpression
    {
        public IToken Token { get; }

        public ThisExpression(IToken token)
        {
            Token = token;
        }

        public T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitThisExpression(this);
        }
    }
}