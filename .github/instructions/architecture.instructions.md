# Architecture Guide: PhxInject System Design

This document explains the architecture of PhxInject, a compile-time dependency injection framework that generates container code using Roslyn incremental source generators.

## System Overview

PhxInject consists of three primary components:

1. **Phx.Inject Library** - Public API containing attributes and interfaces consumed by user code
2. **User Code** - Application code using Phx.Inject attributes to declare specifications
3. **Phx.Inject.Generator** - Roslyn source generator that analyzes user code and generates container implementations

```
┌──────────────────┐      ┌──────────────────┐
│   User Code      │─────▶│  Phx.Inject      │
│   (Application)  │      │  (Library)       │
└──────────────────┘      └──────────────────┘
         │                         │
         │ Analyzed by             │ References
         ▼                         ▼
┌──────────────────────────────────────────┐
│   Phx.Inject.Generator                   │
│   (Roslyn Source Generator)              │
│                                          │
│   ┌──────────────────────────────────┐  │
│   │ Five-Stage Pipeline:             │  │
│   │ 1. Metadata Extraction           │  │
│   │ 2. Core Analysis                 │  │
│   │ 3. Linking                       │  │
│   │ 4. Code Generation               │  │
│   │ 5. Rendering                     │  │
│   └──────────────────────────────────┘  │
└──────────────────────────────────────────┘
         │
         │ Generates
         ▼
┌──────────────────┐
│   Generated      │
│   Container Code │
└──────────────────┘
```

## Design Goals

### Primary Objectives

1. **Zero Runtime Overhead**: All dependency resolution logic generated at compile time
2. **Compile-Time Validation**: Detect configuration errors before runtime
3. **Type Safety**: Leverage C# type system to prevent misconfigurations
4. **Performance**: Fast builds via incremental source generation
5. **Developer Experience**: Clear diagnostics with actionable error messages

### Design Philosophy

- **Explicit over Implicit**: Developers must explicitly declare dependencies via attributes
- **Fail Fast**: Report all detectable errors at compile time
- **No Magic**: Generated code should be readable and debuggable
- **Incremental by Default**: Use Roslyn's incremental generator APIs for caching

## Component Relationships

### Public API Surface (Phx.Inject)

**Attributes** for marking specifications and dependencies:
- `[Specification]` - Marks a class/interface as a dependency specification
- `[SpecificationInterfaceType]` - Marks an interface for implementation generation
- `[Factory]` - Marks a method as a factory for creating instances
- `[Builder]` - Marks a method as a builder for configuring instances
- `[Injector]` - Marks a method parameter as requiring injector reference
- `[Link]` - Links specifications to enable type resolution across specifications

**Interfaces** for generated code:
- `IInjector<TSpec>` - Base interface for generated injectors
- Domain-specific interfaces generated based on specification types

**Exceptions** for runtime error reporting:
- Used when runtime conditions violate compile-time guarantees (rare)

### Source Generator (Phx.Inject.Generator)

The generator operates as a **five-stage pipeline**:

#### Stage 1: Metadata Extraction

**Purpose**: Extract raw metadata from syntax trees about specification types

**Key Types**:
- `SpecificationAnalyzer` - Identifies types with `[Specification]` attribute
- `FactoryMethodAnalyzer` - Extracts factory and builder method metadata
- `AttributeReader` - Parses attribute arguments

**Output**: `SpecificationMetadata` records containing type symbols and locations

**Design Pattern**: **Specification Pattern** - Each analyzer defines criteria for what constitutes valid metadata

#### Stage 2: Core Analysis

**Purpose**: Validate metadata and build semantic model

**Key Types**:
- `CoreValidator` - Validates specification rules (e.g., factory return types)
- `DependencyGraphBuilder` - Constructs dependency graph for each specification
- `TypeMapper` - Maps types to factory methods

**Output**: `CoreModel` containing validated specifications and dependency graphs

**Design Pattern**: **Factory Pattern** - Validate and transform raw metadata into domain model

#### Stage 3: Linking

**Purpose**: Resolve cross-specification dependencies via `[Link]` attributes

**Key Types**:
- `LinkResolver` - Resolves linked specifications
- `TypeIndex` - Global index of all available types across specifications

**Output**: `LinkedModel` with complete dependency resolution

**Key Invariant**: After linking, all resolvable dependencies must have a resolution path

#### Stage 4: Code Generation

**Purpose**: Generate C# syntax trees for container implementations

**Key Types**:
- `InjectorGenerator` - Generates injector class implementations
- `MethodGenerator` - Generates individual factory/builder method implementations
- `DependencyChainGenerator` - Generates dependency resolution chains

**Output**: `GeneratedCodeModel` containing syntax trees

**Design Pattern**: **Builder Pattern** - Incrementally construct syntax trees

