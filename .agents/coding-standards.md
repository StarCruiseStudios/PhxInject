# C# Coding Standards for PhxInject

Standards for writing C# code in this repository. These apply to all projects except generated code (see [Code Generation Practices](code-generation.md)).

## General Principles

1. **Clarity First**: Code should be obviously correct to a reader familiar with C#
2. **Consistency**: Follow patterns established in the file/project
3. **No Clever Code**: Future maintainers, not just you, must understand the code
4. **Use var Appropriately**: Use `var` for local variables when type is clear from context
5. **Performance Awareness**: Generator code is performance-critical; be conscious of allocations

## File Organization

### Structure Order

Files should follow this organization:

```csharp
// 1. Copyright and license header

// 2. Using statements (alphabetically, System first)
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// 3. Namespace declaration
namespace Phx.Inject.Generator;

// 4. Type declaration(s)
public class MyType { ... }
```

### Using Statements

- Group System namespaces first, then others
- Sort alphabetically within groups
- Remove unused namespaces
- Use global using statements in generated analyzers only (for code gen efficiency)

## Naming Conventions

### Types

- **Classes**: PascalCase (MyClass, SpecificationAnalyzer)
- **Interfaces**: PascalCase with I prefix (IInjector, ISpecification)
- **Records**: PascalCase (DiagnosticData, AnalysisResult)
- **Enums**: PascalCase (InstantiationMode, FilterKind)
- **Enum Values**: PascalCase (Static, Instantiated, Dependency)

### Members

- **Public fields/properties**: PascalCase (MyProperty, CurrentValue)
- **Private fields**: camelCase (internalState, cache)
- **Constants**: PascalCase (PipelineVersion, DefaultTimeout)
- **Local variables**: camelCase (myLocal, result)
- **Parameters**: camelCase (targetType, configuration)
- **Methods**: PascalCase (AnalyzeSpecification, GetDependencies)

### Files

- File name matches primary type (MyType.cs)
- Nested types use dot notation (MyType.NestedType.cs)
- If multiple related types, use grouping folder

## Type Declarations

### Visibility

**Source Generator Projects** (Phx.Inject.Generator, Phx.Inject.Generator.Legacy):
- All types MUST be `internal` - source generators should not expose public APIs
- Only the generated code (output) is public to consuming projects

**Library Projects** (Phx.Inject, Phx.Inject.Legacy):
- Use appropriate visibility based on API design
- Default to `internal` unless type is part of public API

### Classes

Use sealed classes by default:

```csharp
// Prefer this:
public sealed class MyAnalyzer

// Only unseal if subclassing is intended and documented
public class BaseAnalyzer { }
```

### Records

Use records for data containers:

```csharp
// Good: immutable data
public record AnalysisResult(string TypeName, ImmutableArray<FactoryMetadata> Factories);

// Good: with init-only properties for complex cases
public record AnalysisResult {
    public string TypeName { get; init; }
    public ImmutableArray<FactoryMetadata> Factories { get; init; }
}

// Avoid: don't mutate record in post-construction
var result = new AnalysisResult({ ... });
result.TypeName = "Changed"; // No, immutable
```

### Enums

Always specify underlying type for flags:

```csharp
[Flags]
public enum AccessFlags : ulong {
    Public = 1,
    Internal = 2,
    Private = 4,
}
```

## Variables and Constants

### var Usage

Use `var` when:
- Type is obvious from assignment: `var result = GetAnalysisResult()`
- Return type is complex: `var services = _specification.GetServices()`
- In LINQ where clarity is better
- Type is explicitly stated on the right side: `var list = new List<string>()`

Use explicit types when:
- Numeric literals need specific types: `long value = 100;` (not `var value = 100;`)
- Type is non-obvious and aids readability: `INamedTypeSymbol symbol = ...`

## Null Handling

### Use Nullability Annotations

Enable nullable reference types project-wide (should be enabled):

