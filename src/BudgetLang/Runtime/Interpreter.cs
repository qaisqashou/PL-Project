using BudgetLang.Ast;
using BudgetLang.Syntax;

namespace BudgetLang.Runtime;

public sealed class Interpreter
{
    private readonly Dictionary<string, FunctionDeclarationNode> _functions = new(StringComparer.Ordinal);
    private EnvironmentScope? _globalEnvironment;

    public void Execute(ProgramNode program)
    {
        _functions.Clear();

        foreach (var function in program.Functions)
        {
            _functions[function.Name] = function;
        }

        _globalEnvironment = new EnvironmentScope(null);
        ExecuteStatements(program.Statements, _globalEnvironment);
    }

    private void ExecuteStatements(IReadOnlyList<StatementNode> statements, EnvironmentScope environment)
    {
        foreach (var statement in statements)
        {
            ExecuteStatement(statement, environment);
        }
    }

    private void ExecuteStatement(StatementNode statement, EnvironmentScope environment)
    {
        switch (statement)
        {
            case VariableDeclarationNode declaration:
                var value = declaration.Initializer is null
                    ? DefaultValue(declaration.Type)
                    : CloneValue(EvaluateExpression(declaration.Initializer, environment));
                environment.Declare(declaration.Name, value);
                break;

            case AssignmentStatementNode assignment:
                environment.Assign(assignment.Name, CloneValue(EvaluateExpression(assignment.Expression, environment)));
                break;

            case FieldAssignmentStatementNode fieldAssignment:
                var transaction = environment.Lookup(fieldAssignment.TargetName) as TransactionValue
                    ?? throw Error($"Variable '{fieldAssignment.TargetName}' is not a transaction.");
                var updatedValue = CloneValue(EvaluateExpression(fieldAssignment.Expression, environment));
                environment.Assign(fieldAssignment.TargetName, UpdateField(transaction, fieldAssignment.FieldName, updatedValue));
                break;

            case IfStatementNode ifStatement:
                var condition = EvaluateExpression(ifStatement.Condition, environment) as BoolValue
                    ?? throw Error("If conditions must evaluate to bool.");
                if (condition.Value)
                {
                    ExecuteStatements(ifStatement.ThenBlock.Statements, new EnvironmentScope(environment));
                }
                else if (ifStatement.ElseBlock is not null)
                {
                    ExecuteStatements(ifStatement.ElseBlock.Statements, new EnvironmentScope(environment));
                }
                break;

            case ForeachStatementNode foreachStatement:
                var items = EvaluateExpression(foreachStatement.Source, environment) as ArrayValue
                    ?? throw Error("Foreach source must be an array.");
                foreach (var item in items.Elements)
                {
                    var foreachEnvironment = new EnvironmentScope(environment);
                    foreachEnvironment.Declare(foreachStatement.ItemName, CloneValue(item));
                    ExecuteStatements(foreachStatement.Body.Statements, foreachEnvironment);
                }
                break;

            case SwitchStatementNode switchStatement:
                ExecuteSwitch(switchStatement, environment);
                break;

            case ReturnStatementNode returnStatement:
                throw new ReturnSignal(CloneValue(EvaluateExpression(returnStatement.Expression, environment)));

            case BudgetStatementNode budgetStatement:
                var budgetValue = EvaluateExpression(budgetStatement.Expression, environment);
                Console.WriteLine($"BUDGET {budgetStatement.Label}: {FormatValue(budgetValue)}");
                break;

            case TrackStatementNode trackStatement:
                var trackedValue = EvaluateExpression(trackStatement.Expression, environment) as TransactionValue
                    ?? throw Error("Track statements require a transaction value.");
                Console.WriteLine($"TRACK {FormatTransaction(trackedValue)}");
                break;

            case CallStatementNode callStatement:
                _ = EvaluateFunctionCall(callStatement.Call, environment);
                break;

            case BlockStatementNode blockStatement:
                ExecuteStatements(blockStatement.Statements, new EnvironmentScope(environment));
                break;

            default:
                throw Error($"Unsupported statement kind '{statement.GetType().Name}'.");
        }
    }

