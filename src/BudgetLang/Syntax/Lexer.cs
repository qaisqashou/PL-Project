namespace BudgetLang.Syntax;

public enum TokenKind
{
    EndOfFile,
    Identifier,
    MoneyLiteral,
    IntLiteral,
    StringLiteral,
    MoneyKeyword,
    IntKeyword,
    StringKeyword,
    TransactionKeyword,
    IfKeyword,
    ElseKeyword,
    ForeachKeyword,
    InKeyword,
    ReturnKeyword,
    BudgetKeyword,
    TrackKeyword,
    SwitchKeyword,
    CaseKeyword,
    DefaultKeyword,
    LeftParen,
    RightParen,
    LeftBrace,
    RightBrace,
    LeftBracket,
    RightBracket,
    Semicolon,
    Comma,
    Colon,
    Dot,
    Assign,
    Plus,
    Minus,
    Star,
    Slash,
    EqualEqual,
    NotEqual,
    Less,
    LessEqual,
    Greater,
    GreaterEqual
}

public sealed record Token(TokenKind Kind, string Lexeme, int Line, int Column);

public sealed class Lexer
{
    private static readonly Dictionary<string, TokenKind> Keywords = new(StringComparer.Ordinal)
    {
        ["money"] = TokenKind.MoneyKeyword,
        ["int"] = TokenKind.IntKeyword,
        ["string"] = TokenKind.StringKeyword,
        ["transaction"] = TokenKind.TransactionKeyword,
        ["if"] = TokenKind.IfKeyword,
        ["else"] = TokenKind.ElseKeyword,
        ["foreach"] = TokenKind.ForeachKeyword,
        ["in"] = TokenKind.InKeyword,
        ["return"] = TokenKind.ReturnKeyword,
        ["budget"] = TokenKind.BudgetKeyword,
        ["track"] = TokenKind.TrackKeyword,
        ["switch"] = TokenKind.SwitchKeyword,
        ["case"] = TokenKind.CaseKeyword,
        ["default"] = TokenKind.DefaultKeyword
    };

    private readonly string _text;
    private readonly List<Token> _tokens = [];
    private int _position;
    private int _line = 1;
    private int _column = 1;

    public Lexer(string text)
    {
        _text = text;
    }

    public IReadOnlyList<Token> Tokenize()
    {
        while (!IsAtEnd())
        {
            SkipTrivia();

            if (IsAtEnd())
            {
                break;
            }

            var line = _line;
            var column = _column;
            var current = Current();

            if (char.IsLetter(current))
            {
                ReadIdentifierOrKeyword(line, column);
                continue;
            }

            if (char.IsDigit(current))
            {
                ReadIntLiteral(line, column);
                continue;
            }

            if (current == '$')
            {
                ReadMoneyLiteral(line, column);
                continue;
            }

            if (current == '"')
            {
                ReadStringLiteral(line, column);
                continue;
            }

            switch (current)
            {
                case '(':
                    AddSingleCharToken(TokenKind.LeftParen, line, column);
                    break;
                case ')':
                    AddSingleCharToken(TokenKind.RightParen, line, column);
                    break;
                case '{':
                    AddSingleCharToken(TokenKind.LeftBrace, line, column);
                    break;
                case '}':
                    AddSingleCharToken(TokenKind.RightBrace, line, column);
                    break;
                case '[':
                    AddSingleCharToken(TokenKind.LeftBracket, line, column);
                    break;
                case ']':
                    AddSingleCharToken(TokenKind.RightBracket, line, column);
                    break;
                case ';':
                    AddSingleCharToken(TokenKind.Semicolon, line, column);
                    break;
                case ',':
                    AddSingleCharToken(TokenKind.Comma, line, column);
                    break;
                case ':':
                    AddSingleCharToken(TokenKind.Colon, line, column);
                    break;
                case '.':
                    AddSingleCharToken(TokenKind.Dot, line, column);
                    break;
                case '+':
                    AddSingleCharToken(TokenKind.Plus, line, column);
                    break;
                case '-':
                    AddSingleCharToken(TokenKind.Minus, line, column);
                    break;
                case '*':
                    AddSingleCharToken(TokenKind.Star, line, column);
                    break;
                case '/':
                    AddSingleCharToken(TokenKind.Slash, line, column);
                    break;
                case '=':
                    AddComparisonToken('=', TokenKind.EqualEqual, TokenKind.Assign, line, column);
                    break;
                case '!':
                    AddComparisonToken('=', TokenKind.NotEqual, null, line, column);
                    break;
                case '<':
                    AddComparisonToken('=', TokenKind.LessEqual, TokenKind.Less, line, column);
                    break;
                case '>':
                    AddComparisonToken('=', TokenKind.GreaterEqual, TokenKind.Greater, line, column);
                    break;
                default:
                    throw Error(line, column, $"Unexpected character '{current}'.");
            }
        }

        _tokens.Add(new Token(TokenKind.EndOfFile, string.Empty, _line, _column));
        return _tokens;
    }

