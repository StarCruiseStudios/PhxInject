# Code Documentation Standards for PhxInject

This document defines standards for XML documentation comments across all PhxInject projects. Follow these guidelines to ensure consistent, professional-quality documentation suitable for staff-engineer review.

## Overview

Documentation should be written as if you are a staff engineer documenting code for other senior engineers in a mature production codebase. Focus on explaining intent and design decisions, not restating what the code literally does.

## When to Document

### Public Surface (Always)

Document all public types, members, and free methods. These are part of the contract with external code.

### Internal Surface (When Important)

For internal types and members (`internal`, `private`), document those that:
- Implement critical functionality in the pipeline
- Have non-obvious behavior or important invariants
- Have architectural significance within system design
- Have important threading, performance, or side-effect implications
- Are called from multiple places and understanding their contract matters

### Skip Documentation When

- Code is trivially obvious (self-documenting)
- Member is immediately deprecated
- It's a simple getter/setter with no special semantics

## What to Include

Every doc comment should address relevant points from this list:

### For Types (Classes, Interfaces, Records, Enums)

- **Why this type exists**: The problem it solves or role it plays
- **Architectural role**: Position in the pipeline or system design
- **Key invariants**: Important constraints on state or behavior
- **Usage context**: When this type should and should NOT be used
- **Threading model**: If applicable, describe thread safety guarantees
- **Performance characteristics**: If important or non-obvious

### For Methods and Properties

- **Intent**: Why this member exists and what problem it solves
- **Parameter semantics**: What each parameter means and constraints on valid values
- **Return value meaning**: What the return value represents
- **Behavioral guarantees**: What the method promises (e.g., "never returns null", "idempotent")
- **Side effects**: Non-obvious side effects or state changes
- **Exceptions**: Important exceptions and when they occur
- **Important invariants**: Conditions that must hold before/after execution
- **Architectural context**: Relationship to pipeline stages or broader system design

### For Enum Values

- Document each enum value with a `<summary>` on the value itself
- Do NOT redundantly document the enum type itself if it's just explaining what the values mean
- Explain the semantic meaning and implications of each value

### For Constants and Fields

- Only document if the purpose or valid values are non-obvious
- Remove comments that merely restate the constant name in different words

## What to Avoid

- **Low-value comments**: Comments that describe what the code literally does without adding insight
- **Redundant comments**: Remove comments if the information is trivially derived from the declaration
- **Implementation details**: Don't document the "how" unless it affects usage
- **HTML formatting tags**: Use only C# XML doc comment tags (`<c>`, `<see>`, `<paramref>`, etc.)

### Example of Redundant Documentation to Remove

```csharp
/// <summary> The primitive type name for string. </summary>
public const string StringPrimitiveTypeName = "string";
```

This comment is redundant and should be removed.

### Example of Redundant Enum Documentation to Avoid

```csharp
/// <summary>
///     Specifies how a specification is instantiated.
/// </summary>
/// <remarks>
///     <para>
///     Controls the lifetime and ownership of the specification instance:
///     - Static: Specification has only static members, no instance needed
///     - Instantiated: Injector creates new specification instance per request
///     - Dependency: Specification instance provided externally (e.g., from parent injector)
///     </para>
///     <para>
///     This determines whether the generated container wraps a specification field/property
///     or simply forwards calls to static methods.
///     </para>
/// </remarks>
internal enum SpecInstantiationMode {
    /// <summary> The specification is static. </summary>
    Static = 0,
    /// <summary> The specification is instantiated by the injector. </summary>
    Instantiated = 1,
    /// <summary> The specification is provided by a dependency injector. </summary>
    Dependency = 2
}
```

Instead, document only the enum values with meaningful descriptions, not the enum type itself:

```csharp
internal enum SpecInstantiationMode {
    /// <summary>
    /// Specification has only static members; no instance creation needed.
    /// </summary>
    Static = 0,

    /// <summary>
    /// Injector creates a new specification instance per request.
    /// </summary>
    Instantiated = 1,

    /// <summary>
    /// Specification instance is provided externally by a parent injector.
    /// </summary>
    Dependency = 2
}
```

## Style and Formatting

### XML Tag Usage

- Use `<summary>` for brief one-line descriptions
- Use `<remarks>` for detailed explanation when needed
- Use `<param>` and `<returns>` for parameters and return values
- Use `<exception>` for important exceptions
- Use `<c>` for inline code references: `<c>null</c>`, `<c>true</c>`
- Use `<paramref name="..." />` to reference parameters in text
- Use `<see cref="..." />` or `<seealso cref="..." />` for type references
- Use `<inheritdoc />` when a member's documentation is inherited from an interface or base class

### Architectural Context

When documenting components, reference their role in the system:

**For Generator Components**: Reference the appropriate pipeline stage (Metadata, Core, Linking, Code Generation, Rendering). See [Architecture Guide](architecture.instructions.md) for pipeline details.

