using System.Text;
using BudgetLang.Syntax;

namespace BudgetLang.Ast;

public static class AstPrinter
{
    public static string Print(ProgramNode program)
    {
        var builder = new StringBuilder();
        WriteProgram(builder, program, 0);
        return builder.ToString();
    }

    private static void WriteProgram(StringBuilder builder, ProgramNode program, int indent)
    {
        WriteLine(builder, indent, "Program");

        WriteLine(builder, indent + 1, "Functions");
        foreach (var function in program.Functions)
        {
            WriteFunction(builder, function, indent + 2);
        }

        WriteLine(builder, indent + 1, "Statements");
        foreach (var statement in program.Statements)
        {
            WriteStatement(builder, statement, indent + 2);
        }
    }

    private static void WriteFunction(StringBuilder builder, FunctionDeclarationNode function, int indent)
    {
        WriteLine(builder, indent, $"Function {function.Name} : {function.ReturnType}");

        WriteLine(builder, indent + 1, "Parameters");
        foreach (var parameter in function.Parameters)
        {
            WriteLine(builder, indent + 2, $"{parameter.Name} : {parameter.Type}");
        }

        WriteLine(builder, indent + 1, "Body");
        WriteBlock(builder, function.Body, indent + 2);
    }

    private static void WriteBlock(StringBuilder builder, BlockStatementNode block, int indent)
    {
        WriteLine(builder, indent, "Block");
        foreach (var statement in block.Statements)
        {
            WriteStatement(builder, statement, indent + 1);
        }
    }

    private static void WriteStatement(StringBuilder builder, StatementNode statement, int indent)
    {
        switch (statement)
        {
            case BlockStatementNode block:
                WriteBlock(builder, block, indent);
                break;

            case VariableDeclarationNode declaration:
                WriteLine(builder, indent, $"VarDecl {declaration.Name} : {declaration.Type}");
                if (declaration.Initializer is not null)
                {
                    WriteLine(builder, indent + 1, "Initializer");
                    WriteExpression(builder, declaration.Initializer, indent + 2);
                }
                break;

            case AssignmentStatementNode assignment:
                WriteLine(builder, indent, $"Assign {assignment.Name}");
                WriteExpression(builder, assignment.Expression, indent + 1);
                break;

            case FieldAssignmentStatementNode fieldAssignment:
                WriteLine(builder, indent, $"FieldAssign {fieldAssignment.TargetName}.{fieldAssignment.FieldName}");
                WriteExpression(builder, fieldAssignment.Expression, indent + 1);
                break;

            case IfStatementNode ifStatement:
                WriteLine(builder, indent, "If");
                WriteLine(builder, indent + 1, "Condition");
                WriteExpression(builder, ifStatement.Condition, indent + 2);
                WriteLine(builder, indent + 1, "Then");
                WriteBlock(builder, ifStatement.ThenBlock, indent + 2);
                if (ifStatement.ElseBlock is not null)
                {
                    WriteLine(builder, indent + 1, "Else");
                    WriteBlock(builder, ifStatement.ElseBlock, indent + 2);
                }
                break;

            case ForeachStatementNode foreachStatement:
                WriteLine(builder, indent, $"Foreach {foreachStatement.ItemName} : {foreachStatement.ItemType}");
                WriteLine(builder, indent + 1, "Source");
                WriteExpression(builder, foreachStatement.Source, indent + 2);
                WriteLine(builder, indent + 1, "Body");
                WriteBlock(builder, foreachStatement.Body, indent + 2);
                break;

            case SwitchStatementNode switchStatement:
                WriteLine(builder, indent, "Switch");
                WriteLine(builder, indent + 1, "Expression");
                WriteExpression(builder, switchStatement.Expression, indent + 2);

                foreach (var caseClause in switchStatement.Cases)
                {
                    WriteLine(builder, indent + 1, $"Case {FormatLiteral(caseClause.Value)}");
                    foreach (var caseStatement in caseClause.Statements)
                    {
                        WriteStatement(builder, caseStatement, indent + 2);
                    }
                }

                if (switchStatement.DefaultClause is not null)
                {
                    WriteLine(builder, indent + 1, "Default");
                    foreach (var defaultStatement in switchStatement.DefaultClause.Statements)
                    {
                        WriteStatement(builder, defaultStatement, indent + 2);
                    }
                }
                break;

            case ReturnStatementNode returnStatement:
                WriteLine(builder, indent, "Return");
                WriteExpression(builder, returnStatement.Expression, indent + 1);
                break;

            case BudgetStatementNode budgetStatement:
                WriteLine(builder, indent, $"Budget \"{budgetStatement.Label}\"");
                WriteExpression(builder, budgetStatement.Expression, indent + 1);
                break;

            case TrackStatementNode trackStatement:
                WriteLine(builder, indent, "Track");
                WriteExpression(builder, trackStatement.Expression, indent + 1);
                break;

            case CallStatementNode callStatement:
                WriteLine(builder, indent, "CallStatement");
                WriteExpression(builder, callStatement.Call, indent + 1);
                break;

            default:
                WriteLine(builder, indent, $"UnknownStatement {statement.GetType().Name}");
                break;
        }
    }

