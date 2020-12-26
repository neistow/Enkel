using Enkel.Core;
using Enkel.Core.Lexer;
using Enkel.Core.Lexer.Interfaces;

namespace Enkel.Lexer
{
    public class Rule : IRule
    {
        public string RegexString { get; }

        public Rule(string regex, TokenType type)
        {
            RegexString = $@"(?<{type.ToString()}>\G{regex})";
        }
    }
}