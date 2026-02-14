# API Design Guidelines for Phx.Inject

Standards for designing public API types and attributes in the Phx.Inject library.

## Attribute Design Principles

### Single Responsibility

Each attribute should have one clear purpose:

```csharp
// Good: Single, clear purpose
[AttributeUsage(AttributeTargets.Method)]
public sealed class FactoryAttribute : Attribute { }

// Avoid: Multiple concerns mixed
[AttributeUsage(AttributeTargets.Method)]
public sealed class FactoryAttribute : Attribute {
    public bool IsTransient { get; init; }
    public bool CanBeNull { get; init; }
    public string? CustomName { get; init; }
    public int Priority { get; init; }
    // Too many concerns!
}
```

### Explicit Over Implicit

Force users to be explicit about their intent:

```csharp
// Good: Explicit parameters
[AttributeUsage(AttributeTargets.Method)]
public sealed class FactoryAttribute : Attribute {
    public int Version { get; init; } = 1; // Explicit default
}

// Avoid: Implicit magic values
[AttributeUsage(AttributeTargets.Method)]
public sealed class FactoryAttribute : Attribute {
    public int Priority { get; init; } // What does 0 vs 1 mean? Implicit
}
```

### Documentation-Driven Design

Design for clarity, not brevity. Users should understand the contract without guessing:

```csharp
/// <summary>
/// Marks a method as a dependency factory in a specification.
/// </summary>
/// <remarks>
/// <para>
/// The method's return type determines which injector methods it serves.
/// The method's parameters are resolved as dependencies from other factories/builders,
/// matched by type.
/// </para>
/// <para>
/// Multiple factories with the same return type are an error; specify
/// <see cref="Name"/> to distinguish them.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class FactoryAttribute : Attribute {
    /// <summary>
    /// Optional name to distinguish factories with the same return type.
    /// </summary>
    public string? Name { get; init; }
}
```

### Extensibility vs. Stability

**Conservative API Surface**

- Only expose what users need directly
- Keep implementation details internal
- Don't prematurely expose for future features that don't exist yet
- Simpler surface is easier to maintain

## Naming Conventions

### Attribute Names

- Always suffix with "Attribute": `[Factory]` not `[Fact]`
- Use clear, domain-specific names
- Match the user's mental model

```csharp
// Good: Clear domain terminology
[Factory]
[Builder]
[Specification]
[Injector]

// Avoid: Vague or abbreviated
[Gen]
[Spec]
[Create]
```

### Property Names

- PascalCase, full words
- Match user terminology

```csharp
public sealed class FactoryAttribute : Attribute {
    /// <summary>Specifies the fabrication mode for this factory.</summary>
    public FabricationMode FabricationMode { get; set; } = FabricationMode.Recurrent;
}
```

## Validation Decisions

### Design-Time vs. Runtime Validation

Phx.Inject validates at design time (compilation), not runtime. This is a core design principle.

**Validate at compile time (in generator)**:
- Missing required dependencies
- Circular dependencies
- Type mismatches
- Duplicate factory definitions

**Don't add runtime validation**:
- Don't throw in attribute constructors
- Don't add runtime guards

```csharp
// Good: No runtime validation
[AttributeUsage(AttributeTargets.Method)]
public class FactoryAttribute : Attribute { 
    public FabricationMode FabricationMode { get; set; } = FabricationMode.Recurrent;
}

// Avoid: Runtime validation in attribute
[AttributeUsage(AttributeTargets.Method)]
public class FactoryAttribute : Attribute {
    private string _name;
    
    public string Name {
        get => _name;
        init {
            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentException("Name cannot be empty"); // Wrong!
            }
            _name = value;
        }
    }
}
```

The generator validates, not the attribute.

## Optional Parameters

### Design for Defaults

Attributes should work sensibly with minimal configuration:

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class SpecificationAttribute : Attribute {
    // No parameters required - works with just [Specification]
}

[AttributeUsage(AttributeTargets.Interface)]
public class InjectorAttribute : Attribute {
    /// <summary>Specification types to use for dependency resolution.</summary>
    public IEnumerable<Type> Specifications { get; }
    
    public InjectorAttribute(params Type[] specifications) {
        Specifications = specifications;
    }
}
```

### Avoid Optional in Incorrect Context

```csharp
// Good: Optional when it enhances but isn't required
public class FactoryAttribute : Attribute {
    public FabricationMode FabricationMode { get; set; } = FabricationMode.Recurrent;
}

// Avoid: Optional when it changes fundamental behavior
public class SpecificationAttribute : Attribute {
    public bool Enabled { get; init; } = true; // Why optional? Confusing.
}
```

## Examples in Documentation

Always include realistic examples:

```csharp
/// <summary>
/// Marks a method as a dependency factory in a specification.
/// </summary>
/// <remarks>
/// <para>
/// Example specification:
/// <code>
/// [Specification]
/// public static class MyServices {
///     [Factory]
///     public DatabaseConnection CreateConnection() {
///         return new DatabaseConnection("connection string");
///     }
///     
///     [Factory]
///     public IRepository CreateRepository(DatabaseConnection db) {
///         return new Repository(db);
///     }
/// }
/// </code>
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class FactoryAttribute : Attribute { }
```

## Backwards Compatibility

### Adding to Public API

Adding is safe; removing or changing is breaking:

```csharp
// Safe: Add new property with default
public class FactoryAttribute : Attribute {
    public FabricationMode FabricationMode { get; set; } = FabricationMode.Recurrent;
}

// Breaking: Remove or change meaning
public class FactoryAttribute : Attribute {
    // Removed FabricationMode property - breaks existing code
}

// Breaking: Change default
public class FactoryAttribute : Attribute {
    public FabricationMode FabricationMode { get; set; } = FabricationMode.Scoped; // Changed default
}
```

### Deprecation

Mark deprecated features explicitly:

```csharp
/// <summary>
/// This attribute is deprecated and will be removed in version 3.0.
/// Use <see cref="NewFactoryAttribute"/> instead.
/// </summary>
[Obsolete("Use NewFactoryAttribute instead. This will be removed in version 3.0.", false)]
[AttributeUsage(AttributeTargets.Method)]
public sealed class OldFactoryAttribute : Attribute { }
```

## Sealing Attributes

**Current Implementation**: PhxInject attributes are not sealed to allow extensibility if needed.

Best practice is to seal attributes unless extensibility is specifically required:

```csharp
// Best practice: Sealed attributes
[AttributeUsage(AttributeTargets.Method)]
public sealed class FactoryAttribute : Attribute { }

// Current PhxInject implementation: Unsealed for extensibility
[AttributeUsage(AttributeTargets.Method)]
public class FactoryAttribute : Attribute { }
```

Benefits of sealed attributes:
- Prevent accidental subclassing
- Protect API contract
- Enable compiler optimizations
- Match framework convention

When to leave unsealed:
- Extensibility is a planned feature
- Framework design requires inheritance

## Validation Checklist

Before adding new public types/attributes:

- [ ] Single, clear responsibility
- [ ] Name is clear and unambiguous
- [ ] Documented with examples
- [ ] Backwards compatible (doesn't break existing code)
- [ ] No runtime validation; design-time only
- [ ] Generator projects updated to understand new type
- [ ] Behavior matches documentation exactly
- [ ] Defaults are sensible and safe
