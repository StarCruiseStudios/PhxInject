# Agent Instructions for Phx.Inject.Generator

Phx.Inject.Generator is the Roslyn incremental source generator that transforms specifications and injectors into compiled injection code.

**Start with**: [Root Agent Instructions](../../.agents/AGENTS.md)

## Project Scope

This project is responsible for:
- **Discovery**: Finding `[Specification]` and `[Injector]` types
- **Analysis**: Analyzing specifications to extract factories and builders
- **Linking**: Matching injector methods to specification factories/builders
- **Code Generation**: Producing valid, efficient C# code
- **Diagnostics**: Reporting compilation errors to users

### What NOT to Do

- Don't add user-facing attributes here (those belong in Phx.Inject)
- Don't change public API without updating Phx.Inject
- Don't expose generator internals; keep implementation details private
- Don't skip performance considerations; large projects depend on fast generation

## Critical Performance Constraints

**Incremental generation must be fast.** Projects with 100+ dependencies must analyze and generate in < 500ms. This is non-negotiable.

### Performance Guidelines

- **Caching**: Use incremental providers aggressively; cache expensive analysis
- **Early Exit**: Return from predicates as soon as conditions fail
- **Allocations**: Be conscious of LINQ allocations in hot paths
- **Roslyn API**: Know which APIs are expensive (e.g., `GetAttributes()` on every symbol)
- **Testing**: Include performance benchmarks for complex analyzers/linkers

Example:

```csharp
// Good: Quick predicate exit
context.SyntaxProvider.CreateSyntaxProvider(
    predicate: (syntax, _) => {
        // Fast: check syntax node type only
        return syntax is ClassDeclarationSyntax { AttributeLists: not [] };
    },
    transform: (ctx, _) => {
        // Expensive: only on filtered candidates
        return AnalyzeSpecification(ctx);
    }
);

// Avoid: No predicate filtering
context.SyntaxProvider.CreateSyntaxProvider(
    predicate: (syntax, _) => true, // Processes every syntax node!
    transform: (ctx, _) => AnalyzeSpecification(ctx)
);
```

## Generator Pipeline

The generator processes user code through two sequential stages. See [Generator Pipeline Architecture](generator-pipeline.md) for detailed patterns.

**Stage 1: Metadata Extraction** - Extract metadata from user code by parsing specifications, injectors, factories, and builders. This stage creates metadata models that mirror the syntactic structure found in user code.

**Stage 2: Code Generation and Rendering** - Transform metadata models into generated C# code. This stage maps metadata to output templates and renders them as `.generated.cs` files.

## Key Architectural Patterns

### Result Types and Diagnostic Recording

All analysis stages use `IResult<T>` to encapsulate values and diagnostics:

```csharp
// Pipeline model types use EquatableList (has structural equality for caching)
public record SpecificationAnalysisResult : IEquatable<SpecificationAnalysisResult> {
    public required string TypeName { get; init; }
    public required EquatableList<FactoryMetadata> Factories { get; init; }
    public required EquatableList<BuilderMetadata> Builders { get; init; }
}

// Results carry diagnostics alongside data
public static IResult<SpecificationAnalysisResult> Analyze(
    INamedTypeSymbol specification, 
    IDiagnosticsRecorder diagnostics) {
    // Use DiagnosticsRecorder.Capture to handle exceptions and collect diagnostics
    return DiagnosticsRecorder.Capture(recorder => {
        var factories = ExtractFactories(specification);
        var builders = ExtractBuilders(specification);
        return new SpecificationAnalysisResult {
            TypeName = specification.Name,
            Factories = new EquatableList<FactoryMetadata>(factories),
            Builders = new EquatableList<BuilderMetadata>(builders)
        };
    });
}
```

**Why**:
- `EquatableList<T>` supports structural equality (required for incremental caching)
- Standard `ImmutableArray` lacks structural equality
- `IResult<T>` encapsulates both success values and error diagnostics
- `DiagnosticsRecorder.Capture` automatically handles exceptions

### Diagnostic-First Error Handling

Errors are reported as diagnostics via `IDiagnosticsRecorder`, not by throwing exceptions:

```csharp
// Good: Report diagnostics and continue processing
public IResult<LinkingResult> LinkDependencies(
    SpecificationAnalysisResult spec,
    IDiagnosticsRecorder diagnostics) {
    var linked = new List<LinkedDependency>();
    
    foreach (var factory in spec.Factories) {
        if (!CanResolveFactory(factory)) {
            // Report diagnostic for this error, but continue
            diagnostics.Add(DiagnosticInfo.UnresolvableDependency(
                factory.Name, 
                factory.Location));
            continue;
        }
        linked.Add(LinkFactory(factory));
    }
    
    return Result.Ok(new LinkingResult(new EquatableList<LinkedDependency>(linked)));
}

// Lower-layer functions may throw for internal bugs (not user errors):
private string ExtractName(INamedTypeSymbol? symbol) {
    if (symbol is null) {
        // Internal bug: symbol shouldn't be null here
        throw new InvalidOperationException(
            "Symbol should not be null at this point");
    }
    return symbol.Name;
}
```

