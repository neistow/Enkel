using Enkel.Core.Parser.Statements;

namespace Enkel.Interpreter
{
    public static class FunctionStatementExtensions
    {
        public static bool IsConstructor(this FunctionStatement functionStatement)
        {
            return functionStatement.Token.Lexeme == "constructor";
        }
    }
}