#### Stage 5: Rendering

**Purpose**: Emit final C# source code strings

**Key Types**:
- `CodeRenderer` - Converts syntax trees to formatted strings
- `FileOrganizer` - Organizes generated code into appropriate files

**Output**: Generated `.g.cs` files added to compilation

**Design Pattern**: **Template Method** - Consistent rendering structure with customization points

## Key Architectural Patterns

### Incremental Generation

**Why**: Roslyn's incremental generator API caches pipeline outputs, dramatically improving build performance

**How**:
- Each pipeline stage is a transformation function
- Transformations are pure (deterministic, no side effects)
- Roslyn caches transformation results based on input hashes
- Only recompute when inputs change

**Implementation**:
```csharp
var metadataProvider = context.SyntaxProvider
    .ForAttributeWithMetadataName("Phx.Inject.SpecificationAttribute", ...)
    .Select((ctx, _) => ExtractMetadata(ctx));  // Cached by Roslyn

var validatedProvider = metadataProvider
    .Select((metadata, _) => Validate(metadata)); // Reuses cache
```

### Diagnostic-Driven Design

**Why**: Compile-time error reporting is a core feature

**How**:
- Each pipeline stage collects diagnostics
- Diagnostics use `IResult<T>` pattern (success with value or failure with diagnostics)
- Generator continues processing to report multiple errors in one pass
- Diagnostics include source location for precise error messages

**Implementation**:
```csharp
public IResult<CoreModel> Analyze(SpecificationMetadata metadata) {
    return DiagnosticsRecorder.Capture(diagnostics => {
        var validated = ValidateFactories(metadata).GetValue(diagnostics);
        var graph = BuildDependencyGraph(validated).GetValue(diagnostics);
        return new CoreModel(validated, graph);
    });
}
```

### Specification Pattern (Domain Validation)

**Why**: Complex validation rules for what constitutes valid specifications

**How**:
- Each validation encapsulated in a specification type
- Specifications compose (AND, OR, NOT) for complex rules
- Reusable across multiple pipeline stages

**Example**:
- `IsPublicOrInternalType` - Validates type visibility
- `HasParameterlessConstructor` - Validates instantiation capability
- `ReturnTypeIsNotVoid` - Validates factory return types

## System Invariants

These invariants must hold throughout the system:

1. **No Circular Dependencies**: Dependency graphs must be acyclic
2. **All Dependencies Resolvable**: After linking, every dependency has a resolution path
3. **Type Safety**: Generated code never requires unsafe casts
4. **Deterministic Output**: Same input always produces same generated code (hash-based caching)
5. **Never Throw to Compiler**: Generator catches all exceptions and converts to diagnostics

## Modification Points

When adding new features, consider these extension points:

### Adding New Attributes

1. Define attribute in `Phx.Inject` project
2. Add analyzer in Stage 1 (Metadata Extraction)
3. Update `CoreValidator` in Stage 2 to validate new metadata
4. Extend `InjectorGenerator` in Stage 4 to handle new generation patterns

### Adding New Diagnostics

1. Define diagnostic descriptor in `DiagnosticDescriptors.cs`
2. Report diagnostic at appropriate pipeline stage using `DiagnosticsRecorder`
3. Add test case validating diagnostic is reported correctly

### Changing Generated Code Format

1. Modify `MethodGenerator` or `InjectorGenerator` in Stage 4
2. Update `CodeRenderer` in Stage 5 if formatting changes needed
3. Update snapshot tests to verify new output format

## Performance Considerations

### Generator Performance

- **Incremental APIs**: Always use incremental providers (`Select`, `Combine`, `Where`)
- **Early Filtering**: Use syntactic predicates in `ForAttributeWithMetadataName` to avoid expensive semantic analysis
- **Caching**: Cache expensive computations (fully qualified names, symbol lookups)
- **Avoid Enumerating All Symbols**: Never iterate entire compilation unless absolutely necessary

### Generated Code Performance

- **Zero Allocations for Simple Factories**: Generated code should directly call specification methods
- **Dependency Chain Optimization**: Reuse dependencies across multiple factory invocations when safe
- **No Reflection**: All type resolution at compile time
- **Inline Small Methods**: Use expression-bodied members for simple getters

## Testing Strategy

See [Testing Standards](testing.instructions.md) for comprehensive testing guidance covering:
- Unit testing individual pipeline stages
- Integration testing full generator pipeline
- Snapshot testing generated code output
- Diagnostic validation testing

## Additional Resources

- [Coding Standards](coding-standards.instructions.md) - C# coding conventions
- [Code Generation Standards](code-generation.instructions.md) - Source generator specific patterns
- [Documentation Standards](documentation.instructions.md) - XML documentation guidelines
- [Testing Standards](testing.instructions.md) - Testing practices and patterns
