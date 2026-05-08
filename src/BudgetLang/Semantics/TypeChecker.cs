using BudgetLang.Ast;
using BudgetLang.Syntax;

namespace BudgetLang.Semantics;

public sealed class TypeChecker
{
    private static readonly Dictionary<string, TypeSymbol> TransactionFields = new(StringComparer.Ordinal)
    {
        ["amount"] = TypeSymbol.Money,
        ["category"] = TypeSymbol.String,
        ["kind"] = TypeSymbol.String,
        ["note"] = TypeSymbol.String
    };

    private readonly Dictionary<string, FunctionDeclarationNode> _functions = new(StringComparer.Ordinal);
    private Scope? _globalScope;

    public void Check(ProgramNode program)
    {
        RegisterFunctions(program.Functions);

        _globalScope = new Scope(null);
        CheckStatements(program.Statements, _globalScope, TypeSymbol.Void);

        foreach (var function in program.Functions)
        {
            CheckFunction(function);
        }
    }

    private void RegisterFunctions(IReadOnlyList<FunctionDeclarationNode> functions)
    {
        _functions.Clear();

        foreach (var function in functions)
        {
            if (!_functions.TryAdd(function.Name, function))
            {
                throw Error($"Duplicate function '{function.Name}'.");
            }
        }
    }

    private void CheckFunction(FunctionDeclarationNode function)
    {
        var scope = new Scope(_globalScope);

        foreach (var parameter in function.Parameters)
        {
            scope.Declare(parameter.Name, parameter.Type);
        }

        CheckStatements(function.Body.Statements, scope, function.ReturnType);
    }

    private void CheckStatements(IReadOnlyList<StatementNode> statements, Scope scope, TypeSymbol expectedReturnType)
    {
        foreach (var statement in statements)
        {
            CheckStatement(statement, scope, expectedReturnType);
        }
    }

    private void CheckStatement(StatementNode statement, Scope scope, TypeSymbol expectedReturnType)
    {
        switch (statement)
        {
            case VariableDeclarationNode declaration:
                if (scope.IsDeclaredInCurrentScope(declaration.Name))
                {
                    throw Error($"Variable '{declaration.Name}' is already declared in this scope.");
                }

                if (declaration.Initializer is not null)
                {
                    var initializerType = CheckExpression(declaration.Initializer, scope);
                    RequireSameType(declaration.Type, initializerType, $"Cannot initialize '{declaration.Name}' with a value of type '{initializerType}'.");
                }

                scope.Declare(declaration.Name, declaration.Type);
                break;

            case AssignmentStatementNode assignment:
                var targetType = scope.Lookup(assignment.Name)
                    ?? throw Error($"Undefined variable '{assignment.Name}'.");
                var valueType = CheckExpression(assignment.Expression, scope);
                RequireSameType(targetType, valueType, $"Cannot assign a value of type '{valueType}' to '{assignment.Name}' of type '{targetType}'.");
                break;

            case FieldAssignmentStatementNode fieldAssignment:
                var ownerType = scope.Lookup(fieldAssignment.TargetName)
                    ?? throw Error($"Undefined variable '{fieldAssignment.TargetName}'.");

                RequireSameType(TypeSymbol.Transaction, ownerType, $"Field assignment requires a transaction variable, but '{fieldAssignment.TargetName}' has type '{ownerType}'.");
                var fieldType = LookupTransactionField(fieldAssignment.FieldName);
                var assignedFieldType = CheckExpression(fieldAssignment.Expression, scope);
                RequireSameType(fieldType, assignedFieldType, $"Cannot assign a value of type '{assignedFieldType}' to transaction field '{fieldAssignment.FieldName}' of type '{fieldType}'.");
                break;

            case IfStatementNode ifStatement:
                RequireSameType(TypeSymbol.Bool, CheckExpression(ifStatement.Condition, scope), "If conditions must have boolean type.");
                CheckStatements(ifStatement.ThenBlock.Statements, new Scope(scope), expectedReturnType);
                if (ifStatement.ElseBlock is not null)
                {
                    CheckStatements(ifStatement.ElseBlock.Statements, new Scope(scope), expectedReturnType);
                }
                break;

            case ForeachStatementNode foreachStatement:
                var sourceType = CheckExpression(foreachStatement.Source, scope);
                RequireSameType(TypeSymbol.TransactionArray, sourceType, "Foreach sources must have type 'transaction[]'.");
                RequireSameType(TypeSymbol.Transaction, foreachStatement.ItemType, "Foreach loop variables must currently be of type 'transaction'.");

                var foreachScope = new Scope(scope);
                foreachScope.Declare(foreachStatement.ItemName, foreachStatement.ItemType);
                CheckStatements(foreachStatement.Body.Statements, foreachScope, expectedReturnType);
                break;

            case SwitchStatementNode switchStatement:
                var switchType = CheckExpression(switchStatement.Expression, scope);
                if (switchType != TypeSymbol.Int && switchType != TypeSymbol.String)
                {
                    throw Error($"Switch expressions must be int or string, but found '{switchType}'.");
                }

                foreach (var caseClause in switchStatement.Cases)
                {
                    RequireSameType(switchType, caseClause.Value.Type, $"Case value '{caseClause.Value.Value}' does not match switch expression type '{switchType}'.");
                    CheckStatements(caseClause.Statements, new Scope(scope), expectedReturnType);
                }

                if (switchStatement.DefaultClause is not null)
                {
                    CheckStatements(switchStatement.DefaultClause.Statements, new Scope(scope), expectedReturnType);
                }
                break;

            case ReturnStatementNode returnStatement:
                if (expectedReturnType == TypeSymbol.Void)
                {
                    throw Error("Return statements are only allowed inside functions.");
                }

                var returnType = CheckExpression(returnStatement.Expression, scope);
                RequireSameType(expectedReturnType, returnType, $"Return type mismatch. Expected '{expectedReturnType}' but found '{returnType}'.");
                break;

            case BudgetStatementNode budgetStatement:
                var budgetValueType = CheckExpression(budgetStatement.Expression, scope);
                if (budgetValueType == TypeSymbol.Void)
                {
                    throw Error("Budget statements cannot use a void expression.");
                }
                break;

            case TrackStatementNode trackStatement:
                RequireSameType(TypeSymbol.Transaction, CheckExpression(trackStatement.Expression, scope), "Track statements require a transaction value.");
                break;

            case CallStatementNode callStatement:
                _ = CheckFunctionCall(callStatement.Call, scope);
                break;

            case BlockStatementNode blockStatement:
                CheckStatements(blockStatement.Statements, new Scope(scope), expectedReturnType);
                break;

            default:
                throw Error($"Unsupported statement kind '{statement.GetType().Name}'.");
        }
    }

