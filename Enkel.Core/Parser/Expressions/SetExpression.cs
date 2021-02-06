using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Expressions
{
    public class SetExpression : IExpression
    {
        public IExpression Object { get; }
        public IToken Token { get; }
        public IExpression Value { get; }

        public SetExpression(IExpression o, IToken token, IExpression value)
        {
            Object = o;
            Token = token;
            Value = value;
        }

        public T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitSetExpression(this);
        }
    }
}