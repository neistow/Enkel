using System.Collections.Generic;
using Enkel.Core.Lexer;
using Enkel.Core.Lexer.Interfaces;
using Enkel.Core.Parser.Expressions;
using Enkel.Core.Parser.Interfaces;
using Enkel.Core.Parser.Statements;

namespace Enkel.Parser
{
    public class EnkelParser : IParser
    {
        private readonly IList<IToken> _tokens;
        private int _current;

        public EnkelParser(IList<IToken> tokens)
        {
            _tokens = tokens;
        }

        public IEnumerable<IStatement> Parse()
        {
            var statements = new List<IStatement>();
            while (!IsAtEnd)
            {
                statements.Add(ParseDeclaration());
            }

            return statements;
        }

        private IStatement ParseDeclaration()
        {
            if (CurrentTokenMatches(TokenType.Var))
            {
                return ParseVarDeclaration();
            }

            if (CurrentTokenMatches(TokenType.Func))
            {
                return ParseFunction();
            }

            if (CurrentTokenMatches(TokenType.Class))
            {
                return ParseClass();
            }

            return ParseStatement();
        }

        private IStatement ParseVarDeclaration()
        {
            if (!TryConsume(TokenType.Identifier, out var token))
            {
                throw new ParseException("Expected variable identifier", CurrentToken);
            }

            IExpression initializer = null;
            if (CurrentTokenMatches(TokenType.Equal))
            {
                initializer = ParseExpression();
            }

            if (!TryConsume(TokenType.Semicolon, out _))
            {
                throw new ParseException("Expected ';' after variable declaration", CurrentToken);
            }

            return new VarStatement(token, initializer);
        }

        private FunctionStatement ParseFunction()
        {
            if (!TryConsume(TokenType.Identifier, out var name))
            {
                throw new ParseException("Expected a function name", CurrentToken);
            }

            if (!TryConsume(TokenType.LeftParen, out _))
            {
                throw new ParseException("Expected '(' after function declaration", CurrentToken);
            }

            var parameters = new List<IToken>();
            if (!CurrentTokenIsOfType(TokenType.RightParen))
            {
                do
                {
                    if (!TryConsume(TokenType.Identifier, out var paramName))
                    {
                        throw new ParseException("Expected a parameter name", CurrentToken);
                    }

                    parameters.Add(paramName);
                } while (CurrentTokenMatches(TokenType.Comma));
            }

            if (!TryConsume(TokenType.RightParen, out _))
            {
                throw new ParseException("Expected ')' after function params", CurrentToken);
            }

            if (!TryConsume(TokenType.LeftBrace, out _))
            {
                throw new ParseException("Expected '{' before function body", CurrentToken);
            }

            var body = ParseBlock();
            return new FunctionStatement(name, parameters, body);
        }

        private IStatement ParseClass()
        {
            if (!TryConsume(TokenType.Identifier, out var name))
            {
                throw new ParseException("Expected class name", CurrentToken);
            }

            if (!TryConsume(TokenType.LeftBrace, out _))
            {
                throw new ParseException("Expected '{' before class body", CurrentToken);
            }

            var methods = new List<FunctionStatement>();
            while (!CurrentTokenIsOfType(TokenType.RightBrace) && !IsAtEnd)
            {
                methods.Add(ParseFunction());
            }

            if (!TryConsume(TokenType.RightBrace, out _))
            {
                throw new ParseException("Expected '}' after class body", CurrentToken);
            }

            return new ClassStatement(name, methods);
        }

        private IStatement ParseStatement()
        {
            if (CurrentTokenMatches(TokenType.If))
            {
                return ParseIfStatement();
            }

            if (CurrentTokenMatches(TokenType.While))
            {
                return ParseWhileStatement();
            }

            if (CurrentTokenMatches(TokenType.For))
            {
                return ParseForStatement();
            }

            if (CurrentTokenMatches(TokenType.Return))
            {
                return ParseReturnStatement();
            }

            if (CurrentTokenMatches(TokenType.LeftBrace))
            {
                return new BlockStatement(ParseBlock());
            }

            return ParseExpressionStatement();
        }

        private IEnumerable<IStatement> ParseBlock()
        {
            var statements = new List<IStatement>();

            while (!CurrentTokenIsOfType(TokenType.RightBrace) && !IsAtEnd)
            {
                statements.Add(ParseDeclaration());
            }

            if (!TryConsume(TokenType.RightBrace, out _))
            {
                throw new ParseException("Expected '}' after block statement", CurrentToken);
            }

            return statements;
        }

