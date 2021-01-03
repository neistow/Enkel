using System.Collections.Generic;
using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Interfaces;

namespace Enkel.Core.Parser.Statements
{
    public class ClassStatement : IStatement
    {
        public IToken Identifier { get; }
        public IEnumerable<FunctionStatement> Methods { get; }

        public ClassStatement(IToken identifier, IEnumerable<FunctionStatement> methods)
        {
            Identifier = identifier;
            Methods = methods;
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitClassStatement(this);
        }
    }
}