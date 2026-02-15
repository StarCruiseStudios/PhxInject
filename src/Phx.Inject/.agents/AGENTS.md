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

All tests in **Phx.Inject.Tests** use **Phx.Test** for test orchestration and **PhxValidation** for assertions. Tests focus on **runtime behavior** of the library and generated injectors.

### Quick Start

```csharp
public class MyFeatureTests : LoggingTestClass
{
    [Injector(typeof(MyTestSpecification))]  // Generated interface implementation
    public interface IMyTestInjector
    {
        MyType GetInstance();
    }

    [Test]
    public void MethodName_Scenario_ExpectedOutcome()
    {
        var injector = Given("A test injector",
            () => new MyTestInjector());

        var result = When("Getting instance",
            () => injector.GetInstance());

        Then("Result is correct",
            () => Verify.That(result.IsType<MyType>()));
    }
}
```

### Test Organization

- Test classes: `{Feature}Tests.cs` inherit from `LoggingTestClass`
- Test specifications: Lightweight specs in `Data/` folder with `[Specification]` attribute
- Assertions: Use `Verify.That()` fluent methods (`.IsEqualTo()`, `.IsType<>()`, etc.)
- Naming: `MethodName_Scenario_ExpectedOutcome`

### See Also

- [Testing Quick Reference](../../.github/instructions/testing-phxinject.instructions.md) - Comprehensive patterns and examples
- [Architecture Guide](../../.github/instructions/architecture.instructions.md) - System design and pipeline
- [Phx.Test Documentation](https://github.com/StarCruiseStudios/PhxTest)
- [Phx.Validation Documentation](https://github.com/StarCruiseStudios/PhxValidation)

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
- [ ] Generator project updated if attribute contract changed
- [ ] Phx.Inject.Generator compiles successfully
- [ ] No breaking changes to existing public surface (unless major version)
- [ ] New attributes documented in README.md if user-facing
- [ ] Comments focus on "why" and user guidance, not code behavior

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
    /// Specifies the fabrication mode for this factory.
    /// </summary>
    public FabricationMode FabricationMode { get; set; } = FabricationMode.Recurrent;
}

// 2. Update Generator to understand new parameter (see Generator project docs)

// 3. Add tests if testing strategy is defined
```
