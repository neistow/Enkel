using System.Collections.Generic;
using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Statements
{
    public class FunctionStatement : IStatement
    {
        public IToken Token { get; }
        public List<IToken> Params { get; } = new List<IToken>();
        public List<IStatement> Body { get; } = new List<IStatement>();

        public FunctionStatement(IToken token, IEnumerable<IToken> parameters, IEnumerable<IStatement> body)
        {
            Token = token;
            Params.AddRange(parameters);
            Body.AddRange(body);
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitFunctionStatement(this);
        }
    }
}