    private void SkipTrivia()
    {
        while (!IsAtEnd())
        {
            if (char.IsWhiteSpace(Current()))
            {
                Advance();
                continue;
            }

            if (Current() == '/' && Peek() == '/')
            {
                while (!IsAtEnd() && Current() != '\n')
                {
                    Advance();
                }

                continue;
            }

            break;
        }
    }

    private void ReadIdentifierOrKeyword(int line, int column)
    {
        var start = _position;

        while (!IsAtEnd() && (char.IsLetterOrDigit(Current()) || Current() == '_'))
        {
            Advance();
        }

        var lexeme = _text[start.._position];
        var kind = Keywords.GetValueOrDefault(lexeme, TokenKind.Identifier);
        _tokens.Add(new Token(kind, lexeme, line, column));
    }

    private void ReadIntLiteral(int line, int column)
    {
        var start = _position;

        while (!IsAtEnd() && char.IsDigit(Current()))
        {
            Advance();
        }

        _tokens.Add(new Token(TokenKind.IntLiteral, _text[start.._position], line, column));
    }

    private void ReadMoneyLiteral(int line, int column)
    {
        var start = _position;
        Advance();

        if (IsAtEnd() || !char.IsDigit(Current()))
        {
            throw Error(line, column, "Money literal must contain digits after '$'.");
        }

        while (!IsAtEnd() && char.IsDigit(Current()))
        {
            Advance();
        }

        _tokens.Add(new Token(TokenKind.MoneyLiteral, _text[start.._position], line, column));
    }

    private void ReadStringLiteral(int line, int column)
    {
        Advance();
        var start = _position;

        while (!IsAtEnd() && Current() != '"')
        {
            if (Current() == '\n')
            {
                throw Error(line, column, "String literal cannot span multiple lines.");
            }

            Advance();
        }

        if (IsAtEnd())
        {
            throw Error(line, column, "Unterminated string literal.");
        }

        var value = _text[start.._position];
        Advance();
        _tokens.Add(new Token(TokenKind.StringLiteral, value, line, column));
    }

    private void AddSingleCharToken(TokenKind kind, int line, int column)
    {
        _tokens.Add(new Token(kind, Current().ToString(), line, column));
        Advance();
    }

    private void AddComparisonToken(char expectedSecondChar, TokenKind pairedKind, TokenKind? singleKind, int line, int column)
    {
        var first = Current();
        Advance();

        if (!IsAtEnd() && Current() == expectedSecondChar)
        {
            var lexeme = $"{first}{Current()}";
            Advance();
            _tokens.Add(new Token(pairedKind, lexeme, line, column));
            return;
        }

        if (singleKind is null)
        {
            throw Error(line, column, $"Unexpected character '{first}'.");
        }

        _tokens.Add(new Token(singleKind.Value, first.ToString(), line, column));
    }

    private bool IsAtEnd() => _position >= _text.Length;

    private char Current() => _text[_position];

    private char Peek() => _position + 1 < _text.Length ? _text[_position + 1] : '\0';

    private void Advance()
    {
        if (_text[_position] == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }

        _position++;
    }

    private static Exception Error(int line, int column, string message) =>
        new InvalidOperationException($"Lexer error at line {line}, column {column}: {message}");
}
