# Chapter 5 — Names, Bindings, and Scopes

## Key Concepts

- Names identify entities in programs, and language design must decide how names and special words behave (§5.2)
- Special words may be reserved words or merely keywords (§5.2.3)
- Variables are characterized by multiple attributes, including name, address, value, type, lifetime, and scope (§5.3)
- Aliasing occurs when multiple names access the same memory location and harms readability and reliability (§5.3.2)
- A binding is an association between an attribute and an entity, and every binding has a time when it occurs (§5.4)
- Type bindings may be static or dynamic, with major consequences for flexibility and error detection (§5.4.2)
- Storage bindings define variable lifetime categories: static, stack-dynamic, explicit heap-dynamic, and implicit heap-dynamic (§5.4.3)
- Scope controls where a name is visible; static and dynamic scope use fundamentally different rules (§5.5.1, §5.5.6)
- Global scope, declaration order, and nested blocks complicate how names are resolved (§5.5.2-§5.5.4)
- Scope is a textual concept, while lifetime is a temporal concept (§5.6)
- A referencing environment is the full set of variables visible at a point in a program (§5.7)
- Named constants improve readability and make programs easier to parameterize and maintain (§5.8)

## Definitions

| Term | Definition (with §section number) |
| :---- | :---- |
| Reserved Word | A special word that cannot be used as a programmer-defined name (§5.2.3) |
| Keyword | A special word with a predefined meaning that may still be redefinable in some languages (§5.2.3) |
| Alias | A situation in which two or more names can access the same memory location (§5.3.2) |
| l-value | The address of a variable (§5.3.2) |
| r-value | The value stored in a variable's memory cell (§5.3.4) |
| Binding | An association between an attribute and an entity, such as a variable and its type (§5.4) |
| Binding Time | The time at which a binding takes place (§5.4) |
| Static Binding | A binding that occurs before run time and remains unchanged during execution (§5.4.1) |
| Dynamic Binding | A binding that occurs during execution or can change during execution (§5.4.1) |
| Lifetime | The interval during which a variable is bound to a particular memory cell (§5.4.3) |
| Scope | The range of statements over which a variable is visible (§5.5) |
| Referencing Environment | The collection of all variables visible in a given statement (§5.7) |
| Named Constant | A variable that is bound to a value only once (§5.8) |

## Design Questions

- **Should names be case sensitive (§5.2.2)?** Case sensitivity enlarges the namespace, but it can hurt readability and make correct writing harder.
- **Should special words be reserved words or keywords (§5.2.3)?** Reserved words reduce ambiguity, while keywords offer more naming freedom at the cost of potential confusion.
- **Should types be bound statically or dynamically (§5.4.2)?** Static binding supports early error detection and efficiency; dynamic binding supports flexibility and generic programming.
- **Which storage categories should the language rely on (§5.4.3)?** Static variables are efficient and history-sensitive, stack-dynamic variables support recursion well, and heap-dynamic variables increase flexibility but complicate storage management.
- **Should scope be static or dynamic (§5.5.1-§5.5.7)?** Static scope makes programs easier to read, check, and optimize, while dynamic scope can reduce parameter passing but weakens readability and reliability.
- **How visible should globals be (§5.5.4)?** Broad global visibility can be convenient, but it increases coupling and makes maintenance harder.

## Important Examples

- JavaScript illustrates dynamic type binding when a variable such as `list` can first hold an array and later a scalar value (§5.4.2.2)
- C++ explicit heap-dynamic variables are created with `new` and released with `delete`, putting memory management directly on the programmer (§5.4.3.3)
- Python global-scope examples show that a global can be referenced inside a function directly, but assigning to it requires a `global` declaration (§5.5.4)
- A C++ `static` local variable shows that a variable can have local scope but program-wide lifetime (§5.6)
- Replacing repeated numeric literals like `100` with a named constant illustrates how named constants improve maintainability (§5.8)

## Quick Reference Table

| Topic | Sebesta's Key Point | Section |
| :---- | :---- | :---- |
| Names | Identifier design and special-word policy affect readability and usability. | §5.2 |
| Variables | Variables are best understood through multiple attributes, not just names. | §5.3 |
| Aliasing | Multiple names for one location are a major source of complexity. | §5.3.2 |
| Binding | Language design differs sharply in when and how attributes are bound. | §5.4 |
| Storage Categories | Static, stack-dynamic, explicit heap-dynamic, and implicit heap-dynamic variables trade efficiency for flexibility. | §5.4.3 |
| Static Scope | Nonlocals are resolved by the program's textual nesting structure. | §5.5.1 |
| Dynamic Scope | Nonlocals are resolved by the active calling chain at run time. | §5.5.6 |
| Scope vs. Lifetime | Scope is spatial; lifetime is temporal. | §5.6 |
| Referencing Environment | Visible variables depend on the language's scoping rules. | §5.7 |
| Named Constants | Single-assignment names aid readability, parameterization, and safer maintenance. | §5.8 |
