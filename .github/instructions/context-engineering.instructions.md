# Context Engineering for Maximum Copilot Effectiveness

Best practices for structuring code, files, and projects to maximize GitHub Copilot's understanding and effectiveness in the PhxInject codebase.

## Context Engineering Philosophy

Copilot's effectiveness depends on the context it can see. Well-structured code with clear names, good locality, and explicit types gives Copilot better understanding and produces better suggestions.

## File Organization

### Colocate Related Code

**Principle**: Keep related code in the same file or nearby files

✅ **Good Structure**:
```
Metadata/
  SpecificationAnalyzer.cs       # Analyzer
  SpecificationMetadata.cs       # Data model
  SpecificationAnalyzerTests.cs  # Tests nearby
```

❌ **Poor Structure**:
```
Analyzers/
  SpecificationAnalyzer.cs       # Analyzer here
Models/
  SpecificationMetadata.cs       # Data model far away
Tests/Unit/Metadata/
  SpecificationAnalyzerTests.cs  # Tests separated
```

### Maintain Logical Folder Structure

Mirror architecture in folder structure:

```
Phx.Inject.Generator/
├── Metadata/           # Stage 1: Metadata Extraction
│   ├── SpecificationAnalyzer.cs
│   └── FactoryMethodAnalyzer.cs
├── Core/               # Stage 2: Core Analysis
│   ├── CoreValidator.cs
│   └── DependencyGraphBuilder.cs
├── Linking/            # Stage 3: Linking
│   └── LinkResolver.cs
├── CodeGeneration/     # Stage 4: Code Generation
│   └── InjectorGenerator.cs
└── Rendering/          # Stage 5: Rendering
    └── CodeRenderer.cs
```

This structure helps Copilot understand:
- Which component belongs to which pipeline stage
- Where to find related functionality
- How components interact

### Use Descriptive File Names

File names should indicate purpose:

✅ **Good**:
- `CircularDependencyDetector.cs` - Clear what it does
- `FactoryMethodMetadata.cs` - Clear data model
- `DiagnosticDescriptors.cs` - Clear diagnostic definitions

❌ **Poor**:
- `Detector.cs` - Detects what?
- `Metadata.cs` - Metadata for what?
- `Diagnostics.cs` - Diagnostic IDs? Descriptors? Utilities?

## Naming Conventions

### Semantic Names

Use names that indicate purpose and behavior:

```csharp
// ✅ GOOD: Names indicate purpose
public class SpecificationAnalyzer  // Analyzes specifications
{
    public IResult ExtractMetadata(INamedTypeSymbol symbol)  // Extracts metadata
    {
        var factories = GetFactoryMethods(symbol);  // Gets factory methods
        return BuildMetadata(symbol, factories);     // Builds metadata
    }
}

// ❌ POOR: Generic, unclear names
public class Analyzer  // Analyzes what?
{
    public IResult Process(INamedTypeSymbol symbol)  // Processes how?
    {
        var items = GetItems(symbol);  // What items?
        return Build(symbol, items);    // Builds what?
    }
}
```

### Type Suffixes

Use consistent type suffixes:

- `Analyzer` - Extracts and analyzes information
- `Validator` - Validates input/configuration
- `Generator` - Generates output (code, data)
- `Resolver` - Resolves dependencies/references
- `Builder` - Constructs complex objects
- `Metadata` - Data model for metadata
- `Result` - Result of operation
- `Tests` - Test class

```csharp
public class SpecificationAnalyzer { }     // Analyzes specifications
public class CoreValidator { }             // Validates core data
public class InjectorGenerator { }         // Generates injector code
public class LinkResolver { }              // Resolves links
public class DependencyGraphBuilder { }    // Builds dependency graph
public record FactoryMethodMetadata { }    // Metadata for factory method
public record AnalysisResult { }           // Result of analysis
public class CoreValidatorTests { }        // Tests for CoreValidator
```

### Explicit Types Over var

Use explicit types for better context:

```csharp
// ✅ GOOD: Explicit types give Copilot context
INamedTypeSymbol symbol = compilation.GetTypeByMetadataName("MyType");
IResult<SpecificationMetadata> result = analyzer.Analyze(symbol);
EquatableList<FactoryMethodMetadata> factories = GetFactories(symbol);

// ❌ POOR: var hides type information
var symbol = compilation.GetTypeByMetadataName("MyType");
var result = analyzer.Analyze(symbol);
var factories = GetFactories(symbol);
```

**Exception**: Use `var` when type is obvious from right side:
```csharp
var analyzer = new SpecificationAnalyzer();  // Type is clear
var factories = new List<FactoryMethodMetadata>();  // Type is clear
```

## Code Structure

### Small, Focused Methods

Break large methods into small, well-named methods:

```csharp
// ✅ GOOD: Small, focused methods
public IResult Analyze(INamedTypeSymbol symbol)
{
    return DiagnosticsRecorder.Capture(diagnostics => {
        ValidateSymbol(symbol, diagnostics);
        var factories = ExtractFactories(symbol);
        var builders = ExtractBuilders(symbol);
        return BuildMetadata(symbol, factories, builders);
    });
}

private void ValidateSymbol(INamedTypeSymbol symbol, DiagnosticsCollector diagnostics)
{
    if (!IsPublicOrInternal(symbol))
    {
        diagnostics.ReportError(DiagnosticId.SpecificationMustBePublic, ...);
    }
    
    if (symbol.IsStatic)
    {
        diagnostics.ReportError(DiagnosticId.SpecificationCannotBeStatic, ...);
    }
}

// ❌ POOR: Large, monolithic method
public IResult Analyze(INamedTypeSymbol symbol)
{
    return DiagnosticsRecorder.Capture(diagnostics => {
        // 100+ lines of validation, extraction, processing
        // Hard for Copilot to understand flow
    });
}
```

### Clear Dependencies

Make dependencies explicit via constructor:

```csharp
// ✅ GOOD: Dependencies explicit
public class LinkResolver
{
    private readonly ITypeIndex _typeIndex;
    private readonly IDiagnosticReporter _reporter;
    
    public LinkResolver(ITypeIndex typeIndex, IDiagnosticReporter reporter)
    {
        _typeIndex = typeIndex;
        _reporter = reporter;
    }
    
    public IResult ResolveLink(string typeName)
    {
        // Copilot knows we have _typeIndex and _reporter available
    }
}

// ❌ POOR: Hidden dependencies
public class LinkResolver
{
    public IResult ResolveLink(string typeName)
    {
        var typeIndex = GlobalTypeIndex.Instance;  // Where did this come from?
        var reporter = new DiagnosticReporter();   // Why create here?
    }
}
```

## Documentation

### XML Documentation

XML docs provide context to Copilot:

```csharp
/// <summary>
/// Analyzes a specification type and extracts metadata for code generation.
/// </summary>
/// <param name="symbol">The specification type symbol to analyze.</param>
/// <returns>
/// Success with <see cref="SpecificationMetadata"/> if valid,
/// or failure with diagnostics if invalid.
/// </returns>
/// <remarks>
/// Operates as part of Stage 1 (metadata extraction) of the generator pipeline.
/// Results are cached and reused across multiple generator invocations.
/// </remarks>
public IResult<SpecificationMetadata> Analyze(INamedTypeSymbol symbol)
{
    // Implementation
}
```

Copilot uses this context when:
- Calling this method
- Implementing similar methods
- Writing tests

### Inline Comments for Non-Obvious Code

```csharp
// ✅ GOOD: Explain why, not what
public void ProcessDependencies(IEnumerable<ITypeSymbol> dependencies)
{
    // Must iterate in reverse to handle removal during iteration
    var list = dependencies.ToList();
    for (int i = list.Count - 1; i >= 0; i--)
    {
        if (ShouldRemove(list[i]))
        {
            list.RemoveAt(i);
        }
    }
}

// ❌ POOR: Obvious comment
public void ProcessDependencies(IEnumerable<ITypeSymbol> dependencies)
{
    // Loop through dependencies
    foreach (var dep in dependencies)
    {
        // Process the dependency
        Process(dep);
    }
}
```

