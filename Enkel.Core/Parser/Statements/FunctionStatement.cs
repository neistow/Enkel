using System.Collections.Generic;
using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Statements
{
    public class FunctionStatement : IStatement
    {
        public IToken Name { get; }
        public List<IToken> Params { get; } = new List<IToken>();
        public List<IStatement> Body { get; } = new List<IStatement>();

        public FunctionStatement(IToken name, IEnumerable<IToken> parameters, IEnumerable<IStatement> body)
        {
            Name = name;
            Params.AddRange(parameters);
            Body.AddRange(body);
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitFunctionStatement(this);
        }
    }
}