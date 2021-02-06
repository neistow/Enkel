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

        public object VisitBinaryExpression(BinaryExpression expression)
        {
            var left = Evaluate(expression.Left);
            var right = Evaluate(expression.Right);

            switch (expression.Operator.Type)
            {
                case TokenType.Minus:
                    EnsureOperandsAreNumbers(expression.Operator, left, right);
                    return Convert.ToDouble(left) - Convert.ToDouble(right);
                case TokenType.Plus:
                    return left switch
                    {
                        double d1 when right is double d2 => Convert.ToDouble(d1) + Convert.ToDouble(d2),
                        string str1 when right is string str2 => Convert.ToString(str1) + Convert.ToString(str2),
                        _ => throw new EnkelRuntimeException(
                            $"{expression.Operator}: Operands must be either strings or numbers", expression.Operator)
                    };
                case TokenType.Slash:
                    EnsureOperandsAreNumbers(expression.Operator, left, right);

                    var first = Convert.ToDouble(left);
                    var second = Convert.ToDouble(right);

                    if (second == 0)
                    {
                        throw new EnkelRuntimeException($"{expression.Operator}: Division by zero",
                            expression.Operator);
                    }

                    return first / second;
                case TokenType.Star:
                    EnsureOperandsAreNumbers(expression.Operator, left, right);
                    return Convert.ToDouble(left) * Convert.ToDouble(right);
                case TokenType.Greater:
                    EnsureOperandsAreNumbers(expression.Operator, left, right);
                    return Convert.ToDouble(left) > Convert.ToDouble(right);
                case TokenType.GreaterEqual:
                    EnsureOperandsAreNumbers(expression.Operator, left, right);
                    return Convert.ToDouble(left) >= Convert.ToDouble(right);
                case TokenType.Less:
                    EnsureOperandsAreNumbers(expression.Operator, left, right);
                    return Convert.ToDouble(left) < Convert.ToDouble(right);
                case TokenType.LessEqual:
                    EnsureOperandsAreNumbers(expression.Operator, left, right);
                    return Convert.ToDouble(left) <= Convert.ToDouble(right);
                case TokenType.BangEqual:
                    return !IsEqual(left, right);
                case TokenType.DoubleEqual:
                    return IsEqual(left, right);
            }

            return null;
        }

        public object VisitGroupingExpression(GroupingExpression expression)
        {
            return Evaluate(expression.Expression);
        }

        public object VisitLiteralExpression(LiteralExpression expression)
        {
            return expression.Value;
        }

        public object VisitUnaryExpression(UnaryExpression expression)
        {
            var right = Evaluate(expression.Right);

            switch (expression.Operator.Type)
            {
                case TokenType.Minus:
                    EnsureOperandIsNumber(expression.Operator, right);
                    return -Convert.ToDouble(right);
                case TokenType.Bang:
                    EnsureOperandIsBool(expression.Operator, right);
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
            var function = new EnkelFunction(statement, _environment, false);
            _environment.Define(statement.Token, function);
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

        public Unit VisitClassStatement(ClassStatement statement)
        {
            _environment.Define(statement.Identifier, null);

            var methods = new Dictionary<string, EnkelFunction>();
            foreach (var method in statement.Methods)
            {
                var function = new EnkelFunction(method, _environment, method.IsConstructor());
                if (!methods.TryAdd(method.Token.Lexeme, function))
                {
                    throw new EnkelRuntimeException("Can't declare a duplicate method", method.Token);
                }
            }

            var @class = new EnkelClass(statement.Identifier, methods);
            _environment.Assign(statement.Identifier, @class);

            return Unit.Value;
        }

        public object VisitVarExpression(VariableExpression expression)
        {
            return FindVariable(expression.Token, expression);
        }

        public object VisitAssignmentExpression(AssignmentExpression expression)
        {
            var value = Evaluate(expression.Expression);

            if (!_locals.TryGetValue(expression, out var distance))
            {
                _globals.Assign(expression.Target, value);
            }
            else
            {
                _environment.AssignAt(distance, expression.Target, value);
            }

            return value;
        }

        public object VisitLogicalExpression(LogicalExpression expression)
        {
            var left = Evaluate(expression.Left);

            if (!(left is bool result))
            {
                throw new EnkelRuntimeException($"{expression.Operator.Lexeme}: expression result must be a bool type",
                    expression.Operator);
            }

            if (expression.Operator.Type == TokenType.Or)
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

            return Evaluate(expression.Right);
        }

        public object VisitCallExpression(CallExpression expression)
        {
            var callee = Evaluate(expression.Callee);

            var args = expression.Arguments.Select(Evaluate).ToList();

            if (!(callee is ICallable function))
            {
                throw new EnkelRuntimeException($"{callee} is not callable", expression.Token);
            }

            if (args.Count != function.Arity)
            {
                throw new EnkelRuntimeException($"Expected {function.Arity} arguments. Got: {args.Count}",
                    expression.Token);
            }

            return function.Call(this, args);
        }

        public object VisitGetExpression(GetExpression expression)
        {
            var obj = Evaluate(expression.Object);
            if (obj is EnkelInstance instance)
            {
                return instance.Get(expression.Token);
            }
            
            throw new EnkelRuntimeException($"Can't access property {expression.Token.Lexeme}", expression.Token);
        }

        public object VisitSetExpression(SetExpression expression)
        {
            var obj = Evaluate(expression.Object);

            if (!(obj is EnkelInstance enkelInstance))
            {
                throw new EnkelRuntimeException("Can't set a field for non class instance", expression.Token);
            }

            var value = Evaluate(expression.Value);
            enkelInstance.Set(expression.Token, value);
            return value;
        }

        public object VisitThisExpression(ThisExpression expression)
        {
            return FindVariable(expression.Token, expression);
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