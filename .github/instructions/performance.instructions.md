# Performance Optimization for PhxInject

Performance optimization guidelines for PhxInject source generators and generated code. Focus on build-time performance (generator execution) and runtime performance (generated injector code).

## Performance Philosophy

1. **Measure First**: Profile before optimizing
2. **Incremental by Default**: Use Roslyn incremental APIs for caching
3. **Pay-Per-Use**: Only compute what's needed
4. **Zero Runtime Overhead**: All dependency resolution at compile time
5. **Fast Builds**: Generator should add <500ms to build time

## Source Generator Performance

### Incremental Generator Patterns

**Critical**: Always use incremental providers to enable Roslyn's caching:

```csharp
// ✅ GOOD: Incremental providers with caching
[Generator]
public class PhxInjectGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Roslyn caches results based on input equality
        var specifications = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Phx.Inject.SpecificationAttribute",
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: (ctx, _) => ExtractMetadata(ctx))
            .Where(spec => spec is not null);
        
        // Each .Select/.Where creates a cached transformation point
        var validated = specifications
            .Select((spec, _) => Validate(spec));
        
        var generated = validated
            .Select((spec, _) => GenerateCode(spec));
        
        context.RegisterSourceOutput(generated, (ctx, code) => {
            ctx.AddSource(code.FileName, code.Source);
        });
    }
}

// ❌ AVOID: Non-incremental approach
[Generator]
public class SlowGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // Recomputes everything on every change
        var specs = FindAllSpecifications(context.Compilation);
        foreach (var spec in specs)
        {
            GenerateCode(spec);
        }
    }
}
```

### Early Filtering with Predicates

**Critical**: Filter syntax nodes as early as possible:

```csharp
// ✅ GOOD: Quick syntactic check before semantic analysis
var specifications = context.SyntaxProvider
    .ForAttributeWithMetadataName(
        "Phx.Inject.SpecificationAttribute",
        predicate: (node, _) => {
            // Fast syntactic checks only
            return node is ClassDeclarationSyntax cls
                && cls.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword) 
                                       || m.IsKind(SyntaxKind.InternalKeyword));
        },
        transform: (ctx, _) => {
            // Expensive semantic analysis only for filtered nodes
            return ExtractSemanticMetadata(ctx);
        });

// ❌ AVOID: Always allowing semantic analysis
var specifications = context.SyntaxProvider
    .ForAttributeWithMetadataName(
        "Phx.Inject.SpecificationAttribute",
        predicate: (node, _) => true, // No filtering!
        transform: (ctx, _) => ExtractSemanticMetadata(ctx));
```

### Avoid Expensive Operations

**Critical**: Don't enumerate all symbols or perform expensive operations repeatedly:

```csharp
// ❌ AVOID: Enumerating all compilation symbols
public void Analyze(Compilation compilation)
{
    var allTypes = compilation.GetSymbolsWithName(
        _ => true, 
        SymbolFilter.Type); // Expensive!
    
    foreach (var type in allTypes)
    {
        if (HasAttribute(type, "SpecificationAttribute"))
        {
            Process(type);
        }
    }
}

// ✅ GOOD: Use ForAttributeWithMetadataName
var specifications = context.SyntaxProvider
    .ForAttributeWithMetadataName(
        "Phx.Inject.SpecificationAttribute",
        predicate: (node, _) => node is ClassDeclarationSyntax,
        transform: (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol(ctx.TargetNode));
```

### Cache Expensive Computations

```csharp
// ✅ GOOD: Cache fully qualified names
private static readonly Dictionary<int, string> _nameCache = new();

private static string GetFullyQualifiedName(INamedTypeSymbol symbol)
{
    var key = SymbolEqualityComparer.Default.GetHashCode(symbol);
    if (_nameCache.TryGetValue(key, out var cached))
    {
        return cached;
    }
    
    var name = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    _nameCache[key] = name;
    return name;
}

// ❌ AVOID: Recomputing on every call
private static string GetFullyQualifiedName(INamedTypeSymbol symbol)
{
    return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
}
```

### Memory Efficiency

**Use Span<T> for symbol traversal**:

```csharp
// ✅ GOOD: Use Span for zero-allocation traversal
public void ProcessParameters(IMethodSymbol method)
{
    ReadOnlySpan<IParameterSymbol> parameters = method.Parameters.AsSpan();
    foreach (var param in parameters)
    {
        ProcessParameter(param);
    }
}

// ❌ AVOID: LINQ allocations in hot paths
public void ProcessParameters(IMethodSymbol method)
{
    var validParams = method.Parameters
        .Where(p => p.Type != null)
        .Select(p => p.Name)
        .ToList(); // Multiple allocations
}
```

### Pipeline Model Equality

**Critical**: Use EquatableList for structural equality in pipeline models:

```csharp
// ✅ GOOD: EquatableList provides structural equality for Roslyn caching
public record SpecificationMetadata(
    string Name,
    EquatableList<FactoryMethodMetadata> Factories
);

// ❌ AVOID: ImmutableArray lacks structural equality
public record SpecificationMetadata(
    string Name,
    ImmutableArray<FactoryMethodMetadata> Factories // Won't cache properly!
);
```

### Batch Operations

```csharp
// ✅ GOOD: Combine multiple inputs before processing
var combinedData = specifications
    .Combine(context.CompilationProvider)
    .Select((pair, _) => {
        var (spec, compilation) = pair;
        return Analyze(spec, compilation);
    });

// ❌ AVOID: Repeated compilation access
var analyzed = specifications
    .Select((spec, _) => {
        var compilation = GetCompilation(); // Expensive repeated access
        return Analyze(spec, compilation);
    });
```

## Generated Code Performance

### Zero-Allocation Factories