```csharp
// Good: explicit about nullability
public string GetName() { ... }        // Never null
public string? GetOptionalName() { ... } // May be null

public void Process(MyType value) { }      // Value must not be null
public void Process(MyType? value) { }     // Value may be null
```

## Collections

### Use Immutable Collections

For API surfaces and cached data:

```csharp
// Phx.Inject.Generator commonly uses:
using System.Collections.Immutable;
using Phx.Inject.Generator.Incremental.Util;

// For general data structures (not pipeline models):
public record AnalysisResult(
    string TypeName,
    ImmutableDictionary<string, TypeMetadata> TypeIndex
);

// Avoid: mutable lists in public APIs
public List<string> Names { get; } // Not this
```

**Exception for Pipeline Model Types**: Standard `System.Collections.Immutable` types lack structural equality and are unsuitable for pipeline model types used with Roslyn's incremental generators. Use the `EquatableList<T>` type defined in this project instead.

```csharp
// WRONG: Do not use ImmutableArray in pipeline models
public record PipelineData(ImmutableArray<SpecFactoryMethodMetadata> Factories); // ✗

// CORRECT: Use EquatableList for pipeline models
public record PipelineData(EquatableList<SpecFactoryMethodMetadata> Factories); // ✓
```

### LINQ Usage

Prefer LINQ for transformations:

```csharp
// Good: clear transformation
var factoryOutputTypes = specification.Methods
    .Where(m => m.HasAttribute("Factory"))
    .Select(m => m.ReturnType)
    .Distinct()
    .ToImmutableArray();

// Avoid: manual iteration when LINQ is clearer
var list = new List<ITypeSymbol>();
foreach (var method in specification.Methods) {
    if (method.HasAttribute("Factory")) {
        list.Add(method.ReturnType);
    }
}
```

## Error Handling

### Generator Exception Policy

The generator must never throw an exception to the compiler. Instead:

1. **Lower-layer functions** may throw exceptions when an exceptional case occurs and the current type cannot continue to process
2. **Upper-layer functions** (pipeline stages) MUST catch all exceptions and convert them to diagnostics
3. **User errors** (invalid configurations, dependency issues) should be reported via diagnostics on result types, not exceptions
4. **Continue processing**: Attempt to continue parsing and processing to catch as many errors as possible before exiting

```csharp
// Lower layer: May throw for truly exceptional cases
private string ExtractName(INamedTypeSymbol? symbol) {
    // Exceptional case: this indicates a bug in our code, not user error
    if (symbol is null) {
        throw new InvalidOperationException(
            "Symbol should not be null at this point in the pipeline");
    }
    return symbol.Name;
}

// Upper layer: Catches exceptions and converts to diagnostics, using IResult
public IResult Analyze(InputData input) {
    var diagnostics = new List<DiagnosticData>();
    AnalysisData? data = null;
    
    try {
        data = PerformAnalysis(input);
    } catch (Exception ex) {
        // Convert exception to diagnostic instead of throwing
        diagnostics.Add(new DiagnosticData(
            DiagnosticId.InternalError,
            "An internal error occurred during analysis: " + ex.Message,
            input.Location));
        // Continue gracefully, return failure result
        return new AnalysisFailure(new EquatableList<DiagnosticData>(diagnostics));
    }
    
    return diagnostics.Count > 0
        ? new AnalysisFailure(new EquatableList<DiagnosticData>(diagnostics))
        : new AnalysisSuccess(data!);
}

// User errors: Reported via diagnostics, not exceptions
public void ProcessFactory(FactoryMetadata factory, DiagnosticsCollector diagnostics) {
    if (!IsValidReturn(factory.ReturnType)) {
        // User error: report diagnostic and continue
        diagnostics.ReportError(
            DiagnosticId.InvalidFactoryReturn,
            $"Factory method '{factory.Name}' has invalid return type",
            factory.Location);
        return; // Continue processing other factories
    }
}

// Avoid: exceptions for normal control flow or user errors
if (!TryParseConfig(input)) {
    throw new Exception("Bad config"); // Wrong! Report diagnostic instead
}
```