    private static void WriteExpression(StringBuilder builder, ExpressionNode expression, int indent)
    {
        switch (expression)
        {
            case LiteralExpressionNode literal:
                WriteLine(builder, indent, $"Literal {FormatLiteral(literal)} : {literal.Type}");
                break;

            case IdentifierExpressionNode identifier:
                WriteLine(builder, indent, $"Identifier {identifier.Name}");
                break;

            case ArrayLiteralExpressionNode arrayLiteral:
                WriteLine(builder, indent, "ArrayLiteral");
                foreach (var element in arrayLiteral.Elements)
                {
                    WriteExpression(builder, element, indent + 1);
                }
                break;

            case FunctionCallExpressionNode functionCall:
                WriteLine(builder, indent, $"FunctionCall {functionCall.Name}");
                foreach (var argument in functionCall.Arguments)
                {
                    WriteExpression(builder, argument, indent + 1);
                }
                break;

            case FieldAccessExpressionNode fieldAccess:
                WriteLine(builder, indent, $"FieldAccess .{fieldAccess.FieldName}");
                WriteExpression(builder, fieldAccess.Target, indent + 1);
                break;

            case BinaryExpressionNode binaryExpression:
                WriteLine(builder, indent, $"Binary {OperatorText(binaryExpression.Operator)}");
                WriteLine(builder, indent + 1, "Left");
                WriteExpression(builder, binaryExpression.Left, indent + 2);
                WriteLine(builder, indent + 1, "Right");
                WriteExpression(builder, binaryExpression.Right, indent + 2);
                break;

            default:
                WriteLine(builder, indent, $"UnknownExpression {expression.GetType().Name}");
                break;
        }
    }

    private static string FormatLiteral(LiteralExpressionNode literal) =>
        literal.Type == TypeSymbol.String ? $"\"{literal.Value}\"" :
        literal.Type == TypeSymbol.Money ? $"${literal.Value}" :
        literal.Value.ToString() ?? string.Empty;

    private static string OperatorText(TokenKind kind) =>
        kind switch
        {
            TokenKind.Plus => "+",
            TokenKind.Minus => "-",
            TokenKind.Star => "*",
            TokenKind.Slash => "/",
            TokenKind.EqualEqual => "==",
            TokenKind.NotEqual => "!=",
            TokenKind.Less => "<",
            TokenKind.LessEqual => "<=",
            TokenKind.Greater => ">",
            TokenKind.GreaterEqual => ">=",
            _ => kind.ToString()
        };

    private static void WriteLine(StringBuilder builder, int indent, string text)
    {
        builder.Append(' ', indent * 2);
        builder.AppendLine(text);
    }
}
