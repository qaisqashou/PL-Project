using BudgetLang.Ast;

namespace BudgetLang.Syntax;

public sealed class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private int _position;

    public Parser(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens;
    }

    public ProgramNode ParseProgram()
    {
        var functions = new List<FunctionDeclarationNode>();
        var statements = new List<StatementNode>();

        while (!Check(TokenKind.EndOfFile))
        {
            if (IsFunctionDeclarationStart())
            {
                functions.Add(ParseFunctionDeclaration());
            }
            else
            {
                statements.Add(ParseStatement());
            }
        }

        return new ProgramNode(functions, statements);
    }

    private FunctionDeclarationNode ParseFunctionDeclaration()
    {
        var returnType = ParseType();
        var name = Consume(TokenKind.Identifier, "Expected function name.").Lexeme;
        Consume(TokenKind.LeftParen, "Expected '(' after function name.");

        var parameters = new List<ParameterNode>();
        if (!Check(TokenKind.RightParen))
        {
            do
            {
                var parameterType = ParseType();
                var parameterName = Consume(TokenKind.Identifier, "Expected parameter name.").Lexeme;
                parameters.Add(new ParameterNode(parameterType, parameterName));
            }
            while (Match(TokenKind.Comma));
        }

        Consume(TokenKind.RightParen, "Expected ')' after parameter list.");
        var body = ParseBlock();
        return new FunctionDeclarationNode(returnType, name, parameters, body);
    }

    private StatementNode ParseStatement()
    {
        if (IsTypeStart(Current().Kind))
        {
            return ParseVariableDeclaration();
        }

        if (Match(TokenKind.IfKeyword))
        {
            return ParseIfStatement();
        }

        if (Match(TokenKind.ForeachKeyword))
        {
            return ParseForeachStatement();
        }

        if (Match(TokenKind.SwitchKeyword))
        {
            return ParseSwitchStatement();
        }

        if (Match(TokenKind.ReturnKeyword))
        {
            var expression = ParseExpression();
            Consume(TokenKind.Semicolon, "Expected ';' after return statement.");
            return new ReturnStatementNode(expression);
        }

        if (Match(TokenKind.BudgetKeyword))
        {
            var label = Consume(TokenKind.StringLiteral, "Expected string literal after 'budget'.").Lexeme;
            var expression = ParseExpression();
            Consume(TokenKind.Semicolon, "Expected ';' after budget statement.");
            return new BudgetStatementNode(label, expression);
        }

        if (Match(TokenKind.TrackKeyword))
        {
            var expression = ParseExpression();
            Consume(TokenKind.Semicolon, "Expected ';' after track statement.");
            return new TrackStatementNode(expression);
        }

        if (Check(TokenKind.Identifier))
        {
            return ParseIdentifierLedStatement();
        }

        throw Error(Current(), $"Unexpected token '{Current().Lexeme}' at the start of a statement.");
    }

    private StatementNode ParseVariableDeclaration()
    {
        var type = ParseType();
        var name = Consume(TokenKind.Identifier, "Expected variable name.").Lexeme;
        ExpressionNode? initializer = null;

        if (Match(TokenKind.Assign))
        {
            initializer = ParseExpression();
        }

        Consume(TokenKind.Semicolon, "Expected ';' after variable declaration.");
        return new VariableDeclarationNode(type, name, initializer);
    }

    private StatementNode ParseIfStatement()
    {
        Consume(TokenKind.LeftParen, "Expected '(' after 'if'.");
        var condition = ParseExpression();
        Consume(TokenKind.RightParen, "Expected ')' after if condition.");

        var thenBlock = ParseBlock();
        BlockStatementNode? elseBlock = null;

        if (Match(TokenKind.ElseKeyword))
        {
            elseBlock = ParseBlock();
        }

        return new IfStatementNode(condition, thenBlock, elseBlock);
    }

    private StatementNode ParseForeachStatement()
    {
        Consume(TokenKind.LeftParen, "Expected '(' after 'foreach'.");
        var itemType = ParseType();
        var itemName = Consume(TokenKind.Identifier, "Expected foreach variable name.").Lexeme;
        Consume(TokenKind.InKeyword, "Expected 'in' in foreach statement.");
        var source = ParseExpression();
        Consume(TokenKind.RightParen, "Expected ')' after foreach header.");
        var body = ParseBlock();
        return new ForeachStatementNode(itemType, itemName, source, body);
    }

    private StatementNode ParseSwitchStatement()
    {
        Consume(TokenKind.LeftParen, "Expected '(' after 'switch'.");
        var expression = ParseExpression();
        Consume(TokenKind.RightParen, "Expected ')' after switch expression.");
        Consume(TokenKind.LeftBrace, "Expected '{' to start switch body.");

        var cases = new List<CaseClauseNode>();
        DefaultClauseNode? defaultClause = null;

        while (!Check(TokenKind.RightBrace))
        {
            if (Match(TokenKind.CaseKeyword))
            {
                var value = ParseCaseLiteral();
                Consume(TokenKind.Colon, "Expected ':' after case value.");
                cases.Add(new CaseClauseNode(value, ParseStatementListUntil(TokenKind.CaseKeyword, TokenKind.DefaultKeyword, TokenKind.RightBrace)));
                continue;
            }

            if (Match(TokenKind.DefaultKeyword))
            {
                Consume(TokenKind.Colon, "Expected ':' after 'default'.");
                defaultClause = new DefaultClauseNode(ParseStatementListUntil(TokenKind.RightBrace));
                break;
            }

            throw Error(Current(), "Expected 'case', 'default', or '}' inside switch statement.");
        }

        Consume(TokenKind.RightBrace, "Expected '}' after switch statement.");
        return new SwitchStatementNode(expression, cases, defaultClause);
    }

    private StatementNode ParseIdentifierLedStatement()
    {
        if (Peek(1).Kind == TokenKind.LeftParen)
        {
            var call = ParseFunctionCall();
            Consume(TokenKind.Semicolon, "Expected ';' after function call.");
            return new CallStatementNode(call);
        }

        var name = Consume(TokenKind.Identifier, "Expected identifier.").Lexeme;

        if (Match(TokenKind.Assign))
        {
            var expression = ParseExpression();
            Consume(TokenKind.Semicolon, "Expected ';' after assignment.");
            return new AssignmentStatementNode(name, expression);
        }

        Consume(TokenKind.Dot, "Expected '=' or '.' after identifier.");
        var fieldName = Consume(TokenKind.Identifier, "Expected field name after '.'.").Lexeme;
        Consume(TokenKind.Assign, "Expected '=' after field name.");
        var fieldValue = ParseExpression();
        Consume(TokenKind.Semicolon, "Expected ';' after field assignment.");
        return new FieldAssignmentStatementNode(name, fieldName, fieldValue);
    }

    private BlockStatementNode ParseBlock()
    {
        Consume(TokenKind.LeftBrace, "Expected '{' to start block.");
        var statements = new List<StatementNode>();

        while (!Check(TokenKind.RightBrace))
        {
            statements.Add(ParseStatement());
        }

        Consume(TokenKind.RightBrace, "Expected '}' after block.");
        return new BlockStatementNode(statements);
    }

    private ExpressionNode ParseExpression() => ParseEquality();

    private ExpressionNode ParseEquality()
    {
        var expression = ParseRelational();

        while (Match(TokenKind.EqualEqual, TokenKind.NotEqual))
        {
            var operatorKind = Previous().Kind;
            var right = ParseRelational();
            expression = new BinaryExpressionNode(expression, operatorKind, right);
        }

        return expression;
    }

    private ExpressionNode ParseRelational()
    {
        var expression = ParseAdditive();

        while (Match(TokenKind.Less, TokenKind.LessEqual, TokenKind.Greater, TokenKind.GreaterEqual))
        {
            var operatorKind = Previous().Kind;
            var right = ParseAdditive();
            expression = new BinaryExpressionNode(expression, operatorKind, right);
        }

        return expression;
    }

    private ExpressionNode ParseAdditive()
    {
        var expression = ParseMultiplicative();

        while (Match(TokenKind.Plus, TokenKind.Minus))
        {
            var operatorKind = Previous().Kind;
            var right = ParseMultiplicative();
            expression = new BinaryExpressionNode(expression, operatorKind, right);
        }

        return expression;
    }

    private ExpressionNode ParseMultiplicative()
    {
        var expression = ParsePostfix();

        while (Match(TokenKind.Star, TokenKind.Slash))
        {
            var operatorKind = Previous().Kind;
            var right = ParsePostfix();
            expression = new BinaryExpressionNode(expression, operatorKind, right);
        }

        return expression;
    }

    private ExpressionNode ParsePostfix()
    {
        var expression = ParsePrimary();

        while (Match(TokenKind.Dot))
        {
            var fieldName = Consume(TokenKind.Identifier, "Expected field name after '.'.").Lexeme;
            expression = new FieldAccessExpressionNode(expression, fieldName);
        }

        return expression;
    }

    private ExpressionNode ParsePrimary()
    {
        if (Match(TokenKind.MoneyLiteral))
        {
            var amount = decimal.Parse(Previous().Lexeme[1..]);
            return new LiteralExpressionNode(amount, TypeSymbol.Money);
        }

        if (Match(TokenKind.IntLiteral))
        {
            return new LiteralExpressionNode(int.Parse(Previous().Lexeme), TypeSymbol.Int);
        }

        if (Match(TokenKind.StringLiteral))
        {
            return new LiteralExpressionNode(Previous().Lexeme, TypeSymbol.String);
        }

        if (Check(TokenKind.Identifier) && Peek(1).Kind == TokenKind.LeftParen)
        {
            return ParseFunctionCall();
        }

        if (Match(TokenKind.Identifier))
        {
            return new IdentifierExpressionNode(Previous().Lexeme);
        }

        if (Match(TokenKind.LeftBracket))
        {
            var elements = new List<ExpressionNode>();
            if (!Check(TokenKind.RightBracket))
            {
                do
                {
                    elements.Add(ParseExpression());
                }
                while (Match(TokenKind.Comma));
            }

            Consume(TokenKind.RightBracket, "Expected ']' after array literal.");
            return new ArrayLiteralExpressionNode(elements);
        }

        if (Match(TokenKind.LeftParen))
        {
            var expression = ParseExpression();
            Consume(TokenKind.RightParen, "Expected ')' after expression.");
            return expression;
        }

        throw Error(Current(), $"Unexpected token '{Current().Lexeme}' in expression.");
    }

    private FunctionCallExpressionNode ParseFunctionCall()
    {
        var name = Consume(TokenKind.Identifier, "Expected function name.").Lexeme;
        Consume(TokenKind.LeftParen, "Expected '(' after function name.");

        var arguments = new List<ExpressionNode>();
        if (!Check(TokenKind.RightParen))
        {
            do
            {
                arguments.Add(ParseExpression());
            }
            while (Match(TokenKind.Comma));
        }

        Consume(TokenKind.RightParen, "Expected ')' after argument list.");
        return new FunctionCallExpressionNode(name, arguments);
    }

    private LiteralExpressionNode ParseCaseLiteral()
    {
        if (Match(TokenKind.IntLiteral))
        {
            return new LiteralExpressionNode(int.Parse(Previous().Lexeme), TypeSymbol.Int);
        }

        if (Match(TokenKind.StringLiteral))
        {
            return new LiteralExpressionNode(Previous().Lexeme, TypeSymbol.String);
        }

        throw Error(Current(), "Case values must be int or string literals.");
    }

    private List<StatementNode> ParseStatementListUntil(params TokenKind[] terminators)
    {
        var statements = new List<StatementNode>();

        while (!terminators.Contains(Current().Kind))
        {
            statements.Add(ParseStatement());
        }

        return statements;
    }

    private TypeSymbol ParseType()
    {
        if (Match(TokenKind.MoneyKeyword))
        {
            return TypeSymbol.Money;
        }

        if (Match(TokenKind.IntKeyword))
        {
            return TypeSymbol.Int;
        }

        if (Match(TokenKind.StringKeyword))
        {
            return TypeSymbol.String;
        }

        if (Match(TokenKind.TransactionKeyword))
        {
            if (Match(TokenKind.LeftBracket))
            {
                Consume(TokenKind.RightBracket, "Expected ']' after '[' in array type.");
                return TypeSymbol.TransactionArray;
            }

            return TypeSymbol.Transaction;
        }

        throw Error(Current(), "Expected a type.");
    }

    private bool IsFunctionDeclarationStart()
    {
        if (!IsTypeStart(Current().Kind))
        {
            return false;
        }

        var identifierOffset = 1;
        var nextOffset = 2;

        if (Current().Kind == TokenKind.TransactionKeyword &&
            Peek(1).Kind == TokenKind.LeftBracket &&
            Peek(2).Kind == TokenKind.RightBracket)
        {
            identifierOffset = 3;
            nextOffset = 4;
        }

        return Peek(identifierOffset).Kind == TokenKind.Identifier &&
               Peek(nextOffset).Kind == TokenKind.LeftParen;
    }

    private static bool IsTypeStart(TokenKind kind) =>
        kind is TokenKind.MoneyKeyword
            or TokenKind.IntKeyword
            or TokenKind.StringKeyword
            or TokenKind.TransactionKeyword;

    private bool Match(params TokenKind[] kinds)
    {
        foreach (var kind in kinds)
        {
            if (Check(kind))
            {
                _position++;
                return true;
            }
        }

        return false;
    }

    private Token Consume(TokenKind kind, string message)
    {
        if (Check(kind))
        {
            return _tokens[_position++];
        }

        throw Error(Current(), message);
    }

    private bool Check(TokenKind kind) => Current().Kind == kind;

    private Token Current() => _tokens[_position];

    private Token Previous() => _tokens[_position - 1];

    private Token Peek(int offset)
    {
        var index = Math.Min(_position + offset, _tokens.Count - 1);
        return _tokens[index];
    }

    private static Exception Error(Token token, string message)
    {
        var displayLexeme = token.Kind == TokenKind.EndOfFile ? "end of file" : token.Lexeme;
        var normalizedMessage = message.Replace("''", $"'{displayLexeme}'");
        return new InvalidOperationException($"Parser error at line {token.Line}, column {token.Column}: {normalizedMessage}");
    }
}
