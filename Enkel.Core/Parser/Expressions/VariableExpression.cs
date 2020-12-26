using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Expressions
{
    public class VariableExpression : IExpression
    {
        public IToken Token { get; }

        public VariableExpression(IToken token)
        {
            Token = token;
        }

        public T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitVarExpr(this);
        }
    }
}