        private IStatement ParseIfStatement()
        {
            if (!TryConsume(TokenType.LeftParen, out _))
            {
                throw new ParseException("Expected '(' after if statement", CurrentToken);
            }

            var condition = ParseExpression();

            if (!TryConsume(TokenType.RightParen, out _))
            {
                throw new ParseException("Expected enclosing ')' after expression in if statement.", CurrentToken);
            }

            if (!TryConsume(TokenType.LeftBrace, out _))
            {
                throw new ParseException("Expected '{' after if condition.", CurrentToken);
            }

            var body = new BlockStatement(ParseBlock());

            if (!CurrentTokenMatches(TokenType.Else))
            {
                return new IfStatement(condition, body, null);
            }

            IStatement elseBranch;
            if (CurrentTokenMatches(TokenType.If))
            {
                elseBranch = ParseIfStatement();
            }
            else
            {
                if (!TryConsume(TokenType.LeftBrace, out _))
                {
                    throw new ParseException("Expected '{' after else condition", CurrentToken);
                }

                elseBranch = new BlockStatement(ParseBlock());
            }

            return new IfStatement(condition, body, elseBranch);
        }

        private IStatement ParseWhileStatement()
        {
            if (!TryConsume(TokenType.LeftParen, out _))
            {
                throw new ParseException("Expected '(' after if statement", CurrentToken);
            }

            var condition = ParseExpression();

            if (!TryConsume(TokenType.RightParen, out _))
            {
                throw new ParseException("Expected enclosing ')' after expression in if statement", CurrentToken);
            }

            if (!TryConsume(TokenType.LeftBrace, out _))
            {
                throw new ParseException("Expected '{' after if condition", CurrentToken);
            }

            var body = new BlockStatement(ParseBlock());
            return new WhileStatement(condition, body);
        }

        private IStatement ParseForStatement()
        {
            if (!TryConsume(TokenType.LeftParen, out _))
            {
                throw new ParseException("Expected '(' after for statement", CurrentToken);
            }

            IStatement initializer;
            if (CurrentTokenMatches(TokenType.Semicolon))
            {
                initializer = null;
            }
            else if (CurrentTokenMatches(TokenType.Var))
            {
                initializer = ParseVarDeclaration();
            }
            else
            {
                initializer = ParseExpressionStatement();
            }

            IExpression condition = null;
            if (!CurrentTokenIsOfType(TokenType.Semicolon))
            {
                condition = ParseExpression();
            }

            condition ??= new LiteralExpression(true);

            if (!TryConsume(TokenType.Semicolon, out _))
            {
                throw new ParseException("Expected ';' after for loop condition", CurrentToken);
            }

            IExpression action = null;
            if (!CurrentTokenIsOfType(TokenType.RightParen))
            {
                action = ParseExpression();
            }

            if (!TryConsume(TokenType.RightParen, out _))
            {
                throw new ParseException("Expected ')' after for loop", CurrentToken);
            }

            if (!TryConsume(TokenType.LeftBrace, out _))
            {
                throw new ParseException("Expected '{' in for loop body", CurrentToken);
            }

            IStatement body = new BlockStatement(ParseBlock());
            if (action != null)
            {
                body = new BlockStatement(new[]
                {
                    body,
                    new ExpressionStatement(action)
                });
            }

            body = new WhileStatement(condition, (BlockStatement) body);

            if (initializer != null)
            {
                body = new BlockStatement(new[] {initializer, body});
            }

            return body;
        }

        private IStatement ParseReturnStatement()
        {
            var token = PreviousToken;
            IExpression value = null;
            if (!CurrentTokenIsOfType(TokenType.Semicolon))
            {
                value = ParseExpression();
            }

            if (!TryConsume(TokenType.Semicolon, out _))
            {
                throw new ParseException("Expected ';' after return statement", CurrentToken);
            }

            return new ReturnStatement(token, value);
        }

        private IStatement ParseExpressionStatement()
        {
            var expr = ParseExpression();
            if (!TryConsume(TokenType.Semicolon, out _))
            {
                throw new ParseException("Expected ';' after expression", CurrentToken);
            }

            return new ExpressionStatement(expr);
        }

        private IExpression ParseExpression()
        {
            return ParseAssignment();
        }

        private IExpression ParseAssignment()
        {
            var expr = ParseOr();

            if (CurrentTokenMatches(TokenType.Equal))
            {
                var value = ParseAssignment();

                if (expr is VariableExpression variableExpr)
                {
                    return new AssignmentExpression(variableExpr.Token, value);
                }

                if (expr is GetExpression getExpression)
                {
                    return new SetExpression(getExpression.Object, getExpression.Token, value);
                }

                throw new ParseException("Invalid assignment target", CurrentToken);
            }

            return expr;
        }

