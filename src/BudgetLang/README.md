# BudgetLang D2 Starter

This project is a C# starter implementation for BudgetLang D2.

Current structure:

- `Syntax/Lexer.cs`: converts source text into tokens.
- `Syntax/Parser.cs`: builds an AST from the token stream.
- `Ast/SyntaxTree.cs`: AST node and type definitions.
- `Semantics/TypeChecker.cs`: checks declarations, function calls, field access, and expression types.
- `Runtime/Interpreter.cs`: executes the program after type checking.

This starter follows the design choices already declared in D1:

- Separate lexer and parser phases, matching Sebesta's compiler front-end split (§4.2).
- Static scope for variables and blocks (§5.5.1).
- Static type bindings for declared variables and functions (§5.4.1-§5.4.2).
- Strong exact-match typing with no implicit coercions in this first pass (§6.13-§6.15).
- Expression precedence encoded in the parser, not left informal (§7.2.1).

Current assumptions to revisit before final D2 submission:

- `transaction` has the fixed fields `amount`, `category`, `kind`, and `note`.
- `track` requires a `transaction` value.
- `switch` uses no fall-through; only the first matching case runs, otherwise `default` runs.
- Empty array literals are not supported yet.

Run from the `BudgetLang` project directory (this folder in the repo, or the root of the extracted D2 zip):

```powershell
dotnet build
dotnet run --project . -- tests/valid/valid1.txt
```

Print the AST only:

```powershell
dotnet run --project . -- --ast tests/valid/valid1.txt
```

Parser tests:

Valid programs:

```powershell
dotnet run --project . -- --ast tests/valid/valid1.txt
dotnet run --project . -- --ast tests/valid/valid2.txt
dotnet run --project . -- --ast tests/valid/valid3.txt
```

Malformed programs:

```powershell
dotnet run --project . -- --ast tests/invalid/invalid1_missing_semicolon.txt
dotnet run --project . -- --ast tests/invalid/invalid2_missing_closing_brace.txt
dotnet run --project . -- --ast tests/invalid/invalid3_bad_if_header.txt
dotnet run --project . -- --ast tests/invalid/invalid4_broken_field_assignment.txt
dotnet run --project . -- --ast tests/invalid/invalid5_missing_right_paren_call.txt
```

Suggested next steps:

1. Run the sample program and fix any parser/runtime gaps.
2. Add parser test inputs for the three valid programs and five malformed programs required by D3.
3. Decide and document the remaining D1/D2 semantic details explicitly, especially parameter passing and switch behavior.
