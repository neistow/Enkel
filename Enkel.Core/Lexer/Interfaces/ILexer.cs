using System.Collections.Generic;

namespace Enkel.Core.Lexer.Interfaces
{
    public interface ILexer
    {
        IList<IToken> Tokens();
    }
}