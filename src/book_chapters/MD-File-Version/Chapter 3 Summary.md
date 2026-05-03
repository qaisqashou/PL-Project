# Chapter 3 — Describing Syntax and Semantics

## Key Concepts

- Syntax is the form of program constructs; semantics is their meaning (§3.1)
- Language descriptions must serve evaluators, implementors, and users, so clarity matters (§3.1)
- Syntax can be described through recognition devices or generation devices (§3.2)
- Lexemes are the smallest syntactic units, and tokens are categories of lexemes (§3.2)
- Context-free grammars, also called BNF, are the standard formalism for programming-language syntax (§3.3.1)
- Derivations and parse trees show how grammars generate legal sentences and reveal program structure (§3.3.1.5-§3.3.1.6)
- Ambiguity is dangerous because one sentence can then have multiple parse trees and meanings (§3.3.1.7)
- Operator precedence and associativity can be encoded directly in grammar structure (§3.3.1.8-§3.3.1.9)
- EBNF improves the readability and writability of grammar descriptions without increasing generative power (§3.3.2)
- Attribute grammars extend context-free grammars to describe static semantics such as type rules (§3.4)
- Dynamic semantics can be described operationally, denotationally, or axiomatically (§3.5)

## Definitions

| Term | Definition (with §section number) |
| :---- | :---- |
| Syntax | The form of expressions, statements, and program units in a language (§3.1) |
| Semantics | The meaning of expressions, statements, and program units (§3.1) |
| Lexeme | A lowest-level syntactic unit such as an identifier, literal, operator, or special word (§3.2) |
| Token | A category of lexemes, such as `identifier` or `int_literal` (§3.2) |
| Language Recognizer | A device that accepts legal strings of a language and rejects illegal ones (§3.2.1) |
| Language Generator | A device or formalism that generates the sentences of a language (§3.2.2) |
| Metalanguage | A language used to describe another language (§3.3.1.3) |
| Context-Free Grammar / BNF | A formal syntax-description mechanism built from productions over terminals and nonterminals (§3.3.1) |
| Derivation | A sequence of rule applications beginning with the start symbol and producing a sentence (§3.3.1.5) |
| Parse Tree | A hierarchical representation of the syntactic structure of a sentence (§3.3.1.6) |
| Ambiguity | The property of a grammar that allows a sentential form to have two or more distinct parse trees (§3.3.1.7) |
| Attribute Grammar | A grammar augmented with attributes and functions to specify static semantics (§3.4.2) |
| Operational Semantics | A semantics method that describes execution by state changes on an idealized machine (§3.5.1) |
| Denotational Semantics | A semantics method that maps language constructs to mathematical objects and functions (§3.5.2) |
| Axiomatic Semantics | A semantics method based on logical assertions, preconditions, and postconditions (§3.5.3) |
| Weakest Precondition | The least restrictive precondition that guarantees a postcondition after executing a statement (§3.5.3.2) |

## Design Questions

- **Should syntax be described for humans mainly as a recognizer or as a generator (§3.2)?** Recognizers match what compilers do, but generators are usually easier for people to read and reason about.
- **Should the grammar be allowed to remain ambiguous (§3.3.1.7)?** Ambiguous grammars can be shorter, but unambiguous grammars make meaning and implementation safer and clearer.
- **How should precedence and associativity be enforced (§3.3.1.8-§3.3.1.9)?** Designers can rely on extra parsing rules outside the grammar or encode operator behavior structurally inside the grammar.
- **How verbose should the formal grammar notation be (§3.3.2)?** Plain BNF is minimal and widely understood, while EBNF is shorter and easier for humans to maintain.
- **How much static semantic information should be formalized (§3.4)?** Attribute grammars can capture important compile-time rules, but they add complexity beyond plain context-free syntax.
- **Which style of semantic description best fits the goal (§3.5)?** Operational semantics is execution-oriented, denotational semantics is mathematically compositional, and axiomatic semantics is proof-oriented.

## Important Examples

- The Java statement `index = 2 * count + 17;` is used to distinguish lexemes from their token categories (§3.2)
- A small grammar for assignment statements illustrates recursive list descriptions, derivations, and generated sentences (§3.3.1.5)
- The statement `A = B * (A + C)` shows how a parse tree captures hierarchical expression structure (§3.3.1.6)
- The sentence `A = B + C * A` demonstrates ambiguity because it can produce two distinct parse trees (§3.3.1.7)
- Grammar restructuring with `<expr>`, `<term>`, and `<factor>` is used to force multiplication to bind tighter than addition (§3.3.1.8)
- The weakest precondition method is illustrated by substituting an assignment expression into a desired postcondition (§3.5.3.3)

## Quick Reference Table

| Topic | Sebesta's Key Point | Section |
| :---- | :---- | :---- |
| Syntax vs. Semantics | Syntax gives form; semantics gives meaning. | §3.1 |
| Lexemes and Tokens | Programs are usefully described as strings of lexemes grouped into token classes. | §3.2 |
| Recognizers vs. Generators | Compilers act as recognizers, but generators are often better for exposition. | §3.2 |
| BNF / CFG | The standard formal tool for describing programming-language syntax. | §3.3.1 |
| Derivations | Sentences are generated by repeated rule application from a start symbol. | §3.3.1.5 |
| Parse Trees | Trees expose the hierarchical structure on which translation depends. | §3.3.1.6 |
| Ambiguity | Multiple parse trees for one sentence create semantic uncertainty. | §3.3.1.7 |
| EBNF | Adds concise notation such as repetition and option markers. | §3.3.2 |
| Attribute Grammars | Extend grammars to express static semantic constraints. | §3.4 |
| Dynamic Semantics | Operational, denotational, and axiomatic methods capture meaning in different ways. | §3.5 |
