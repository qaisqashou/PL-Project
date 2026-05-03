# Chapter 7 — Expressions and Assignment Statements

## Key Concepts

- Expressions are the primary way computations are specified in programming languages (§7.1)
- Arithmetic-expression meaning depends on operator precedence, associativity, parentheses, and operand evaluation order (§7.2.1)
- Functional side effects make operand evaluation order semantically important (§7.2.2.1)
- Referential transparency is lost when an expression's value depends on side effects rather than only on its visible inputs (§7.2.2.2)
- Overloaded operators reuse one symbol for multiple operations, which can help or hurt readability (§7.3)
- Type conversions may be implicit coercions or explicit casts and may be widening or narrowing (§7.4)
- Relational and Boolean expressions are central to control structures and benefit from true Boolean types (§7.5)
- Short-circuit evaluation can improve efficiency and safety, but it can also suppress intended side effects (§7.6)
- Assignment statements are the core state-changing mechanism of imperative languages (§7.7)
- Assignment design varies through conditional targets, compound operators, unary assignment operators, assignment as an expression, and multiple assignments (§7.7.2-§7.7.6)
- Mixed-mode assignment asks when a value of one type may be assigned to a variable of another type (§7.8)

## Definitions

| Term | Definition (with §section number) |
| :---- | :---- |
| Operator Precedence | The rule that determines the order of evaluation among adjacent operators of different precedence levels (§7.2.1.1) |
| Operator Associativity | The rule that determines the evaluation order of adjacent operators with the same precedence (§7.2.1.2) |
| Functional Side Effect | A side effect produced when a function changes a two-way parameter or a nonlocal variable (§7.2.2.1) |
| Referential Transparency | The property that an expression can be replaced by its value without changing program behavior (§7.2.2.2) |
| Overloaded Operator | A single operator symbol that has more than one meaning (§7.3) |
| Coercion | An implicit type conversion inserted automatically by the implementation (§7.4.1) |
| Widening Conversion | A conversion to a type that can represent all or nearly all values of the original type (§7.4.1) |
| Narrowing Conversion | A conversion to a type that cannot represent all values of the original type without possible loss (§7.4.1) |
| Short-Circuit Evaluation | Evaluation that determines a result without evaluating all operands when the answer is already known (§7.6) |
| Mixed-Mode Assignment | An assignment in which the right-hand expression type differs from the target variable's type (§7.8) |

## Design Questions

- **Should operand evaluation order be fixed (§7.2.2.1)?** A fixed order improves predictability and reliability, while implementation freedom can enable better optimization.
- **Should programmers be allowed to overload operators (§7.3)?** Intuitive overloading can improve readability for new types, but poor overloading can make expressions misleading.
- **Should coercions be implicit (§7.4.1)?** Implicit conversions reduce boilerplate and can improve writability, but they also hide errors and weaken type checking.
- **Should Boolean operators short-circuit (§7.6)?** Short-circuiting improves efficiency and prevents some run-time errors, but it may suppress side effects that a programmer expected to occur.
- **Should assignment behave like a normal expression (§7.7.5)?** Allowing assignment inside larger expressions increases compactness, but it also creates harder-to-read code and more opportunities for subtle bugs.
- **How strict should mixed-mode assignment be (§7.8)?** Free coercion increases convenience, while allowing only widening conversions increases reliability.

## Important Examples

- The expression `a + fun(&a)` shows how a functional side effect can make the result depend on operand evaluation order (§7.2.2.1)
- The Boolean test `(index < listlen) && (list[index] != key)` shows why short-circuit evaluation prevents out-of-range access in a search loop (§7.6)
- Perl conditional targets are illustrated by `($flag ? $count1 : $count2) = 0;`, which assigns to one of two variables based on a condition (§7.7.2)
- Compound assignment such as `sum += value;` abbreviates `sum = sum + value;` (§7.7.3)
- Assignment as an expression appears in `while ((ch = getchar()) != EOF)`, where the assignment value is immediately tested (§7.7.5)
- Multiple assignment such as `($first, $second) = ($second, $first);` swaps values without an explicit temporary variable (§7.7.6)

## Quick Reference Table

| Topic | Sebesta's Key Point | Section |
| :---- | :---- | :---- |
| Expressions | The semantics of expressions depend on evaluation rules, not just operator symbols. | §7.1-§7.2 |
| Operand Evaluation Order | Side effects make unspecified evaluation order a real semantic hazard. | §7.2.2.1 |
| Referential Transparency | Expressions are easier to reason about when evaluation does not alter hidden state. | §7.2.2.2 |
| Overloaded Operators | Reusing symbols can improve notation or destroy clarity depending on design. | §7.3 |
| Type Conversions | Widening is safer than narrowing; coercion trades convenience for error detection. | §7.4 |
| Boolean Expressions | Real Boolean types improve readability and reduce misuse of numeric values as conditions. | §7.5 |
| Short-Circuiting | It improves safety and efficiency but interacts dangerously with side effects. | §7.6 |
| Assignment Variants | Modern languages provide many assignment forms beyond simple `=`. | §7.7 |
| Assignment as Expression | Compact but risky because it introduces side effects into larger expressions. | §7.7.5 |
| Mixed-Mode Assignment | Languages differ in how freely they allow assignment across type boundaries. | §7.8 |
