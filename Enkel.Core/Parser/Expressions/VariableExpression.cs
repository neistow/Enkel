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

        public T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitVarExpression(this);
        }
    }
}