using System.Text.RegularExpressions;

namespace Enkel.Lexer
{
    public static class MatchExtensions
    {
        /// <summary>
        /// Returns index of the start of the match in the string
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public static int Start(this Match match)
        {
            return match.Index;
        }

        /// <summary>
        /// Returns index of the end of the match in the string
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public static int End(this Match match)
        {
            return match.Index + match.Length;
        }
    }
}