    private void ExecuteSwitch(SwitchStatementNode switchStatement, EnvironmentScope environment)
    {
        var switchValue = EvaluateExpression(switchStatement.Expression, environment);

        foreach (var caseClause in switchStatement.Cases)
        {
            var caseValue = EvaluateLiteral(caseClause.Value);
            if (ValuesEqual(switchValue, caseValue))
            {
                ExecuteStatements(caseClause.Statements, new EnvironmentScope(environment));
                return;
            }
        }

        if (switchStatement.DefaultClause is not null)
        {
            ExecuteStatements(switchStatement.DefaultClause.Statements, new EnvironmentScope(environment));
        }
    }

    private RuntimeValue EvaluateExpression(ExpressionNode expression, EnvironmentScope environment)
    {
        return expression switch
        {
            LiteralExpressionNode literal => EvaluateLiteral(literal),
            IdentifierExpressionNode identifier => CloneValue(environment.Lookup(identifier.Name)),
            ArrayLiteralExpressionNode arrayLiteral => new ArrayValue(arrayLiteral.Elements.Select(element => CloneValue(EvaluateExpression(element, environment))).ToList()),
            FunctionCallExpressionNode functionCall => EvaluateFunctionCall(functionCall, environment),
            FieldAccessExpressionNode fieldAccess => EvaluateFieldAccess(fieldAccess, environment),
            BinaryExpressionNode binaryExpression => EvaluateBinaryExpression(binaryExpression, environment),
            _ => throw Error($"Unsupported expression kind '{expression.GetType().Name}'.")
        };
    }

    private RuntimeValue EvaluateLiteral(LiteralExpressionNode literal) =>
        literal.Type.Name switch
        {
            "money" => new MoneyValue((decimal)literal.Value),
            "int" => new IntValue((int)literal.Value),
            "string" => new StringValue((string)literal.Value),
            "bool" => new BoolValue((bool)literal.Value),
            _ => throw Error($"Unsupported literal type '{literal.Type}'.")
        };

    private RuntimeValue EvaluateFunctionCall(FunctionCallExpressionNode functionCall, EnvironmentScope callerEnvironment)
    {
        if (!_functions.TryGetValue(functionCall.Name, out var function))
        {
            throw Error($"Undefined function '{functionCall.Name}'.");
        }

        var functionEnvironment = new EnvironmentScope(_globalEnvironment);
        for (var i = 0; i < function.Parameters.Count; i++)
        {
            var argumentValue = CloneValue(EvaluateExpression(functionCall.Arguments[i], callerEnvironment));
            functionEnvironment.Declare(function.Parameters[i].Name, argumentValue);
        }

        try
        {
            ExecuteStatements(function.Body.Statements, functionEnvironment);
        }
        catch (ReturnSignal signal)
        {
            return signal.Value;
        }

        throw Error($"Function '{function.Name}' completed without returning a value.");
    }

    private RuntimeValue EvaluateFieldAccess(FieldAccessExpressionNode fieldAccess, EnvironmentScope environment)
    {
        var target = EvaluateExpression(fieldAccess.Target, environment) as TransactionValue
            ?? throw Error($"Field access '.{fieldAccess.FieldName}' requires a transaction value.");

        return fieldAccess.FieldName switch
        {
            "amount" => new MoneyValue(target.Amount),
            "category" => new StringValue(target.Category),
            "kind" => new StringValue(target.Kind),
            "note" => new StringValue(target.Note),
            _ => throw Error($"Unknown transaction field '{fieldAccess.FieldName}'.")
        };
    }

