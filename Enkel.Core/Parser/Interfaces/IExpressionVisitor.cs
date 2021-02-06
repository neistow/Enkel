using Enkel.Core.Parser.Expressions;

namespace Enkel.Core.Parser.Interfaces
{
    public interface IExpressionVisitor<T>
    {
        T VisitBinaryExpression(BinaryExpression expression);
        T VisitGroupingExpression(GroupingExpression expression);
        T VisitLiteralExpression(LiteralExpression expression);
        T VisitUnaryExpression(UnaryExpression expression);
        T VisitVarExpression(VariableExpression expression);
        T VisitAssignmentExpression(AssignmentExpression expression);
        T VisitLogicalExpression(LogicalExpression expression);
        T VisitCallExpression(CallExpression expression);
        T VisitGetExpression(GetExpression expression);
        T VisitSetExpression(SetExpression expression);
        T VisitThisExpression(ThisExpression expression);
    }
}