**Why**:
- Let users fix all errors at once, not one at a time
- Exceptions are caught by upper-layer `Capture` and converted to diagnostics
- Continue processing to provide comprehensive error feedback

### Extension Methods for Roslyn

Keep Roslyn symbol analysis in extension methods:

```csharp
// Good: Extension methods isolate Roslyn logic
public static bool IsSpecification(this INamedTypeSymbol symbol) =>
    symbol.GetAttributes()
        .Any(attr => attr.AttributeClass?.Name == "SpecificationAttribute");

public static ImmutableArray<MethodSymbol> GetFactories(
    this INamedTypeSymbol specification) =>
    specification.GetMembers()
        .OfType<IMethodSymbol>()
        .Where(m => m.IsFactory())
        .ToImmutableArray();

// Usage: Read like a sentence
var factories = specification.GetFactories();

// Avoid: Inline Roslyn code everywhere
foreach (var method in specification.GetMembers().OfType<IMethodSymbol>()) {
    if (method.GetAttributes().Any(attr => attr.AttributeClass?.Name == "FactoryAttribute")) {
        // Use method...
    }
}
```

## Directory Structure

```
Phx/
  Inject/
    Generator/
      Incremental/
        Stage1/                          # Metadata extraction
          Pipeline/
            Attributes/                  # Attribute transformers
            Specification/               # Specification metadata
            Injector/                    # Injector metadata
            Auto/                        # Auto-dependency metadata
          Model/
            Attributes/                  # Attribute metadata classes
        Stage2/                          # Code generation
          Core/
            Pipeline/
              SpecContainer/             # Specification container mappers
              Injector/                  # Injector mappers
            Model/                       # Generated output models
        Util/
          EquatableList.cs               # Custom collection with value equality
        Diagnostics/
          Result.cs                      # IResult interface
          DiagnosticsRecorder.cs         # Diagnostic collection
          DiagnosticInfo.cs              # Diagnostic definitions
      Extensions/
        RoslynExtensions.cs              # Symbol analysis helpers
      PhxInjectSourceGenerator.cs        # Main entrypoint
```

## Multi-Version Support

This project maintains legacy versions (`Phx.Inject.Generator.Legacy`) for backwards compatibility with older Roslyn versions. However, **primary development focuses on the current version** (`Phx.Inject.Generator`).

**Legacy versions are being phased out**. When adding features:

1. Implement in current version (`Phx.Inject.Generator`) - this is the focus
2. Legacy version updates are optional; if legacy cannot support the feature, that is acceptable
3. Run `Phx.Inject.Generator.Tests` (primary test suite)
4. Legacy test suite (`Phx.Inject.Generator.Tests.Legacy`) may be skipped if the feature is current-version only

Refer to [Architecture Guide](../../.agents/architecture.md) for additional context on version support.

## Testing Strategy

Testing strategy is not yet defined. See [Testing Standards](../../.agents/testing.md) for current status.

## Validation Checklist

Before completing generator changes:

- [ ] Incremental provider caching used where possible
- [ ] Predicates return early; expensive analysis only on filtered candidates
- [ ] No runtime exceptions; errors reported as diagnostics via `IDiagnosticsRecorder`
- [ ] `IResult<T>` used for encapsulating values and diagnostics
- [ ] Pipeline models use `EquatableList<T>` for structural equality (not `ImmutableArray`)
- [ ] `DiagnosticsRecorder.Capture` used for exception-safe execution
- [ ] Performance: large specifications generate quickly (< 500ms)
- [ ] Generated code is readable and formatted consistently
- [ ] Diagnostics are clear and actionable
- [ ] Documentation updated for new analysis rules
- [ ] No compiler warnings in generated code

## References

- **[Architecture Guide](../../.agents/architecture.md)**: System design overview
- **[Code Generation Practices](../../.agents/code-generation.md)**: Generated code standards
- **[Coding Standards](../../.agents/coding-standards.md)**: General C# guidelines
- **[Testing Standards](../../.agents/testing.md)**: Test practices
- **[Documentation Standards](../../.agents/documentation.md)**: Documentation requirements
- **[Phx.Inject AGENTS](../Phx.Inject/.agents/AGENTS.md)**: Public API contracts