**Goal**: Generated injector code should have zero allocations for simple factories:

```csharp
// ✅ GOOD: Direct call, no allocations
public int GetInt() => TestSpecification.GetIntValue();

// ✅ GOOD: Single allocation for instance
public MyClass GetMyClass()
{
    return TestSpecification.CreateMyClass();
}

// ❌ AVOID: Unnecessary wrapper allocations
public MyClass GetMyClass()
{
    return () => TestSpecification.CreateMyClass()(); // Extra lambda allocation
}
```

### Dependency Chain Optimization

**Reuse dependencies when safe**:

```csharp
// ✅ GOOD: Shared dependency resolved once
public A GetA()
{
    var shared = GetSharedDependency();
    var b = GetB(shared);
    var c = GetC(shared); // Reuse
    return new A(b, c);
}

// ❌ AVOID: Resolving same dependency multiple times
public A GetA()
{
    var b = GetB(GetSharedDependency());
    var c = GetC(GetSharedDependency()); // Duplicate resolution
    return new A(b, c);
}
```

### Inline Small Methods

```csharp
// ✅ GOOD: Expression-bodied member for simple getters
public int GetValue() => 42;

// ✅ GOOD: Small method body
public MyClass GetMyClass()
{
    var dep = GetDependency();
    return new MyClass(dep);
}

// ❌ AVOID: Unnecessary method extraction for trivial operations
public int GetValue()
{
    return GetValueInternal();
}
private int GetValueInternal() => 42;
```

### Avoid Reflection

**Critical**: All type resolution at compile time:

```csharp
// ✅ GOOD: Direct type reference
public IService GetService() => new ServiceImplementation();

// ❌ AVOID: Runtime reflection
public IService GetService()
{
    var type = Type.GetType("ServiceImplementation");
    return (IService)Activator.CreateInstance(type);
}
```

## Performance Testing

### Benchmark Generator Performance

```csharp
[Fact]
public void Performance_LargeSpecification_CompletesQuickly()
{
    // Arrange - 100 factories
    var source = GenerateLargeSpecification(factoryCount: 100);
    var stopwatch = Stopwatch.StartNew();
    
    // Act
    RunGenerator(source);
    
    // Assert
    stopwatch.Stop();
    Assert.True(stopwatch.ElapsedMilliseconds < 1000,
        $"Generator took {stopwatch.ElapsedMilliseconds}ms");
}
```

### Profile with BenchmarkDotNet

For detailed profiling:

```csharp
[MemoryDiagnoser]
public class GeneratorBenchmarks
{
    [Benchmark]
    public void GenerateSimpleSpecification()
    {
        var source = @"
            [Specification]
            public class TestSpec {
                [Factory] public int GetInt() => 42;
            }
        ";
        RunGenerator(source);
    }
}
```

## Performance Checklist

### Generator Performance

- [ ] **Incremental providers used** - `Select`, `Combine`, `Where` for caching
- [ ] **Early filtering with predicates** - Syntactic checks before semantic analysis
- [ ] **No symbol enumeration** - Avoid `GetSymbolsWithName(_ => true)`
- [ ] **Expensive operations cached** - Fully qualified names, symbol lookups
- [ ] **EquatableList for pipeline models** - Not ImmutableArray
- [ ] **Memory efficient** - Use `Span<T>` for traversal
- [ ] **Batch operations** - Combine inputs before processing

### Generated Code Performance

- [ ] **Zero allocations for simple factories** - Direct method calls
- [ ] **Dependency chains optimized** - Shared dependencies resolved once
- [ ] **Small methods inlined** - Expression-bodied members used
- [ ] **No reflection** - All type resolution at compile time
- [ ] **Generated code compiles efficiently** - No unnecessary complexity

## Performance Targets

### Generator Execution Time

- **Small projects** (<10 specifications): <100ms overhead
- **Medium projects** (10-50 specifications): <250ms overhead
- **Large projects** (50+ specifications): <500ms overhead

### Generated Code Performance

- **Simple factory calls**: 0 allocations beyond instance creation
- **Complex dependency chains**: Linear in dependency count
- **Injector instantiation**: Constant time (O(1))

## Common Performance Pitfalls

❌ **Don't repeat expensive operations**:
```csharp
for (int i = 0; i < symbols.Length; i++)
{
    var name = symbols[i].ToDisplayString(); // Expensive, repeated
    if (IsValid(name)) { ... }
}
```

✅ **Cache results**:
```csharp
var names = symbols.Select(s => s.ToDisplayString()).ToArray();
for (int i = 0; i < names.Length; i++)
{
    if (IsValid(names[i])) { ... }
}
```

❌ **Don't allocate in hot paths**:
```csharp
foreach (var method in methods)
{
    var parameters = method.Parameters.ToList(); // Allocation per iteration
    Process(parameters);
}
```

✅ **Reuse or avoid allocations**:
```csharp
foreach (var method in methods)
{
    ProcessSpan(method.Parameters.AsSpan()); // No allocation
}
```

## Profiling Tools

### Roslyn Performance

Use Roslyn's built-in timing:

```
set DOTNET_EnableEventPipe=1
set DOTNET_EventPipeConfig=Microsoft-CodeAnalysis-General:Informational
dotnet build
```

### .NET Profiling

```powershell
# Memory profiling
dotnet-trace collect --process-id <pid> --providers Microsoft-Windows-DotNETRuntime:0x1:4

# CPU profiling
dotnet-trace collect --process-id <pid> --profile cpu-sampling
```

## Questions?

- For incremental generator patterns: See [Code Generation Standards](code-generation.instructions.md)
- For testing performance: See [Testing Standards](testing.instructions.md)
- For generator architecture: See [Architecture Guide](architecture.instructions.md)
