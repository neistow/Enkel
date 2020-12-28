using System;
using System.Collections.Generic;
using System.Linq;
using Enkel.Core.Common;
using Enkel.Core.Exceptions;
using Enkel.Core.Interpreter.Interfaces;
using Enkel.Core.Lexer;
using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Expressions;
using Enkel.Core.Parser.Interfaces;
using Enkel.Core.Parser.Statements;

namespace Enkel.Interpreter
{
    public class EnkelInterpreter : IInterpreter
    {
        private IEnkelEnvironment _globals;
        private IEnkelEnvironment _environment;
        private readonly Dictionary<IExpression, int> _locals = new Dictionary<IExpression, int>();

        public EnkelInterpreter()
        {
            _globals = EnkelEnvironment.GlobalEnvironment;
            _environment = _globals;
        }

        public void Interpret(IEnumerable<IStatement> statements)
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }

        public object VisitBinaryExpr(BinaryExpression expr)
        {
            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);

            switch (expr.Operator.Type)
            {
                case TokenType.Minus:
                    EnsureOperandsAreNumbers(expr.Operator, left, right);
                    return Convert.ToDouble(left) - Convert.ToDouble(right);
                case TokenType.Plus:
                    return left switch
                    {
                        double d1 when right is double d2 => Convert.ToDouble(d1) + Convert.ToDouble(d2),
                        string str1 when right is string str2 => Convert.ToString(str1) + Convert.ToString(str2),
                        _ => throw new EnkelRuntimeException(
                            $"{expr.Operator}: Operands must be either strings or numbers", expr.Operator)
                    };
                case TokenType.Slash:
                    EnsureOperandsAreNumbers(expr.Operator, left, right);

                    var first = Convert.ToDouble(left);
                    var second = Convert.ToDouble(right);

                    if (second == 0)
                    {
                        throw new EnkelRuntimeException($"{expr.Operator}: Division by zero", expr.Operator);
                    }

                    return first / second;
                case TokenType.Star:
                    EnsureOperandsAreNumbers(expr.Operator, left, right);
                    return Convert.ToDouble(left) * Convert.ToDouble(right);
                case TokenType.Greater:
                    EnsureOperandsAreNumbers(expr.Operator, left, right);
                    return Convert.ToDouble(left) > Convert.ToDouble(right);
                case TokenType.GreaterEqual:
                    EnsureOperandsAreNumbers(expr.Operator, left, right);
                    return Convert.ToDouble(left) >= Convert.ToDouble(right);
                case TokenType.Less:
                    EnsureOperandsAreNumbers(expr.Operator, left, right);
                    return Convert.ToDouble(left) < Convert.ToDouble(right);
                case TokenType.LessEqual:
                    EnsureOperandsAreNumbers(expr.Operator, left, right);
                    return Convert.ToDouble(left) <= Convert.ToDouble(right);
                case TokenType.BangEqual:
                    return !IsEqual(left, right);
                case TokenType.DoubleEqual:
                    return IsEqual(left, right);
            }