        private IExpression ParseOr()
        {
            var expr = ParseAnd();

            while (CurrentTokenMatches(TokenType.Or))
            {
                var op = PreviousToken;
                var right = ParseAnd();
                expr = new LogicalExpression(expr, op, right);
            }

            return expr;
        }

        private IExpression ParseAnd()
        {
            var expr = ParseEquality();

            while (CurrentTokenMatches(TokenType.And))
            {
                var op = PreviousToken;
                var right = ParseEquality();
                expr = new LogicalExpression(expr, op, right);
            }

            return expr;
        }

        private IExpression ParseEquality()
        {
            var expr = ParseComparison();
            while (CurrentTokenMatches(TokenType.BangEqual, TokenType.DoubleEqual))
            {
                var op = PreviousToken;
                var right = ParseComparison();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private IExpression ParseComparison()
        {
            var expr = ParseTerm();
            while (CurrentTokenMatches(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            {
                var op = PreviousToken;
                var right = ParseTerm();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private IExpression ParseTerm()
        {
            var expr = ParseFactor();

            while (CurrentTokenMatches(TokenType.Minus, TokenType.Plus))
            {
                var op = PreviousToken;
                var right = ParseFactor();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private IExpression ParseFactor()
        {
            var expr = ParseUnary();

            while (CurrentTokenMatches(TokenType.Slash, TokenType.Star))
            {
                var op = PreviousToken;
                var right = ParseUnary();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private IExpression ParseUnary()
        {
            if (CurrentTokenMatches(TokenType.Bang, TokenType.Minus))
            {
                var op = PreviousToken;
                var right = ParseUnary();
                return new UnaryExpression(op, right);
            }

            return ParseCall();
        }

        private IExpression ParseCall()
        {
            var expr = ParsePrimary();

            while (true)
            {
                if (CurrentTokenMatches(TokenType.LeftParen))
                {
                    expr = ParseCall(expr);
                }
                else if (CurrentTokenMatches(TokenType.Dot))
                {
                    if (!TryConsume(TokenType.Identifier, out var token))
                    {
                        throw new ParseException("Expected prop name after '.'", CurrentToken);
                    }

                    expr = new GetExpression(token, expr);
                }
                else
                {
                    return expr;
                }
            }
        }

        private IExpression ParseCall(IExpression callee)
        {
            var args = new List<IExpression>();
            if (!CurrentTokenIsOfType(TokenType.RightParen))
            {
                do
                {
                    args.Add(ParseExpression());
                } while (CurrentTokenMatches(TokenType.Comma));
            }

            if (!TryConsume(TokenType.RightParen, out var parenthesis))
            {
                throw new ParseException("Expected ')' after arguments", CurrentToken);
            }

            return new CallExpression(callee, parenthesis, args);
        }

        private IExpression ParsePrimary()
        {
            if (CurrentTokenMatches(TokenType.False))
            {
                return new LiteralExpression(false);
            }

            if (CurrentTokenMatches(TokenType.True))
            {
                return new LiteralExpression(true);
            }

            if (CurrentTokenMatches(TokenType.None))
            {
                return new LiteralExpression(null);
            }

            if (CurrentTokenMatches(TokenType.Number, TokenType.String))
            {
                return new LiteralExpression(PreviousToken.Literal);
            }

            if (CurrentTokenMatches(TokenType.This))
            {
                return new ThisExpression(PreviousToken);
            }

            if (CurrentTokenMatches(TokenType.LeftParen))
            {
                var expr = ParseExpression();
                if (!TryConsume(TokenType.RightParen, out _))
                {
                    throw new ParseException("Expected ')' after expression", CurrentToken);
                }

                return new GroupingExpression(expr);
            }

            if (CurrentTokenMatches(TokenType.Identifier))
            {
                return new VariableExpression(PreviousToken);
            }

            throw new ParseException("Expected an expression", CurrentToken);
        }

        private bool TryConsume(TokenType type, out IToken token)
        {
            if (CurrentTokenIsOfType(type))
            {
                token = Advance();
                return true;
            }

            token = null;
            return false;
        }

        private bool CurrentTokenMatches(params TokenType[] typesToMatch)
        {
            foreach (var tokenType in typesToMatch)
            {
                if (CurrentTokenIsOfType(tokenType))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool CurrentTokenIsOfType(TokenType type)
        {
            if (IsAtEnd)
            {
                return false;
            }

            return CurrentToken.Type == type;
        }

        private IToken Advance()
        {
            if (!IsAtEnd)
            {
                _current++;
            }

            return PreviousToken;
        }

        private bool IsAtEnd => CurrentToken.Type == TokenType.Eof;

        private IToken CurrentToken => _tokens[_current];

        private IToken PreviousToken => _current == 0 ? null : _tokens[_current - 1];
    }
}