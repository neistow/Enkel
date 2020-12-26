using System.Collections.Generic;
using Enkel.Core.Common;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Interpreter.Interfaces
{
    public interface IInterpreter : IExprVisitor<object>, IStatementVisitor<Unit>
    {
        void Interpret(IEnumerable<IStatement> statements);
        void ExecuteBlock(IEnumerable<IStatement> statements, IEnkelEnvironment environment);
    }
}