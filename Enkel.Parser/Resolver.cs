using System.Collections.Generic;
using Enkel.Core.Common;
using Enkel.Core.Exceptions;
using Enkel.Core.Interpreter;
using Enkel.Core.Interpreter.Interfaces;
using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Expressions;
using Enkel.Core.Parser.Interfaces;
using Enkel.Core.Parser.Statements;

namespace Enkel.Parser
{
    public class Resolver : IExprVisitor<Unit>, IStatementVisitor<Unit>
    {
        private readonly IInterpreter _interpreter;

        private readonly Stack<Dictionary<string, bool>> _scopes = new Stack<Dictionary<string, bool>>();
        private FunctionType _currentFunction = FunctionType.None;

        public Resolver(IInterpreter interpreter)
        {
            _interpreter = interpreter;
        }


        public Unit VisitBinaryExpr(BinaryExpression expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);

            return Unit.Value;
        }

        public Unit VisitGroupingExpr(GroupingExpression expr)
        {
            Resolve(expr.Expression);
            return Unit.Value;
        }

        public Unit VisitLiteralExpr(LiteralExpression expr)
        {
            return Unit.Value;
        }

        public Unit VisitUnaryExpr(UnaryExpression expr)
        {
            Resolve(expr.Right);
            return Unit.Value;
        }

        public Unit VisitVarExpr(VariableExpression expr)
        {
            if (_scopes.Count != 0)
            {
                var scope = _scopes.Peek();
                if (scope.TryGetValue(expr.Token.Lexeme, out var processed))
                {
                    if (!processed)
                    {
                        throw new ResolveException("Can't initialize local variable with itself");
                    }
                }
            }

            ResolveLocal(expr, expr.Token);
            return Unit.Value;
        }

        public Unit VisitAssignmentExpr(AssignmentExpression expr)
        {
            Resolve(expr.Expression);
            ResolveLocal(expr, expr.Target);

            return Unit.Value;
        }

        public Unit VisitLogicalExpr(LogicalExpression expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return Unit.Value;
        }

        public Unit VisitCallExpr(CallExpression expr)
        {
            Resolve(expr.Callee);

            foreach (var arg in expr.Arguments)
            {
                Resolve(arg);
            }

            return Unit.Value;
        }

        public Unit VisitExpressionStatement(ExpressionStatement statement)
        {
            Resolve(statement.Expression);
            return Unit.Value;
        }

        public Unit VisitVarStatement(VarStatement statement)
        {
            Declare(statement.Token);

            if (statement.Initializer != null)
            {
                Resolve(statement.Initializer);
            }

            Define(statement.Token);
            return Unit.Value;
        }

        public Unit VisitBlockStatement(BlockStatement statement)
        {
            BeginScope();
            Resolve(statement.Statements);
            EndScope();

            return Unit.Value;
        }

        public Unit VisitIfStatement(IfStatement statement)
        {
            Resolve(statement.Condition);
            Resolve(statement.Body);
            if (statement.ElseBranch != null)
            {
                Resolve(statement.ElseBranch);
            }

            return Unit.Value;
        }

        public Unit VisitWhileStatement(WhileStatement statement)
        {
            Resolve(statement.Condition);
            Resolve(statement.Body);

            return Unit.Value;
        }

        public Unit VisitFunctionStatement(FunctionStatement statement)
        {
            Declare(statement.Name);
            Define(statement.Name);

            ResolveFunction(statement, FunctionType.Function);
            return Unit.Value;
        }

        public Unit VisitReturnStatement(ReturnStatement statement)
        {
            if (_currentFunction == FunctionType.None)
            {
                throw new ResolveException("Unexpected return outside a function");
            }

            if (statement.Value != null)
            {
                Resolve(statement.Value);
            }

            return Unit.Value;
        }

        public void Resolve(IEnumerable<IStatement> statements)
        {
            foreach (var statement in statements)
            {
                Resolve(statement);
            }
        }

        private void Resolve(IStatement statement)
        {
            statement.Accept(this);
        }

        private void Resolve(IExpression expression)
        {
            expression.Accept(this);
        }

        private void ResolveLocal(IExpression expression, IToken token)
        {
            var i = 0;
            foreach (var scope in _scopes.ToArray())
            {
                if (scope.ContainsKey(token.Lexeme))
                {
                    _interpreter.Resolve(expression, i);
                    return;
                }

                ++i;
            }
        }

        private void ResolveFunction(FunctionStatement function, FunctionType type)
        {
            var enclosingFunc = _currentFunction;
            _currentFunction = type;

            BeginScope();
            foreach (var param in function.Params)
            {
                Declare(param);
                Define(param);
            }

            Resolve(function.Body);
            EndScope();

            _currentFunction = enclosingFunc;
        }

        private void BeginScope()
        {
            _scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            if (_scopes.Count == 0)
            {
                throw new ResolveException("Can't close the scope");
            }

            _scopes.Pop();
        }

        private void Declare(IToken token)
        {
            if (_scopes.Count == 0)
            {
                return;
            }

            var scope = _scopes.Peek();
            if (!scope.TryAdd(token.Lexeme, false))
            {
                throw new ResolveException($"Attempt to redeclare an identifier: {token.Lexeme}");
            }
        }

        private void Define(IToken token)
        {
            if (_scopes.Count == 0)
            {
                return;
            }

            var scope = _scopes.Peek();
            if (!scope.ContainsKey(token.Lexeme))
            {
                throw new ResolveException("Attempt to define an undeclared variable!");
            }

            scope[token.Lexeme] = true;
        }
    }
}