## Pattern Consistency

### Establish and Follow Patterns

Once a pattern is established, follow it consistently:

**Error Handling Pattern**:
```csharp
// Established pattern: Use DiagnosticsRecorder.Capture
public IResult Validate(InputData input)
{
    return DiagnosticsRecorder.Capture(diagnostics => {
        // Validation logic
        // Reports errors via diagnostics.ReportError()
    });
}
```

**All similar methods should follow this pattern**. Copilot learns from consistency.

### Common Patterns in PhxInject

1. **Pipeline transformations**: Use `Select`, `Combine`, `Where` for incremental providers
2. **Error reporting**: Use `DiagnosticsRecorder.Capture` with `IResult<T>`
3. **Symbol equality**: Use `SymbolEqualityComparer.Default`
4. **Immutable collections**: Use `EquatableList<T>` for pipeline models
5. **Testing**: Use `Arrange-Act-Assert` structure with descriptive test names

## Project-Level Context Files

### COPILOT.md Files

Create `COPILOT.md` files in key directories to provide context:

**Example: `Phx.Inject.Generator/Metadata/COPILOT.md`**:
```markdown
# Metadata Extraction (Stage 1)

This directory contains components for Stage 1 of the generator pipeline.

## Purpose

Extracts raw metadata from specification types:
- Identifies types with [Specification] attribute
- Extracts factory and builder methods
- Captures attribute arguments and locations

## Key Types

- `SpecificationAnalyzer` - Main entry point, orchestrates extraction
- `FactoryMethodAnalyzer` - Extracts factory method metadata
- `AttributeReader` - Parses attribute arguments

## Patterns

- Use incremental providers for caching
- Early filtering with syntax predicates
- Return `IResult<T>` for validation errors
- Cache expensive symbol operations

## Testing

See `SpecificationAnalyzerTests.cs` for examples.
```

### README.md in Key Directories

Provide high-level context in README files:

```markdown
# Phx.Inject.Generator

Roslyn incremental source generator for PhxInject.

## Architecture

Five-stage pipeline:
1. Metadata Extraction - `Metadata/`
2. Core Analysis - `Core/`
3. Linking - `Linking/`
4. Code Generation - `CodeGeneration/`
5. Rendering - `Rendering/`

## Key Concepts

- **Incremental Providers**: Use Roslyn's caching for performance
- **Diagnostic-Driven**: Report errors as diagnostics, never throw
- **EquatableList<T>**: Use for pipeline models (structural equality)

See `.agents/AGENTS.md` for detailed instructions.
```

## Validation Checklist

Good context engineering:

- [ ] **Related code colocated** - Files in logical folders
- [ ] **Descriptive names** - Types, methods, variables indicate purpose
- [ ] **Explicit types** - Prefer explicit over `var` (except obvious cases)
- [ ] **Small methods** - Each does one thing with clear name
- [ ] **Clear dependencies** - Constructor injection, no hidden dependencies
- [ ] **XML documentation** - Public APIs and complex internals documented
- [ ] **Inline comments** - Explain non-obvious "why"
- [ ] **Pattern consistency** - Established patterns followed throughout
- [ ] **COPILOT.md files** - Key directories have context files
- [ ] **README.md files** - High-level architecture documented

## Anti-Patterns

❌ **Large files with multiple concerns**:
```
Utils.cs   // 5000 lines, everything mixed together
```

❌ **Generic names**:
```csharp
public class Helper { }
public void Process(object data) { }
var result = DoStuff();
```

❌ **Hidden context**:
```csharp
// No docs, no types, cryptic names
var x = Get(y);
Process(x);
```

❌ **Inconsistent patterns**:
```csharp
// Method 1 uses DiagnosticsRecorder.Capture
// Method 2 throws exceptions
// Method 3 returns custom Result type
```

## Questions?

- For architectural structure: See [Architecture Guide](architecture.instructions.md)
- For naming conventions: See [Coding Standards](coding-standards.instructions.md)
- For documentation guidelines: See [Documentation Standards](documentation.instructions.md)
