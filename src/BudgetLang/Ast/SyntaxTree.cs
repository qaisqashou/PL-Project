using BudgetLang.Syntax;

namespace BudgetLang.Ast;

public sealed record TypeSymbol(string Name)
{
    public static readonly TypeSymbol Money = new("money");
    public static readonly TypeSymbol Int = new("int");
    public static readonly TypeSymbol String = new("string");
    public static readonly TypeSymbol Transaction = new("transaction");
    public static readonly TypeSymbol TransactionArray = new("transaction[]");
    public static readonly TypeSymbol Bool = new("bool");
    public static readonly TypeSymbol Void = new("void");

    public override string ToString() => Name;
}

public sealed record ProgramNode(
    IReadOnlyList<FunctionDeclarationNode> Functions,
    IReadOnlyList<StatementNode> Statements);

public sealed record FunctionDeclarationNode(
    TypeSymbol ReturnType,
    string Name,
    IReadOnlyList<ParameterNode> Parameters,
    BlockStatementNode Body);

public sealed record ParameterNode(TypeSymbol Type, string Name);

public abstract record StatementNode;

public sealed record BlockStatementNode(IReadOnlyList<StatementNode> Statements) : StatementNode;

public sealed record VariableDeclarationNode(
    TypeSymbol Type,
    string Name,
    ExpressionNode? Initializer) : StatementNode;

public sealed record AssignmentStatementNode(
    string Name,
    ExpressionNode Expression) : StatementNode;

public sealed record FieldAssignmentStatementNode(
    string TargetName,
    string FieldName,
    ExpressionNode Expression) : StatementNode;

public sealed record IfStatementNode(
    ExpressionNode Condition,
    BlockStatementNode ThenBlock,
    BlockStatementNode? ElseBlock) : StatementNode;

public sealed record ForeachStatementNode(
    TypeSymbol ItemType,
    string ItemName,
    ExpressionNode Source,
    BlockStatementNode Body) : StatementNode;

public sealed record SwitchStatementNode(
    ExpressionNode Expression,
    IReadOnlyList<CaseClauseNode> Cases,
    DefaultClauseNode? DefaultClause) : StatementNode;

public sealed record CaseClauseNode(
    LiteralExpressionNode Value,
    IReadOnlyList<StatementNode> Statements);

public sealed record DefaultClauseNode(IReadOnlyList<StatementNode> Statements);

public sealed record ReturnStatementNode(ExpressionNode Expression) : StatementNode;

public sealed record BudgetStatementNode(string Label, ExpressionNode Expression) : StatementNode;

public sealed record TrackStatementNode(ExpressionNode Expression) : StatementNode;

public sealed record CallStatementNode(FunctionCallExpressionNode Call) : StatementNode;

public abstract record ExpressionNode;

public sealed record LiteralExpressionNode(object Value, TypeSymbol Type) : ExpressionNode;

public sealed record IdentifierExpressionNode(string Name) : ExpressionNode;

public sealed record ArrayLiteralExpressionNode(IReadOnlyList<ExpressionNode> Elements) : ExpressionNode;

public sealed record FunctionCallExpressionNode(
    string Name,
    IReadOnlyList<ExpressionNode> Arguments) : ExpressionNode;

public sealed record FieldAccessExpressionNode(
    ExpressionNode Target,
    string FieldName) : ExpressionNode;

public sealed record BinaryExpressionNode(
    ExpressionNode Left,
    TokenKind Operator,
    ExpressionNode Right) : ExpressionNode;
