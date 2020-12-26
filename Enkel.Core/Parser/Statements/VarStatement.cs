using Enkel.Core.Lexer;
using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Statements
{
    public class VarStatement : IStatement
    {
        public IToken Token { get; }
        public IExpression Initializer { get; }

        public VarStatement(IToken token, IExpression initializer)
        {
            Token = token;
            Initializer = initializer;
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitVarStatement(this);
        }
    }
}