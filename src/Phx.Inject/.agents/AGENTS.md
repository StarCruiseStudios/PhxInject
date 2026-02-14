# Agent Instructions for Phx.Inject (Core Library)

Phx.Inject is the public-facing API and attribute library. This project defines the contracts and user-facing types for the dependency injection framework.

**Start with**: [Root Agent Instructions](../../.agents/AGENTS.md)

## Project Scope

Phx.Inject provides:
- **Attributes**: `[Specification]`, `[Injector]`, `[Factory]`, `[Builder]`, and related types
- **Public Interfaces**: Any types users interact with directly (e.g., `IInjector`)
- **Exceptions**: Domain-specific exceptions for DI configuration errors
- **Documentation**: Public API documentation is primary - this is the user contract

### What NOT to Do Here

- Don't implement core generator logic (that's Phx.Inject.Generator)
- Don't generate code in this project
- Don't add complex analysis or transformation logic

## Architecture Overview

This project acts as a **contract definition layer**:

Key principle: Any change to public APIs affects code generation. The Generator must be updated to understand the new contract.

## Public API Guidelines

All public types and members must be documented thoroughly. See [Documentation Standards](../../.agents/documentation.md#phx-inject-core-library).

### Adding New Attributes

When adding an attribute:

1. **Document thoroughly**: Users must understand exactly what it does and when to use it
2. **Design carefully**: Consider backwards compatibility; changing attribute contract breaks generated code
3. **Update Generator**: Phx.Inject.Generator must understand the new attribute
4. **Add examples**: Include code examples in the documentation
5. **Write tests**: Phx.Inject.Tests must validate the attribute works correctly when processed

Example structure:

```csharp
/// <summary>
/// Marks a method as a dependency builder in a specification.
/// </summary>
/// <remarks>
/// <para>
/// Applied to methods in classes marked with <see cref="SpecificationAttribute" />.
/// Builder methods modify an existing target instance rather than creating a new one.
/// They're useful when:
/// - The type doesn't have a suitable constructor
/// - Initialization requires multiple steps
/// - You want to compose complex initialization logic
/// </para>
/// <para>
/// Builder method signature:
/// <code>
/// [Builder]
/// public void BuildMyType(MyType target, [other dependencies]) { }
/// </code>
/// </para>
/// <para>
/// The first parameter is always the target instance. Remaining parameters are
/// resolved as dependencies from the specification, just like factory methods.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class BuilderAttribute : Attribute { }
```

### Semantic Versioning

- **Major**: Breaking changes to attribute contract, removed attributes
- **Minor**: New attributes, new optional parameters (backwards compatible)
- **Patch**: Bug fixes, documentation updates

Attributes are immutable once released in a major version. Plan carefully.

## Testing Strategy

### Unit Tests

Phx.Inject is primarily attribute types, which do not require unit tests—attributes are declarative markers with no executable logic.

However, any **utility classes designed to be referenced by generator code** should include comprehensive unit tests. These utilities serve as shared contracts between user code and the generator, so correctness is critical.

Examples of utility classes that should be tested:
- Helper methods used by generated code
- Domain model types that the generator depends on
- Validation or transformation utilities

Focus tests on:
- Edge cases and invalid inputs
- Contract guarantees the generator relies on
- Backwards compatibility with different compiler versions

### Integration Tests

Integration testing strategy to be determined. To be completed in a future iteration.

### Generator Compatibility

Before releasing changes:

1. Ensure Phx.Inject.Generator understands the change
2. Run Phx.Inject.Generator.Tests
3. Verify no compilation errors

## Code Organization

Keep types **minimal and flat** in the root `Phx.Inject` namespace:

- **Attributes go in root**: Place attribute types directly in `Phx.Inject` (e.g., `FactoryAttribute.cs`)
- **Utility classes**: If a utility class is needed, keep it in `Phx.Inject` root unless it's truly internal
- **No deep hierarchies**: Avoid nested namespaces; this is the public API contract

This flat structure keeps the API surface simple and discoverable. Users reference `Phx.Inject` and immediately see all public types.

## Key Invariants

Maintain these when modifying public API:

1. **Backwards Compatibility**: Existing code using old attributes must still compile
2. **Contract Clarity**: The attribute contract must be unambiguous to the generator
3. **Semantic Stability**: Don't change the meaning of existing attributes
4. **Documentation First**: Document before implementation; let contracts drive design

## Validation Checklist

Before completing work on Phx.Inject:

- [ ] All public types have comprehensive XML documentation
- [ ] Code examples included in attribute documentation where helpful
- [ ] Tests verify the attribute behavior
- [ ] Generator project updated if attribute contract changed
- [ ] Phx.Inject.Generator compiles successfully
- [ ] No breaking changes to existing public surface (unless major version)
- [ ] New attributes documented in README.md if user-facing
- [ ] Comments focus on "why" and user guidance, not code behavior
- [ ] Phx.Inject.Tests pass

## Communication with Generator

If you change an attribute's meaning or add a new one:

1. **Document in the attribute itself**: Generator developers will read the attribute first
2. **Update Phx.Inject.Generator**: Ensure it understands the new contract
3. **Add tests in Phx.Inject.Generator.Tests**: Verify the generator correctly processes your change
4. **Update this guide**: Document new patterns for future agents/developers

Example: Adding a new optional parameter to an attribute:

```csharp
// 1. Update attribute with new parameter
[AttributeUsage(AttributeTargets.Method)]
public sealed class FactoryAttribute : Attribute {
    /// <summary>
    /// Specifies the lifetime of instances created by this factory.
    /// </summary>
    public LifetimeMode Lifetime { get; init; } = LifetimeMode.Transient;
}

// 2. Update Generator to understand new parameter
private LifetimeMode ExtractLifetime(IMethodSymbol method) {
    var attr = method.GetAttribute("Factory");
    var lifetimeArg = attr?.GetNamedArgument("Lifetime");
    return lifetimeArg != null 
        ? ParseLifetimeMode(lifetimeArg)
        : LifetimeMode.Transient;
}

// 3. Add tests in Generator.Tests
[Test]
public void Generator_HandlesNewLifetimeParameter()
{
    // Test with and without the parameter
}
```