            return null;
        }

        public object VisitGroupingExpr(GroupingExpression expr)
        {
            return Evaluate(expr.Expression);
        }

        public object VisitLiteralExpr(LiteralExpression expr)
        {
            return expr.Value;
        }

        public object VisitUnaryExpr(UnaryExpression expr)
        {
            var right = Evaluate(expr.Right);

            switch (expr.Operator.Type)
            {
                case TokenType.Minus:
                    EnsureOperandIsNumber(expr.Operator, right);
                    return -Convert.ToDouble(right);
                case TokenType.Bang:
                    EnsureOperandIsBool(expr.Operator, right);
                    return !Convert.ToBoolean(right);
            }

            return null;
        }

        public Unit VisitExpressionStatement(ExpressionStatement statement)
        {
            Evaluate(statement.Expression);

            return Unit.Value;
        }

        public Unit VisitVarStatement(VarStatement statement)
        {
            object value = null;
            if (statement.Initializer != null)
            {
                value = Evaluate(statement.Initializer);
            }

            _environment.Define(statement.Token, value);
            return Unit.Value;
        }

        public Unit VisitBlockStatement(BlockStatement statement)
        {
            ExecuteBlock(statement.Statements, new EnkelEnvironment(_environment));

            return Unit.Value;
        }

        public Unit VisitIfStatement(IfStatement statement)
        {
            var current = statement;
            while (true)
            {
                var conditionResult = Evaluate(current.Condition);
                if (!(conditionResult is bool result))
                {
                    throw new EnkelRuntimeException("Expression result in if statement must be a bool value");
                }

                if (result)
                {
                    ExecuteBlock(current.Body.Statements, new EnkelEnvironment(_environment));
                    return Unit.Value;
                }

                if (current.ElseBranch == null)
                {
                    return Unit.Value;
                }

                if (current.ElseBranch is IfStatement ifStatement)
                {
                    current = ifStatement;
                    continue;
                }

                if (current.ElseBranch is BlockStatement blockStatement)
                {
                    ExecuteBlock(blockStatement.Statements, new EnkelEnvironment(_environment));
                    return Unit.Value;
                }

                throw new EnkelRuntimeException("Invalid statement in else branch");
            }
        }

        public Unit VisitWhileStatement(WhileStatement statement)
        {
            var loopEnvironment = new EnkelEnvironment(_environment);

            while (true)
            {
                var value = Evaluate(statement.Condition);
                if (!(value is bool result))
                {
                    throw new EnkelRuntimeException("Expression in while statement must be a bool value");
                }

                if (!result)
                {
                    return Unit.Value;
                }

                ExecuteBlock(statement.Body.Statements, loopEnvironment);
                loopEnvironment.Clear();
            }
        }

        public Unit VisitFunctionStatement(FunctionStatement statement)
        {
            var function = new EnkelFunction(statement, _environment);
            _environment.Define(statement.Name, function);
            return Unit.Value;
        }

        public Unit VisitReturnStatement(ReturnStatement statement)
        {
            object result = null;
            if (statement.Value != null)
            {
                result = Evaluate(statement.Value);
            }

            throw new Return(result);
        }

        public object VisitVarExpr(VariableExpression expr)
        {
            return FindVariable(expr.Token, expr);
        }

        public object VisitAssignmentExpr(AssignmentExpression expr)
        {
            var value = Evaluate(expr.Expression);

            if (!_locals.TryGetValue(expr, out var distance))
            {
                _globals.Assign(expr.Target, value);
            }
            else
            {
                _environment.AssignAt(distance, expr.Target, value);
            }

            return value;
        }

        public object VisitLogicalExpr(LogicalExpression expr)
        {
            var left = Evaluate(expr.Left);

            if (!(left is bool result))
            {
                throw new EnkelRuntimeException($"{expr.Operator.Lexeme}: expression result must be a bool type",
                    expr.Operator);
            }

            if (expr.Operator.Type == TokenType.Or)
            {
                if (result)
                {
                    return left;
                }
            }
            else
            {
                if (!result)
                {
                    return left;
                }
            }

            return Evaluate(expr.Right);
        }

        public object VisitCallExpr(CallExpression expr)
        {
            var callee = Evaluate(expr.Callee);

            var args = expr.Arguments.Select(Evaluate).ToList();

            if (!(callee is ICallable function))
            {
                throw new EnkelRuntimeException($"{callee} is not callable", expr.Token);
            }

            if (args.Count != function.Arity)
            {
                throw new EnkelRuntimeException($"Expected {function.Arity} arguments. Got: {args.Count}", expr.Token);
            }

            return function.Call(this, args);
        }

        public void ExecuteBlock(IEnumerable<IStatement> statements, IEnkelEnvironment environment)
        {
            var previousEnvironment = _environment;
            try
            {
                _environment = environment;
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                _environment = previousEnvironment;
            }
        }

        public void Resolve(IExpression expression, int depth)
        {
            if (!_locals.TryAdd(expression, depth))
            {
                throw new ResolveException($"Can't add a local expression: {expression}");
            }
        }

        private void Execute(IStatement statement)
        {
            statement.Accept(this);
        }

        private void EnsureOperandIsBool(IToken @operator, object operand)
        {
            if (operand is bool)
            {
                return;
            }

            throw new EnkelRuntimeException($"{@operator}: Operand must be a boolean", @operator);
        }

        private void EnsureOperandsAreNumbers(IToken @operator, object firstOperand, object secondOperand)
        {
            if (firstOperand is double && secondOperand is double)
            {
                return;
            }

            throw new EnkelRuntimeException($"{@operator}: Operands must be numbers", @operator);
        }

        private void EnsureOperandIsNumber(IToken @operator, object operand)
        {
            if (operand is double)
            {
                return;
            }

            throw new EnkelRuntimeException($"{@operator}: Operand must be a number", @operator);
        }

        private bool IsEqual(object first, object second)
        {
            return first switch
            {
                null when second == null => true,
                null => false,
                _ => first.Equals(second)
            };
        }

        private object Evaluate(IExpression expr)
        {
            return expr.Accept(this);
        }

        private object FindVariable(IToken token, IExpression expression)
        {
            if (!_locals.TryGetValue(expression, out var distance))
            {
                return _globals.Get(token);
            }

            return _environment.GetAt(distance, token);
        }
    }
}