    private TypeSymbol CheckExpression(ExpressionNode expression, Scope scope)
    {
        return expression switch
        {
            LiteralExpressionNode literal => literal.Type,
            IdentifierExpressionNode identifier => scope.Lookup(identifier.Name)
                ?? throw Error($"Undefined variable '{identifier.Name}'."),
            ArrayLiteralExpressionNode arrayLiteral => CheckArrayLiteral(arrayLiteral, scope),
            FunctionCallExpressionNode functionCall => CheckFunctionCall(functionCall, scope),
            FieldAccessExpressionNode fieldAccess => CheckFieldAccess(fieldAccess, scope),
            BinaryExpressionNode binaryExpression => CheckBinaryExpression(binaryExpression, scope),
            _ => throw Error($"Unsupported expression kind '{expression.GetType().Name}'.")
        };
    }

    private TypeSymbol CheckArrayLiteral(ArrayLiteralExpressionNode arrayLiteral, Scope scope)
    {
        if (arrayLiteral.Elements.Count == 0)
        {
            throw Error("Empty array literals are not supported yet because their element type cannot be inferred.");
        }

        foreach (var element in arrayLiteral.Elements)
        {
            var elementType = CheckExpression(element, scope);
            RequireSameType(TypeSymbol.Transaction, elementType, "BudgetLang currently supports only transaction arrays.");
        }

        return TypeSymbol.TransactionArray;
    }

    private TypeSymbol CheckFunctionCall(FunctionCallExpressionNode functionCall, Scope scope)
    {
        if (!_functions.TryGetValue(functionCall.Name, out var function))
        {
            throw Error($"Undefined function '{functionCall.Name}'.");
        }

        if (functionCall.Arguments.Count != function.Parameters.Count)
        {
            throw Error($"Function '{functionCall.Name}' expects {function.Parameters.Count} argument(s) but received {functionCall.Arguments.Count}.");
        }

        for (var i = 0; i < function.Parameters.Count; i++)
        {
            var argumentType = CheckExpression(functionCall.Arguments[i], scope);
            var parameterType = function.Parameters[i].Type;
            RequireSameType(parameterType, argumentType, $"Argument {i + 1} of function '{functionCall.Name}' must be '{parameterType}', but found '{argumentType}'.");
        }

        return function.ReturnType;
    }

