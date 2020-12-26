using System.Collections.Generic;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Statements
{
    public class BlockStatement : IStatement
    {
        public IEnumerable<IStatement> Statements { get; }

        public BlockStatement(IEnumerable<IStatement> statements)
        {
            Statements = statements;
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitBlockStatement(this);
        }
    }
}