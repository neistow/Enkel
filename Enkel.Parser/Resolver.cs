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
        private ClassType _currentClass = ClassType.None;

        public Resolver(IInterpreter interpreter)
        {
            _interpreter = interpreter;
        }


        public Unit VisitBinaryExpression(BinaryExpression expression)
        {
            Resolve(expression.Left);
            Resolve(expression.Right);

            return Unit.Value;
        }

        public Unit VisitGroupingExpression(GroupingExpression expression)
        {
            Resolve(expression.Expression);
            return Unit.Value;
        }

        public Unit VisitLiteralExpression(LiteralExpression expression)
        {
            return Unit.Value;
        }

        public Unit VisitUnaryExpression(UnaryExpression expression)
        {
            Resolve(expression.Right);
            return Unit.Value;
        }

        public Unit VisitVarExpression(VariableExpression expression)
        {
            if (_scopes.Count != 0)
            {
                var scope = _scopes.Peek();
                if (scope.TryGetValue(expression.Token.Lexeme, out var processed))
                {
                    if (!processed)
                    {
                        throw new ResolveException("Can't initialize local variable with itself");
                    }
                }
            }

            ResolveLocal(expression, expression.Token);
            return Unit.Value;
        }

        public Unit VisitAssignmentExpression(AssignmentExpression expression)
        {
            Resolve(expression.Expression);
            ResolveLocal(expression, expression.Target);

            return Unit.Value;
        }

        public Unit VisitLogicalExpression(LogicalExpression expression)
        {
            Resolve(expression.Left);
            Resolve(expression.Right);
            return Unit.Value;
        }

        public Unit VisitCallExpression(CallExpression expression)
        {
            Resolve(expression.Callee);

            if (expression.Callee is GetExpression getExpression)
            {
                if (getExpression.Token.Lexeme == "constructor")
                {
                    throw new ResolveException("Can't call constructor directly");
                }
            }

            foreach (var arg in expression.Arguments)
            {
                Resolve(arg);
            }

            return Unit.Value;
        }

        public Unit VisitGetExpression(GetExpression expression)
        {
            Resolve(expression.Object);
            return Unit.Value;
        }

        public Unit VisitSetExpression(SetExpression expression)
        {
            Resolve(expression.Value);
            Resolve(expression.Object);

            return Unit.Value;
        }

        public Unit VisitThisExpression(ThisExpression expression)
        {
            if (_currentClass == ClassType.None)
            {
                throw new ResolveException("Can't use 'this' outside of class");
            }

            ResolveLocal(expression, expression.Token);
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
            Declare(statement.Token);
            Define(statement.Token);

            ResolveFunction(statement, FunctionType.Function);
            return Unit.Value;
        }

        public Unit VisitReturnStatement(ReturnStatement statement)
        {
            if (_currentFunction == FunctionType.None)
            {
                throw new ResolveException("Unexpected return outside a function");
            }

            if (statement.Value == null)
            {
                return Unit.Value;
            }

            if (_currentFunction == FunctionType.Constructor)
            {
                throw new ResolveException("Can't return a value from constructor");
            }

            Resolve(statement.Value);

            return Unit.Value;
        }

        public Unit VisitClassStatement(ClassStatement statement)
        {
            var enclosingClass = _currentClass;
            _currentClass = ClassType.Class;

            Declare(statement.Identifier);
            Define(statement.Identifier);

            BeginScope();
            _scopes.Peek().TryAdd("this", true);

            foreach (var method in statement.Methods)
            {
                var type = method.Token.Lexeme == "constructor" ? FunctionType.Constructor : FunctionType.Method;
                ResolveFunction(method, type);
            }

            EndScope();

            _currentClass = enclosingClass;
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