# Chapter 4 — Lexical and Syntax Analysis

## Key Concepts

- Compilers almost always separate lexical analysis from syntax analysis (§4.1)
- Lexical analyzers match character patterns, group characters into lexemes, and return tokens to the parser (§4.2)
- Reserved words are usually recognized with the same pattern as identifiers and then distinguished by table lookup (§4.2)
- Syntax analysis, or parsing, checks syntactic correctness, recovers from errors, and builds a parse tree or its trace (§4.3.1)
- Top-down parsers build from the root toward the leaves and correspond to leftmost derivations (§4.3.2)
- Bottom-up parsers build from the leaves toward the root and correspond to the reverse of a rightmost derivation (§4.3.3)
- General parsing for arbitrary unambiguous grammars is too slow for production compilers, so practical compilers use restricted grammar classes (§4.3.4)
- Recursive-descent parsing is a direct, code-based form of LL parsing (§4.4)
- LL parsing is limited by left recursion and the need to choose productions predictably from lookahead (§4.4.2)
- Bottom-up parsing centers on finding handles and reducing them correctly (§4.5.1)
- LR parsers are powerful bottom-up, shift-reduce parsers driven by ACTION and GOTO tables (§4.5.3)

## Definitions

| Term | Definition (with §section number) |
| :---- | :---- |
| Lexical Analyzer | The front end that reads characters, groups them into lexemes, and returns tokens (§4.2) |
| Token Code | An internal code that represents a lexeme category for later compiler phases (§4.2) |
| State Transition Diagram | A directed graph representation of the finite automaton used for lexical recognition (§4.2) |
| Parser / Syntax Analyzer | The component that checks large-scale syntax and builds parse structure (§4.3) |
| Top-Down Parser | A parser that builds a parse tree from root to leaves, following a leftmost derivation (§4.3.2) |
| Bottom-Up Parser | A parser that builds a parse tree from leaves to root, following the reverse of a rightmost derivation (§4.3.3) |
| Handle | The substring of a right sentential form that should be reduced to produce the previous step in a rightmost derivation (§4.3.3, §4.5.1) |
| Recursive-Descent Parser | A hand-coded LL parser implemented as a collection of mutually recursive subprograms (§4.4) |
| Direct Left Recursion | A grammar situation where a nonterminal appears first on its own RHS, such as `E → E + T` (§4.4.2) |
| Pairwise Disjointness Test | The requirement that a parser be able to choose the correct RHS using available lookahead information (§4.4.2) |
| Left Factoring | Rewriting grammar rules to postpone a parsing decision when alternatives share a prefix (§4.4.2) |
| Shift-Reduce Parsing | A bottom-up method that shifts input onto a stack and reduces handles to nonterminals (§4.5.2) |
| LR Parser | A left-to-right, rightmost-derivation-based parser that uses tables and a stack to direct actions (§4.5.3) |

## Design Questions

- **Should lexical analysis be separated from syntax analysis (§4.1)?** Separation improves simplicity, efficiency, and portability, but it adds an interface boundary between two compiler phases.
- **How should reserved words be recognized (§4.2)?** Dedicated state-diagram paths are possible, but treating them like identifiers and then using table lookup is usually simpler and faster.
- **Should the parser be top-down or bottom-up (§4.3.2-§4.3.3, §4.5.3)?** Top-down parsers are easy to write by hand, while bottom-up parsers handle a larger class of grammars more robustly.
- **How much grammar generality should be sacrificed for speed (§4.3.4)?** General algorithms run in O(n3), but production compilers accept grammar restrictions to achieve linear O(n) behavior.
- **Should parsing logic be coded directly or generated from tables (§4.4, §4.5.3)?** Recursive descent is readable and direct, while LR table-driven parsers are more powerful but more cumbersome to build manually.

## Important Examples

- The chapter builds a simple lexical analyzer around a state diagram and a C program (`front.c`) that recognizes identifiers, integer literals, parentheses, and arithmetic operators (§4.2)
- The expression `(sum + 47) / total` is tokenized to show how the lexical analyzer reports token codes and lexemes (§4.2)
- Left recursion is eliminated by rewriting `E → E + T | T` into a form such as `E → T E'` and `E' → + T E' | ε` (§4.4.2)
- Left factoring rewrites alternatives like `<variable> → id | id [ <expr> ]` so the parser can consume `id` first and decide later (§4.4.2)
- The LR parsing discussion traces how a parser handles input such as `id + id * id` using shifts, reductions, and table lookups (§4.5.3)

## Quick Reference Table

| Topic | Sebesta's Key Point | Section |
| :---- | :---- | :---- |
| Why Separate Lex/Syntax? | Separation makes the compiler simpler, faster to optimize, and more portable. | §4.1 |
| Lexical Analysis | Character streams are transformed into lexemes and tokens by pattern matching. | §4.2 |
| Parser Goals | A parser must detect syntax errors, recover, and build parse structure. | §4.3.1 |
| Top-Down Parsing | Builds from root to leaves and follows leftmost derivation structure. | §4.3.2 |
| Bottom-Up Parsing | Builds from leaves to root and works by handle recognition and reduction. | §4.3.3 |
| Parsing Complexity | Fully general unambiguous parsing is O(n3), while practical compiler parsers aim for O(n). | §4.3.4 |
| Recursive Descent | An LL parser can be written directly as mutually recursive procedures. | §4.4 |
| LL Limits | Left recursion and poor predictability must be removed for top-down parsing. | §4.4.2 |
| Shift-Reduce | Bottom-up parsing uses stack operations to delay decisions until handles are known. | §4.5.2 |
| LR Parsing | ACTION and GOTO tables make powerful deterministic bottom-up parsing practical. | §4.5.3 |