    private TypeSymbol CheckFieldAccess(FieldAccessExpressionNode fieldAccess, Scope scope)
    {
        var ownerType = CheckExpression(fieldAccess.Target, scope);
        RequireSameType(TypeSymbol.Transaction, ownerType, $"Field access '.{fieldAccess.FieldName}' requires a transaction value.");
        return LookupTransactionField(fieldAccess.FieldName);
    }

    private TypeSymbol CheckBinaryExpression(BinaryExpressionNode binaryExpression, Scope scope)
    {
        var leftType = CheckExpression(binaryExpression.Left, scope);
        var rightType = CheckExpression(binaryExpression.Right, scope);

        switch (binaryExpression.Operator)
        {
            case TokenKind.Plus:
                if (leftType == TypeSymbol.String && rightType == TypeSymbol.String)
                {
                    return TypeSymbol.String;
                }

                goto case TokenKind.Minus;

            case TokenKind.Minus:
            case TokenKind.Star:
            case TokenKind.Slash:
                if (leftType == TypeSymbol.Money && rightType == TypeSymbol.Money)
                {
                    return TypeSymbol.Money;
                }

                if (leftType == TypeSymbol.Int && rightType == TypeSymbol.Int)
                {
                    return TypeSymbol.Int;
                }

                throw Error($"Operator '{TokenText(binaryExpression.Operator)}' requires matching numeric operands.");

            case TokenKind.Less:
            case TokenKind.LessEqual:
            case TokenKind.Greater:
            case TokenKind.GreaterEqual:
                RequireSameType(leftType, rightType, $"Operator '{TokenText(binaryExpression.Operator)}' requires both operands to have the same type.");
                if (leftType != TypeSymbol.Int && leftType != TypeSymbol.Money && leftType != TypeSymbol.String)
                {
                    throw Error($"Operator '{TokenText(binaryExpression.Operator)}' is not defined for type '{leftType}'.");
                }

                return TypeSymbol.Bool;

            case TokenKind.EqualEqual:
            case TokenKind.NotEqual:
                RequireSameType(leftType, rightType, $"Operator '{TokenText(binaryExpression.Operator)}' requires both operands to have the same type.");
                return TypeSymbol.Bool;

            default:
                throw Error($"Unsupported binary operator '{binaryExpression.Operator}'.");
        }
    }

    private static TypeSymbol LookupTransactionField(string fieldName)
    {
        if (TransactionFields.TryGetValue(fieldName, out var fieldType))
        {
            return fieldType;
        }

        throw Error($"Unknown transaction field '{fieldName}'.");
    }

    private static void RequireSameType(TypeSymbol expected, TypeSymbol actual, string message)
    {
        if (expected != actual)
        {
            throw Error(message);
        }
    }

    private static string TokenText(TokenKind tokenKind) =>
        tokenKind switch
        {
            TokenKind.Plus => "+",
            TokenKind.Minus => "-",
            TokenKind.Star => "*",
            TokenKind.Slash => "/",
            TokenKind.Less => "<",
            TokenKind.LessEqual => "<=",
            TokenKind.Greater => ">",
            TokenKind.GreaterEqual => ">=",
            TokenKind.EqualEqual => "==",
            TokenKind.NotEqual => "!=",
            _ => tokenKind.ToString()
        };

    private static InvalidOperationException Error(string message) =>
        new($"Type error: {message}");

    private sealed class Scope
    {
        private readonly Dictionary<string, TypeSymbol> _variables = new(StringComparer.Ordinal);
        private readonly Scope? _parent;

        public Scope(Scope? parent)
        {
            _parent = parent;
        }

        public void Declare(string name, TypeSymbol type) => _variables[name] = type;

        public bool IsDeclaredInCurrentScope(string name) => _variables.ContainsKey(name);

        public TypeSymbol? Lookup(string name)
        {
            if (_variables.TryGetValue(name, out var type))
            {
                return type;
            }

            return _parent?.Lookup(name);
        }
    }
}
