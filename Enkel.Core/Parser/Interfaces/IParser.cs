using System.Collections.Generic;

namespace Enkel.Core.Parser.Interfaces
{
    public interface IParser
    {
        IEnumerable<IStatement> Parse();
    }
}