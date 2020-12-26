using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Statements
{
    public class IfStatement : IStatement
    {
        public IExpression Condition { get; }
        public BlockStatement Body { get; }
        public IStatement ElseBranch { get; }

        public IfStatement(IExpression condition, BlockStatement body, IStatement elseBranch)
        {
            Condition = condition;
            Body = body;
            ElseBranch = elseBranch;
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitIfStatement(this);
        }
    }
}