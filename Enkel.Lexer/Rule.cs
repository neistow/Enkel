using Enkel.Core.Lexer;

namespace Enkel.Lexer
{
    public class Rule
    {
        public string RegexString { get; }

        public Rule(string regex, TokenType type)
        {
            RegexString = $@"(?<{type.ToString()}>\G{regex})";
        }
    }
}