**For Public API Components (Phx.Inject)**: Reference where in the user experience this type fits.

### Code References

- Always use `cref` attribute when referencing types, methods, properties, or other code elements
- Example: `<see cref="IInjector" />` or `<see cref="Pipeline.Execute(ISpecification)" />`

### Formatting Guidelines

- Keep summaries concise (one or two sentences when possible)
- Use `<remarks>` for extended documentation
- Use `<para>` tags for paragraph separation in remarks
- Use `<list>` or `<listheader>` for structured information when appropriate
- Use a dash/bullet style for lists within remarks: `- Item description`

## Pipeline Context

When documenting Generator components, reference the appropriate stage in the pipeline.

Refer to [Architecture Guide](architecture.instructions.md) for detailed pipeline descriptions.

## Validation Checklist

Before completing documentation:

- [ ] All public and internal types have doc comments
- [ ] All public and internal members of documented types have doc comments
- [ ] No redundant comments remain
- [ ] All code references use `<see cref="..." />`, `<typeparamref name="..." />`, or `<paramref name="..." />`, `<c>...</c>`, etc as appropriate
- [ ] No HTML tags used for formatting (only XML doc tags)
- [ ] Enum values are documented, not the enum type (when applicable)
- [ ] Architectural role and usage context are clear
- [ ] Non-obvious parameter semantics are explained
- [ ] Important behavioral guarantees are stated
- [ ] Comments focus on intent, not implementation details

## Examples of Good Documentation

### Type Documentation

```csharp
/// <summary>
/// Extracts metadata from specification types for code generation.
/// </summary>
/// <remarks>
/// <para>
/// This analyzer traverses specification types and builds metadata models
/// suitable for the code generation pipeline. It handles both attribute-based
/// and convention-based specifications.
/// </para>
/// <para>
/// Operates as part of Stage 1 (metadata extraction). Results are cached
/// and reused across multiple generator invocations.
/// </para>
/// </remarks>
public class SpecificationAnalyzer { ... }
```

### Method Documentation

```csharp
/// <summary>
/// Registers a dependency with the container using the specified factory.
/// </summary>
/// <param name="key">The unique identifier for this dependency. Must not be null.</param>
/// <param name="factory">A factory function that creates instances. Must not be null.</param>
/// <exception cref="ArgumentNullException">Thrown if <paramref name="key" /> or <paramref name="factory" /> is null.</exception>
/// <exception cref="InvalidOperationException">Thrown if <paramref name="key" /> is already registered.</exception>
/// <remarks>
/// Factory functions are called once per resolution request. For singleton behavior, use
/// <see cref="RegisterSingleton(string, Func{IResolver, object})" /> instead.
/// </remarks>
public void Register(string key, Func<IResolver, object> factory) { ... }
```

### Enum Documentation

```csharp
/// <summary>
/// Determines the scope and lifetime of a dependency instance within the container.
/// </summary>
public enum DependencyScope {
    /// <summary>
    /// A new instance is created each time the dependency is resolved.
    /// </summary>
    Transient = 0,

    /// <summary>
    /// A single instance is created and reused for all resolutions within the same container.
    /// </summary>
    Singleton = 1,

    /// <summary>
    /// A new instance is created per resolved object graph (appropriate for web request handling).
    /// </summary>
    Scoped = 2
}
```

## Documentation Depth by Project

### Phx.Inject (Core Library)

Public API must be thoroughly documented:
- Every public type (attribute, interface, exception)
- Every public method/property
- Design intent and user guidance
- Realistic code examples in remarks

### Phx.Inject.Generator (Source Generator)

Document critical pipeline components:
- Public and internal analyzer/generator classes
- Diagnostic descriptor types
- Important methods that determine behavior
- Public extension methods

Skip documentation for:
- Private helpers
- Trivial properties
- One-off utility methods

## Decision Tree: Should I Document This?

```
Is this public API?
├─ Yes → ALWAYS document, thoroughly
└─ No
├─ Is it critical pipeline functionality?
│  ├─ Yes → Document the architectural role
│  └─ No → Continue
├─ Would another engineer wonder "why"? (not "what")
│  ├─ Yes → Document
│  └─ No → Skip
```

## Quick Rules

1. **Public = Document**: No exceptions. Future developers and external code depend on understanding your API.
2. **Staff-Engineer Quality**: Imagine this is shipping in a production library. Would you be proud of the docs?
3. **Focus on Why**: Explain design decisions, constraints, and context. The "what" is obvious from the code.
4. **Consistency Matters**: Read similar components in the codebase and match their documentation style.
5. **When Unsure**: Document it. Better over-documented than cryptic.

## Questions?

When in doubt about whether to document something or how, ask: _Would a senior engineer reading this code wonder why this decision was made, or what constraints apply?_
