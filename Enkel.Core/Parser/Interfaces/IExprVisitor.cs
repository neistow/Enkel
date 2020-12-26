using Enkel.Core.Parser.Expressions;

namespace Enkel.Core.Parser.Interfaces
{
    public interface IExprVisitor<T>
    {
        T VisitBinaryExpr(BinaryExpression expr);
        T VisitGroupingExpr(GroupingExpression expr);
        T VisitLiteralExpr(LiteralExpression expr);
        T VisitUnaryExpr(UnaryExpression expr);
        T VisitVarExpr(VariableExpression expr);
        T VisitAssignmentExpr(AssignmentExpression expr);
        T VisitLogicalExpr(LogicalExpression expr);
        T VisitCallExpr(CallExpression expr);
    }
}