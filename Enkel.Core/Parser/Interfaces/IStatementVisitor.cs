using Enkel.Core.Parser.Statements;

namespace Enkel.Core.Parser.Interfaces
{
    public interface IStatementVisitor<T>
    {
        T VisitExpressionStatement(ExpressionStatement statement);
        T VisitVarStatement(VarStatement statement);
        T VisitBlockStatement(BlockStatement statement);
        T VisitIfStatement(IfStatement statement);
        T VisitWhileStatement(WhileStatement statement);
        T VisitFunctionStatement(FunctionStatement statement);
        T VisitReturnStatement(ReturnStatement statement);
    }
}