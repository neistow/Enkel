using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Statements
{
    public class WhileStatement : IStatement
    {
        public IExpression Condition { get; }
        public BlockStatement Body { get; }

        public WhileStatement(IExpression condition, BlockStatement body)
        {
            Condition = condition;
            Body = body;
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitWhileStatement(this);
        }
    }
}