    private RuntimeValue EvaluateBinaryExpression(BinaryExpressionNode binaryExpression, EnvironmentScope environment)
    {
        var left = EvaluateExpression(binaryExpression.Left, environment);
        var right = EvaluateExpression(binaryExpression.Right, environment);

        return binaryExpression.Operator switch
        {
            TokenKind.Plus => Add(left, right),
            TokenKind.Minus => Subtract(left, right),
            TokenKind.Star => Multiply(left, right),
            TokenKind.Slash => Divide(left, right),
            TokenKind.EqualEqual => new BoolValue(ValuesEqual(left, right)),
            TokenKind.NotEqual => new BoolValue(!ValuesEqual(left, right)),
            TokenKind.Less => Compare(left, right, comparison => comparison < 0),
            TokenKind.LessEqual => Compare(left, right, comparison => comparison <= 0),
            TokenKind.Greater => Compare(left, right, comparison => comparison > 0),
            TokenKind.GreaterEqual => Compare(left, right, comparison => comparison >= 0),
            _ => throw Error($"Unsupported operator '{binaryExpression.Operator}'.")
        };
    }

    private static RuntimeValue Add(RuntimeValue left, RuntimeValue right) =>
        (left, right) switch
        {
            (MoneyValue a, MoneyValue b) => new MoneyValue(a.Amount + b.Amount),
            (IntValue a, IntValue b) => new IntValue(a.Value + b.Value),
            (StringValue a, StringValue b) => new StringValue(a.Value + b.Value),
            _ => throw Error("Operator '+' requires matching money, int, or string operands.")
        };

    private static RuntimeValue Subtract(RuntimeValue left, RuntimeValue right) =>
        (left, right) switch
        {
            (MoneyValue a, MoneyValue b) => new MoneyValue(a.Amount - b.Amount),
            (IntValue a, IntValue b) => new IntValue(a.Value - b.Value),
            _ => throw Error("Operator '-' requires matching money or int operands.")
        };

    private static RuntimeValue Multiply(RuntimeValue left, RuntimeValue right) =>
        (left, right) switch
        {
            (MoneyValue a, MoneyValue b) => new MoneyValue(a.Amount * b.Amount),
            (IntValue a, IntValue b) => new IntValue(a.Value * b.Value),
            _ => throw Error("Operator '*' requires matching money or int operands.")
        };

    private static RuntimeValue Divide(RuntimeValue left, RuntimeValue right) =>
        (left, right) switch
        {
            (MoneyValue a, MoneyValue b) when b.Amount != 0 => new MoneyValue(a.Amount / b.Amount),
            (IntValue a, IntValue b) when b.Value != 0 => new IntValue(a.Value / b.Value),
            (MoneyValue, MoneyValue) => throw Error("Division by zero."),
            (IntValue, IntValue) => throw Error("Division by zero."),
            _ => throw Error("Operator '/' requires matching money or int operands.")
        };

    private static RuntimeValue Compare(RuntimeValue left, RuntimeValue right, Func<int, bool> predicate)
    {
        var comparison = (left, right) switch
        {
            (MoneyValue a, MoneyValue b) => a.Amount.CompareTo(b.Amount),
            (IntValue a, IntValue b) => a.Value.CompareTo(b.Value),
            (StringValue a, StringValue b) => string.CompareOrdinal(a.Value, b.Value),
            _ => throw Error("Comparison requires matching money, int, or string operands.")
        };

        return new BoolValue(predicate(comparison));
    }

    private static bool ValuesEqual(RuntimeValue left, RuntimeValue right) =>
        (left, right) switch
        {
            (MoneyValue a, MoneyValue b) => a.Amount == b.Amount,
            (IntValue a, IntValue b) => a.Value == b.Value,
            (StringValue a, StringValue b) => a.Value == b.Value,
            (BoolValue a, BoolValue b) => a.Value == b.Value,
            (TransactionValue a, TransactionValue b) =>
                a.Amount == b.Amount &&
                a.Category == b.Category &&
                a.Kind == b.Kind &&
                a.Note == b.Note,
            _ => false
        };

