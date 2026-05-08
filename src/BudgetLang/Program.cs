using BudgetLang.Ast;
using BudgetLang.Runtime;
using BudgetLang.Semantics;
using BudgetLang.Syntax;

if (args.Length is < 1 or > 2)
{
    Console.Error.WriteLine("Usage: dotnet run --project src/BudgetLang -- [--ast] <source-file>");
    return 1;
}

try
{
    var printAstOnly = args.Length == 2;
    var sourcePath = printAstOnly ? args[1] : args[0];

    if (printAstOnly && args[0] != "--ast")
    {
        Console.Error.WriteLine("Usage: dotnet run --project src/BudgetLang -- [--ast] <source-file>");
        return 1;
    }

    var sourceText = File.ReadAllText(sourcePath);

    var lexer = new Lexer(sourceText);
    var tokens = lexer.Tokenize();

    var parser = new Parser(tokens);
    var program = parser.ParseProgram();

    if (printAstOnly)
    {
        Console.WriteLine(AstPrinter.Print(program));
        return 0;
    }

    var typeChecker = new TypeChecker();
    typeChecker.Check(program);

    var interpreter = new Interpreter();
    interpreter.Execute(program);

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}
