using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Enkel.Core;
using Enkel.Core.Exceptions;
using Enkel.Core.Lexer;
using Enkel.Core.Lexer.Interfaces;

namespace Enkel.Lexer
{
    public class EnkelLexer : ILexer
    {
        private readonly IList<string> _source = new List<string>();

        private static readonly IRule[] Rules =
        {
            new Rule(@"class(?!\w+)", TokenType.Class),
            new Rule(@"if(?!\w+)", TokenType.If),
            new Rule(@"else(?!\w+)", TokenType.Else),
            new Rule(@"and(?!\w+)", TokenType.And),
            new Rule(@"or(?!\w+)", TokenType.Or),
            new Rule(@"true(?!\w+)", TokenType.True),
            new Rule(@"false(?!\w+)", TokenType.False),
            new Rule(@"func(?!\w+)", TokenType.Func),
            new Rule(@"for(?!\w+)", TokenType.For),
            new Rule(@"while(?!\w+)", TokenType.While),
            new Rule(@"none(?!\w+)", TokenType.None),
            new Rule(@"return(?!\w+)", TokenType.Return),
            new Rule(@"parent(?!\w+)", TokenType.Parent),
            new Rule(@"this(?!\w+)", TokenType.This),
            new Rule(@"var(?!\w+)", TokenType.Var),

            new Rule(@"\(", TokenType.LeftParen),
            new Rule(@"\)", TokenType.RightParen),
            new Rule(@"\{", TokenType.LeftBrace),
            new Rule(@"\}", TokenType.RightBrace),
            new Rule(@"\,", TokenType.Comma),
            new Rule(@"\.", TokenType.Dot),
            new Rule(@"\-", TokenType.Minus),
            new Rule(@"\+", TokenType.Plus),
            new Rule(@"\;", TokenType.Semicolon),
            new Rule(@"\/", TokenType.Slash),
            new Rule(@"\*", TokenType.Star),

            new Rule(@"!=", TokenType.BangEqual),
            new Rule(@"==", TokenType.DoubleEqual),
            new Rule(@"!", TokenType.Bang),
            new Rule(@"=", TokenType.Equal),
            new Rule(@">=", TokenType.GreaterEqual),
            new Rule(@"<=", TokenType.LessEqual),
            new Rule(@"\>", TokenType.Greater),
            new Rule(@"\<", TokenType.Less),

            new Rule(@"[a-zA-Z]\w*", TokenType.Identifier),
            new Rule(@"""[^""]*""", TokenType.String),
            new Rule(@"\d+(\.?\d+)?", TokenType.Number)
        };

        private readonly Regex _rulesRegex;
        private readonly Regex _tokenStartRegex;

        public EnkelLexer(params string[] source)
        {
            var commentRegex = new Regex(@"\/{2}.*");
            foreach (var line in source)
            {
                _source.Add(commentRegex.Replace(line, ""));
            }

            _tokenStartRegex = new Regex(@"\S");

            var rulesString = string.Join('|', Rules.Select(r => r.RegexString));
            _rulesRegex = new Regex(rulesString);
        }

        public IList<IToken> Tokens()
        {
            var tokens = new List<IToken>();
            for (var i = 0; i < _source.Count; i++)
            {
                tokens.AddRange(GetTokens(_source[i], i + 1));
            }

            tokens.Add(new Token(TokenType.Eof, null, null, _source.Count));
            return tokens;
        }

        private IEnumerable<IToken> GetTokens(string line, int lineNumber)
        {
            var currentPosition = 0;
            while (true)
            {
                if (currentPosition > line.Length)
                {
                    yield break;
                }

                var nextToken = _tokenStartRegex.Match(line, currentPosition);
                if (!nextToken.Success)
                {
                    yield break;
                }

                currentPosition = nextToken.Start();

                var match = _rulesRegex.Match(line, currentPosition);
                if (match.Success)
                {
                    currentPosition = match.End();

                    var matchedGroupName = (match.Groups as IList<Group>).First(g => g.Success && g.Name.Length > 1);
                    if (Enum.TryParse(matchedGroupName.Name, out TokenType tokenType))
                    {
                        yield return tokenType switch
                        {
                            TokenType.Number => new Token(tokenType, match.Value,
                                double.Parse(match.Value, CultureInfo.InvariantCulture), lineNumber),
                            TokenType.String => new Token(tokenType, match.Value, match.Value.Trim('"'),
                                lineNumber),
                            _ => new Token(tokenType, match.Value, null, lineNumber)
                        };

                        continue;
                    }
                }

                throw new LexerException(
                    $"Can't parse token at substring:\n{line.Substring(currentPosition)}");
            }
        }
    }
}