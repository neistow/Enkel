using System.Collections.Generic;
using Enkel.Core.Common;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Interpreter.Interfaces
{
    public interface IInterpreter : IExpressionVisitor<object>, IStatementVisitor<Unit>
    {
        void Interpret(IEnumerable<IStatement> statements);
        void ExecuteBlock(IEnumerable<IStatement> statements, IEnkelEnvironment environment);
        void Resolve(IExpression expression, int depth);
    }
}