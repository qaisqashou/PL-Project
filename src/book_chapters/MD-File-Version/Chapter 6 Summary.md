# Chapter 6 — Data Types

## Key Concepts

- A data type is a set of values together with the predefined operations on those values (§6.1)
- Primitive data types include numeric, Boolean, and character types (§6.2)
- Character string design varies by operations offered and by when length is bound (§6.3)
- User-defined ordinal types include enumeration and subrange types (§6.4)
- Arrays are homogeneous aggregates whose design depends heavily on index and storage binding decisions (§6.5)
- Associative arrays use keys instead of numeric subscripts and are now common in scripting and dynamic languages (§6.6)
- Records are heterogeneous aggregates with named fields (§6.7)
- Tuples and lists are important aggregate forms, especially in functional and scripting languages (§6.8-§6.9)
- Union types allow one variable to hold values of different types at different times (§6.10)
- Pointer and reference types support dynamic structures but create important safety and storage-management issues (§6.11)
- Optional types let variables explicitly represent the absence of a value (§6.12)
- Type checking, strong typing, and type equivalence are central to a language's type system and reliability (§6.13-§6.15)

## Definitions

| Term | Definition (with §section number) |
| :---- | :---- |
| Data Type | A collection of data values and the predefined operations on those values (§6.1) |
| Descriptor | The collection of a variable's attributes used for type checking and storage management (§6.1) |
| Primitive Data Type | A type that is not defined in terms of other types (§6.2) |
| Ordinal Type | A type whose range of values can be associated with the positive integers (§6.4) |
| Associative Array | An unordered collection whose elements are indexed by keys rather than positions (§6.6) |
| Record | A heterogeneous aggregate of data elements identified by field names (§6.7) |
| Tuple | A fixed-size aggregate similar to a record but without field names (§6.8) |
| List | An ordered sequence structure heavily used in functional languages (§6.9) |
| Union | A type whose variables may store values of different types at different times (§6.10) |
| Pointer | A variable whose values are memory addresses plus a special null-like value (§6.11) |
| Dangling Pointer | A pointer that refers to a heap-dynamic variable that has already been deallocated (§6.11.3.1) |
| Lost Heap-Dynamic Variable | An allocated variable that can no longer be accessed by the program, creating garbage or a memory leak (§6.11.3.2) |
| Reference Type | A restricted pointer-like type that refers to objects or values without exposing pointer arithmetic (§6.11.5) |
| Optional Type | A type whose variables may either contain a normal value or a designated no-value marker (§6.12) |
| Type Checking | Ensuring that the operands of an operator are of compatible types (§6.13) |
| Strong Typing | The property that type errors are always detected, either at compile time or run time (§6.14) |
| Type Equivalence | The rule set for deciding when two types are considered the same for compatibility without coercion (§6.15) |

## Design Questions

- **When should string length be bound (§6.3.3)?** Static lengths are efficient, while limited dynamic and fully dynamic strings offer greater flexibility.
- **When should array bounds and storage be bound (§6.5.3)?** Static and fixed stack-dynamic arrays are efficient, while heap-dynamic arrays are more adaptable but more expensive.
- **Should unions be free or discriminated (§6.10.2)?** Free unions maximize flexibility but reduce safety; discriminated unions add tags that support checking.
- **Should the language support pointers, references, or both (§6.11.1)?** Pointers give low-level control, while references provide safer dynamic access with fewer dangerous operations.
- **How should heap storage be reclaimed (§6.11.7.3)?** Explicit deallocation is efficient but risky; garbage collection reduces programmer errors but adds run-time overhead.
- **How much coercion should type checking allow (§6.13-§6.14)?** More coercion improves convenience, but it weakens the reliability benefit of strong typing.
- **Should type equivalence be based on names or structure (§6.15)?** Name equivalence is simpler and stricter, while structure equivalence is more flexible but harder to implement.

## Important Examples

- Enumeration types such as `enum colors {red, blue, green};` improve readability and restrict values to a meaningful set (§6.4)
- A Python dictionary like `{"John": 75000, "Mary": 82000}` illustrates associative arrays keyed by names rather than indices (§6.6)
- A dangling-pointer scenario is created when `p1` and `p2` both reference a heap cell and one reference is deallocated while the other still points to it (§6.11.3.1)
- C# optional types are illustrated by declarations such as `int? x;`, where `null` represents "no value" (§6.12)
- Type equivalence is contrasted by declarations such as two separately named array types that are distinct under name equivalence but compatible under structure equivalence (§6.15)

## Quick Reference Table

| Topic | Sebesta's Key Point | Section |
| :---- | :---- | :---- |
| Primitive Types | Numeric, Boolean, and character types provide the base of the type system. | §6.2 |
| Strings | String design balances rich operations and flexible length against implementation cost. | §6.3 |
| Ordinal Types | Enumeration and subrange types improve readability and checking. | §6.4 |
| Arrays | Array design hinges on index rules, storage binding, and shape decisions. | §6.5 |
| Associative Arrays | Key-indexed collections are common in modern scripting languages. | §6.6 |
| Records / Tuples / Lists | Aggregate types differ mainly in heterogeneity, field naming, and intended use. | §6.7-§6.9 |
| Unions | Multi-type storage is flexible but dangerous without discriminants. | §6.10 |
| Pointers and References | Dynamic structures depend on them, but safety and reclamation are major concerns. | §6.11 |
| Optional Types | Languages can make "no value" explicit in the type system. | §6.12 |
| Type Checking and Strong Typing | Reliable languages detect more type misuse and rely less on unchecked coercion. | §6.13-§6.14 |
| Type Equivalence | Compatibility for structured types depends on whether names or structures are compared. | §6.15 |
