using System.Collections.Generic;
using Enkel.Core.Lexer;
using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Expressions
{
    public class CallExpression : IExpression
    {
        public IExpression Callee { get; }
        public IToken Token { get; }
        public List<IExpression> Arguments { get; }

        public CallExpression(IExpression callee, IToken token, List<IExpression> arguments)
        {
            Callee = callee;
            Token = token;
            Arguments = arguments;
        }

        public T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitCallExpression(this);
        }
    }
}