    private static TransactionValue UpdateField(TransactionValue transaction, string fieldName, RuntimeValue value) =>
        (fieldName, value) switch
        {
            ("amount", MoneyValue moneyValue) => transaction with { Amount = moneyValue.Amount },
            ("category", StringValue stringValue) => transaction with { Category = stringValue.Value },
            ("kind", StringValue stringValue) => transaction with { Kind = stringValue.Value },
            ("note", StringValue stringValue) => transaction with { Note = stringValue.Value },
            _ => throw Error($"Invalid assignment to field '{fieldName}'.")
        };

    private static RuntimeValue DefaultValue(TypeSymbol type) =>
        type.Name switch
        {
            "money" => new MoneyValue(0),
            "int" => new IntValue(0),
            "string" => new StringValue(string.Empty),
            "transaction" => new TransactionValue(0, string.Empty, string.Empty, string.Empty),
            "transaction[]" => new ArrayValue([]),
            _ => throw Error($"No default value is defined for type '{type}'.")
        };

    private static RuntimeValue CloneValue(RuntimeValue value) =>
        value switch
        {
            MoneyValue moneyValue => moneyValue with { },
            IntValue intValue => intValue with { },
            StringValue stringValue => stringValue with { },
            BoolValue boolValue => boolValue with { },
            TransactionValue transactionValue => transactionValue with { },
            ArrayValue arrayValue => new ArrayValue(arrayValue.Elements.Select(CloneValue).ToList()),
            _ => throw Error($"Cannot clone value of type '{value.GetType().Name}'.")
        };

    private static string FormatValue(RuntimeValue value) =>
        value switch
        {
            MoneyValue moneyValue => $"${moneyValue.Amount:0.##}",
            IntValue intValue => intValue.Value.ToString(),
            StringValue stringValue => $"\"{stringValue.Value}\"",
            BoolValue boolValue => boolValue.Value ? "true" : "false",
            TransactionValue transactionValue => FormatTransaction(transactionValue),
            ArrayValue arrayValue => $"[{string.Join(", ", arrayValue.Elements.Select(FormatValue))}]",
            _ => value.ToString() ?? string.Empty
        };

    private static string FormatTransaction(TransactionValue value) =>
        $"amount=${value.Amount:0.##}, category=\"{value.Category}\", kind=\"{value.Kind}\", note=\"{value.Note}\"";

    private static InvalidOperationException Error(string message) =>
        new($"Runtime error: {message}");

    private abstract record RuntimeValue;

    private sealed record MoneyValue(decimal Amount) : RuntimeValue;

    private sealed record IntValue(int Value) : RuntimeValue;

    private sealed record StringValue(string Value) : RuntimeValue;

    private sealed record BoolValue(bool Value) : RuntimeValue;

    private sealed record TransactionValue(decimal Amount, string Category, string Kind, string Note) : RuntimeValue;

    private sealed record ArrayValue(IReadOnlyList<RuntimeValue> Elements) : RuntimeValue;

    private sealed class EnvironmentScope
    {
        private readonly Dictionary<string, RuntimeValue> _values = new(StringComparer.Ordinal);
        private readonly EnvironmentScope? _parent;

        public EnvironmentScope(EnvironmentScope? parent)
        {
            _parent = parent;
        }

        public void Declare(string name, RuntimeValue value)
        {
            _values[name] = value;
        }

        public RuntimeValue Lookup(string name)
        {
            if (_values.TryGetValue(name, out var value))
            {
                return value;
            }

            if (_parent is not null)
            {
                return _parent.Lookup(name);
            }

            throw Error($"Undefined variable '{name}'.");
        }

        public void Assign(string name, RuntimeValue value)
        {
            if (_values.ContainsKey(name))
            {
                _values[name] = value;
                return;
            }

            if (_parent is not null)
            {
                _parent.Assign(name, value);
                return;
            }

            throw Error($"Undefined variable '{name}'.");
        }
    }

    private sealed class ReturnSignal : Exception
    {
        public ReturnSignal(RuntimeValue value)
        {
            Value = value;
        }

        public RuntimeValue Value { get; }
    }
}
