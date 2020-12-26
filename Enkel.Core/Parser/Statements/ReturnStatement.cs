using Enkel.Core.Lexer;
using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Statements
{
    public class ReturnStatement : IStatement
    {
        public IToken Token { get; }
        public IExpression Value { get; }

        public ReturnStatement(IToken token, IExpression value)
        {
            Token = token;
            Value = value;
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitReturnStatement(this);
        }
    }
}