### Result Types for Validation

For expected validation failures, use the existing `IResult<T>` interface:

```csharp
// Good: Use DiagnosticsRecorder.Capture pattern with IResult interface
public IResult<AnalysisData> Analyze(InputData input) {
    // Capture creates a recorder and invokes the lambda
    // Pass the diagnostics recorder to functions that produce diagnostics
    return DiagnosticsRecorder.Capture(diagnostics => {
        var data = PerformAnalysis(input);
        // Call GetValue on IResult to extract value and record diagnostics
        var validated = ValidateData(data).GetValue(diagnostics);
        return validated;
    });
}

// Avoid: custom result type definitions
// The IResult<T> interface is already defined in the codebase
```

## Properties

### Preferred Forms

```csharp
// Good: simple property
public string Name { get; init; }

// Good: computed property
public bool IsValid => !string.IsNullOrEmpty(Name);

// Good: with validator
private string _name = "";
public string Name {
    get => _name;
    init => _name = value ?? throw new ArgumentNullException(nameof(value));
}

// Avoid: complex logic in property getter
public DateTime CreatedDate {
    get {
        // Multiple statements? Use a method instead
        if (_created.Year == DateTime.Now.Year) { ... }
        return _created;
    }
}
```

## Async Code

### Naming

```csharp
// Good: Async suffix for async methods
public async Task<AnalysisResult> AnalyzeAsync(INamedTypeSymbol spec)
public async Task<string> GetNameAsync()

// Avoid: omitting Async suffix
public async Task<AnalysisResult> Analyze(INamedTypeSymbol spec) // Wrong!
```

### Patterns

```csharp
// Good: Use ConfigureAwait(false) in library code
public async Task<AnalysisResult> AnalyzeAsync(INamedTypeSymbol spec) {
    var data = await LoadDataAsync().ConfigureAwait(false);
    return await ProcessAsync(data).ConfigureAwait(false);
}

// Good: Return ValueTask for hot paths
public ValueTask<ISymbol?> ResolveSymbolAsync(string name) {
    if (_cache.TryGetValue(name, out var symbol)) {
        return new ValueTask<ISymbol?>(symbol);
    }
    return new ValueTask<ISymbol?>(ResolveAsync(name));
}
```

## Comments and Documentation

1. **Don't explain obvious code**: `x = x + 1; // Increment x` is noise
2. **Explain intent and non-obvious behavior**:
   ```csharp
   // Must iterate in reverse to avoid index shifting when removing
   for (int i = items.Count - 1; i >= 0; i--) { }
   ```
3. **Document edge cases**:
   ```csharp
   // Empty specifications are valid; no factories is acceptable
   if (spec.Factories.Length == 0) { }
   ```
4. **Use XML documentation** for public APIs (see [Documentation Standards](documentation.md))

## Formatting

### Spacing

- Use 4-space indentation (configured in .editorconfig)
- One blank line between methods
- No trailing whitespace
- Files end with newline

### Braces

Use Allman style (enforced by .editorconfig):

```csharp
if (condition)
{
    // Brace on new line
}
else
{
    // Else on separate line
}
```

### Line Length

- Prefer lines under 100 characters
- Exceeding 120 characters should be rare
- Break long parameter lists across lines

## Validation Checklist

Before completing any coding task:

- [ ] Following naming conventions throughout
- [ ] No unnecessary `var` usage
- [ ] Collections use immutable types where appropriate
- [ ] Null handling correct and explicit
- [ ] Methods are under 30 lines (or well-justified)
- [ ] No comments stating the obvious
- [ ] XML docs for public types/methods
- [ ] No trailing whitespace
- [ ] Error handling appropriate for context
