# Testing Standards for PhxInject

Comprehensive testing strategy for PhxInject projects covering unit tests, integration tests, source generator testing, and generated code validation.

## Testing Philosophy

1. **Test Behavior, Not Implementation**: Focus on observable outcomes, not internal details
2. **Fast Feedback**: Tests should run quickly; aim for <5 seconds for unit test suite
3. **Isolated Tests**: Each test should be independent and not rely on execution order
4. **Clear Failures**: Test failures should clearly indicate what went wrong
5. **Comprehensive Coverage**: Minimum 85% code coverage for all projects

## Test Project Structure

### Naming Conventions

- Test projects: `{ProjectName}.Tests` (e.g., `Phx.Inject.Tests`, `Phx.Inject.Generator.Tests`)
- Test files: `{TestedType}Tests.cs` (e.g., `SpecificationAnalyzerTests.cs`)
- Test methods: `MethodName_Scenario_ExpectedOutcome` (e.g., `Analyze_WhenFactoryReturnsVoid_ReportsDiagnostic`)

### File Organization

```
Phx.Inject.Generator.Tests/
├── Unit/
│   ├── Metadata/
│   │   ├── SpecificationAnalyzerTests.cs
│   │   └── FactoryMethodAnalyzerTests.cs
│   ├── Core/
│   │   └── CoreValidatorTests.cs
│   ├── Linking/
│   │   └── LinkResolverTests.cs
│   └── CodeGeneration/
│       └── InjectorGeneratorTests.cs
├── Integration/
│   ├── PipelineTests.cs
│   └── EndToEndTests.cs
├── Snapshots/
│   └── GeneratedCode/
│       ├── SimpleFactory.verified.cs
│       └── ComplexDependencies.verified.cs
└── Diagnostics/
    ├── ErrorReportingTests.cs
    └── DiagnosticValidationTests.cs
```

## Unit Testing

### General Patterns

Use xUnit for all tests:

```csharp
public class SpecificationAnalyzerTests
{
    [Fact]
    public void Analyze_WhenTypeHasSpecificationAttribute_ReturnsMetadata()
    {
        // Arrange
        var source = @"
            using Phx.Inject;
            
            [Specification]
            public class TestSpec { }
        ";
        var compilation = CreateCompilation(source);
        var analyzer = new SpecificationAnalyzer();
        
        // Act
        var result = analyzer.Analyze(compilation);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Specifications);
        Assert.Equal("TestSpec", result.Value.Specifications[0].Name);
    }
    
    [Fact]
    public void Analyze_WhenTypeIsNotPublic_ReportsDiagnostic()
    {
        // Arrange
        var source = @"
            using Phx.Inject;
            
            [Specification]
            internal class TestSpec { }
        ";
        var compilation = CreateCompilation(source);
        var analyzer = new SpecificationAnalyzer();
        
        // Act
        var result = analyzer.Analyze(compilation);
        
        // Assert
        Assert.False(result.IsSuccess);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(DiagnosticId.SpecificationMustBePublic, diagnostic.Id);
    }
}
```

### Theory Tests for Multiple Cases

Use `[Theory]` for testing multiple scenarios:

```csharp
[Theory]
[InlineData("int", true)]
[InlineData("string", true)]
[InlineData("void", false)]
[InlineData("Task", false)]
public void ValidateReturnType_VariousTypes_ReturnsExpectedResult(
    string returnType, 
    bool expectedValid)
{
    // Arrange
    var source = $@"
        using Phx.Inject;
        
        [Specification]
        public class TestSpec {{
            [Factory]
            public {returnType} GetValue() => default;
        }}
    ";
    var compilation = CreateCompilation(source);
    var validator = new CoreValidator();
    
    // Act
    var result = validator.ValidateFactoryReturnType(compilation);
    
    // Assert
    Assert.Equal(expectedValid, result.IsSuccess);
}
```

### Test Helpers

Create helper methods for common setup:

```csharp
public abstract class GeneratorTestBase
{
    protected static Compilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = new[] {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(SpecificationAttribute).Assembly.Location),
        };
        
        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
    
    protected static IResult<T> GetResult<T>(Compilation compilation, Func<Compilation, IResult<T>> analyzer)
    {
        return analyzer(compilation);
    }
}
```

## Source Generator Testing

### Roslyn Source Generator Testing Pattern

Test generators using `GeneratorDriver`:

```csharp
public class PhxInjectSourceGeneratorTests
{
    [Fact]
    public void Generate_SimpleSpecification_ProducesExpectedCode()
    {
        // Arrange
        var source = @"
            using Phx.Inject;
            
            [Specification]
            public class TestSpec
            {
                [Factory]
                public int GetInt() => 42;
            }
        ";
        
        // Act
        var (compilation, diagnostics) = RunGenerator(source);
        
        // Assert
        Assert.Empty(diagnostics);
        var generatedCode = GetGeneratedSource(compilation, "TestSpec.g.cs");
        Assert.Contains("public class TestSpecInjector", generatedCode);
        Assert.Contains("public int GetInt()", generatedCode);
    }
    
    private (Compilation, ImmutableArray<Diagnostic>) RunGenerator(string source)
    {
        var compilation = CreateCompilation(source);
        var generator = new PhxInjectSourceGenerator();
        
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);
        
        return (outputCompilation, diagnostics);
    }
    
    private string GetGeneratedSource(Compilation compilation, string fileName)
    {
        var generatedTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.EndsWith(fileName));
        
        Assert.NotNull(generatedTree);
        return generatedTree.ToString();
    }
}
```

### Incremental Generator Testing

Test incremental behavior:

```csharp
[Fact]
public void Generate_UnchangedSource_UsesCachedResults()
{
    // Arrange
    var source = @"
        using Phx.Inject;
        [Specification]
        public class TestSpec { }
    ";
    
    var compilation1 = CreateCompilation(source);
    var compilation2 = CreateCompilation(source); // Identical source
    
    var generator = new PhxInjectSourceGenerator();
    var driver = CSharpGeneratorDriver.Create(generator);
    
    // Act - First run
    driver = driver.RunGeneratorsAndUpdateCompilation(
        compilation1, out var output1, out _);
    
    // Act - Second run with identical input
    driver = driver.RunGeneratorsAndUpdateCompilation(
        compilation2, out var output2, out _);
    
    // Assert - Should use cached results
    var runResult = driver.GetRunResult();
    var generatorResult = runResult.Results[0];
    
    // Verify caching behavior (implementation-specific)
    Assert.True(generatorResult.TrackedSteps.All(step => step.Outputs.Any(o => o.Reason == IncrementalStepRunReason.Cached)));
}
```

## Snapshot Testing

### Using Verify Framework

Use Verify for snapshot testing generated code:

```csharp
[UsesVerify]
public class GeneratedCodeSnapshotTests
{
    [Fact]
    public Task SimpleFactory_GeneratesExpectedCode()
    {
        // Arrange
        var source = @"
            using Phx.Inject;
            
            [Specification]
            public class TestSpec
            {
                [Factory]
                public MyClass GetMyClass() => new MyClass();
            }
            
            public class MyClass { }
        ";
        
        // Act
        var (compilation, _) = RunGenerator(source);
        var generatedCode = GetGeneratedSource(compilation, "TestSpec.g.cs");
        
        // Assert - Verify will save snapshot on first run, compare on subsequent runs
        return Verifier.Verify(generatedCode)
            .UseDirectory("Snapshots/GeneratedCode");
    }
}
```

Snapshot files are stored as `.verified.cs` files and committed to source control.

### When to Use Snapshot Tests

- Testing complete generated code output
- Verifying formatting and style of generated code
- Catching unintended changes to generated code structure
- Documenting expected generator output

## Diagnostic Validation Testing

### Testing Error Reporting

Validate that diagnostics are reported correctly:

```csharp
public class DiagnosticTests
{
    [Fact]
    public void Generate_CircularDependency_ReportsDiagnostic()
    {
        // Arrange
        var source = @"
            using Phx.Inject;
            
            [Specification]
            public class TestSpec
            {
                [Factory]
                public A GetA(B b) => new A(b);
                
                [Factory]
                public B GetB(A a) => new B(a);
            }
        ";
        
        // Act
        var (_, diagnostics) = RunGenerator(source);
        
        // Assert
        var error = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticId.CircularDependency, error.Id);
        Assert.Equal(DiagnosticSeverity.Error, error.Severity);
        Assert.Contains("circular dependency", error.GetMessage().ToLower());
    }
    
    [Fact]
    public void Generate_UnresolvableDependency_ReportsDetailedDiagnostic()
    {
        // Arrange
        var source = @"
            using Phx.Inject;
            
            [Specification]
            public class TestSpec
            {
                [Factory]
                public A GetA(B b) => new A(b);
                // B is not defined as a factory
            }
        ";
        
        // Act
        var (_, diagnostics) = RunGenerator(source);
        
        // Assert
        var error = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticId.UnresolvableDependency, error.Id);
        Assert.Contains("B", error.GetMessage()); // Should mention missing type
    }
}
```

### Testing Diagnostic Location

Verify diagnostics point to correct source locations:

```csharp
[Fact]
public void Generate_InvalidFactoryReturn_DiagnosticPointsToMethod()
{
    // Arrange
    var source = @"
        using Phx.Inject;
        
        [Specification]
        public class TestSpec
        {
            [Factory]
            public void InvalidFactory() { } // Line 7
        }
    ";
    
    // Act
    var (_, diagnostics) = RunGenerator(source);
    
    // Assert
    var diagnostic = Assert.Single(diagnostics);
    Assert.Equal(7, diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1);
    Assert.Contains("InvalidFactory", diagnostic.Location.SourceTree.ToString());
}
```

## Integration Testing

### Full Pipeline Tests

Test complete generator pipeline:

```csharp
public class PipelineIntegrationTests
{
    [Fact]
    public void EndToEnd_ComplexSpecification_GeneratesWorkingInjector()
    {
        // Arrange - Complex scenario with multiple factories, builders, and dependencies
        var source = @"
            using Phx.Inject;
            
            [Specification]
            public class ComplexSpec
            {
                [Factory]
                public IService GetService(IRepository repo) => new ServiceImpl(repo);
                
                [Factory]
                public IRepository GetRepository() => new RepositoryImpl();
                
                [Builder]
                public void ConfigureService(IService service)
                {
                    ((ServiceImpl)service).Initialize();
                }
            }
        ";
        
        // Act
        var (compilation, diagnostics) = RunGenerator(source);
        
        // Assert
        Assert.Empty(diagnostics);
        
        // Verify generated code compiles successfully
        var compilationDiagnostics = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error);
        Assert.Empty(compilationDiagnostics);
        
        // Verify expected methods exist
        var generatedCode = GetGeneratedSource(compilation, "ComplexSpec.g.cs");
        Assert.Contains("public IService GetService()", generatedCode);
        Assert.Contains("public IRepository GetRepository()", generatedCode);
    }
}
```

### Cross-Specification Linking Tests

Test linking between specifications:

```csharp
[Fact]
public void Integration_LinkedSpecifications_ResolveDependencies()
{
    // Arrange
    var source = @"
        using Phx.Inject;
        
        [Specification]
        public class SpecA
        {
            [Factory]
            public TypeA GetTypeA() => new TypeA();
        }
        
        [Specification]
        [Link(typeof(SpecA))]
        public class SpecB
        {
            [Factory]
            public TypeB GetTypeB(TypeA a) => new TypeB(a); // Depends on SpecA
        }
    ";
    
    // Act
    var (compilation, diagnostics) = RunGenerator(source);
    
    // Assert
    Assert.Empty(diagnostics);
    var generatedB = GetGeneratedSource(compilation, "SpecB.g.cs");
    Assert.Contains("_specA.GetTypeA()", generatedB); // Should call linked spec
}
```

## Testing Best Practices

### Test Organization

1. **One Assert Per Test** (when possible): Makes failures clearer
2. **Arrange-Act-Assert**: Consistent structure for readability
3. **Descriptive Test Names**: `{Method}_{Scenario}_{ExpectedOutcome}`
4. **Test Edge Cases**: Empty inputs, null values, boundary conditions
5. **Test Error Paths**: Not just happy path

### Performance Testing

For generator performance:

```csharp
[Fact]
public void Performance_LargeSpecification_CompletesInReasonableTime()
{
    // Arrange - Generate source with 100 factories
    var factories = string.Join("\n", Enumerable.Range(0, 100)
        .Select(i => $"[Factory] public Type{i} GetType{i}() => new Type{i}();"));
    
    var source = $@"
        using Phx.Inject;
        
        [Specification]
        public class LargeSpec {{ {factories} }}
    ";
    
    var stopwatch = Stopwatch.StartNew();
    
    // Act
    var (_, diagnostics) = RunGenerator(source);
    
    // Assert
    stopwatch.Stop();
    Assert.Empty(diagnostics);
    Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
        $"Generator took {stopwatch.ElapsedMilliseconds}ms, expected <1000ms");
}
```

### Mocking and Test Doubles

Avoid mocking when possible; use real implementations. When mocking is necessary:

```csharp
public class LinkResolverTests
{
    [Fact]
    public void Resolve_WithMockTypeIndex_FindsLinkedTypes()
    {
        // Arrange
        var typeIndex = new TestTypeIndex();
        typeIndex.Add("SpecA", new SpecMetadata { Name = "SpecA" });
        
        var resolver = new LinkResolver(typeIndex);
        
        // Act
        var result = resolver.Resolve("SpecA");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("SpecA", result.Name);
    }
    
    private class TestTypeIndex : ITypeIndex
    {
        private readonly Dictionary<string, SpecMetadata> _specs = new();
        
        public void Add(string name, SpecMetadata spec) => _specs[name] = spec;
        public SpecMetadata? Find(string name) => _specs.TryGetValue(name, out var spec) ? spec : null;
    }
}
```

## Code Coverage Requirements

- **Minimum**: 85% code coverage for all projects
- **Target**: 90%+ for critical pipeline components
- **Generate Report**: Use `dotnet test --collect:"XPlat Code Coverage"`
- **Exclude From Coverage**:
  - Generated code (marked with `[ExcludeFromCodeCoverage]`)
  - Trivial properties (auto-implemented)
  - Diagnostic descriptor definitions

### Coverage Configuration

In `test.runsettings`:

```xml
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat Code Coverage">
        <Configuration>
          <Format>cobertura</Format>
          <Exclude>[*]*.g.cs</Exclude>
          <ExcludeByAttribute>ExcludeFromCodeCoverage,GeneratedCode</ExcludeByAttribute>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

## Continuous Integration

Tests run automatically on every PR:

1. **Unit Tests**: Run on every commit
2. **Integration Tests**: Run on every commit
3. **Snapshot Tests**: Verify snapshots match expected output
4. **Coverage Report**: Fail if below 85%
5. **Performance Tests**: Warn if significant regression

See [GitHub Actions CI/CD Standards](github-actions.instructions.md) for CI configuration.

## Validation Checklist

Before completing any feature:

- [ ] Unit tests cover new functionality
- [ ] Integration tests validate end-to-end behavior
- [ ] Snapshot tests added for generated code changes
- [ ] Diagnostic tests validate error reporting
- [ ] Edge cases tested (null, empty, boundary values)
- [ ] Error paths tested (invalid input, missing dependencies)
- [ ] Code coverage meets 85% minimum
- [ ] All tests pass locally
- [ ] Test names follow `Method_Scenario_ExpectedOutcome` pattern
- [ ] Tests are fast (<5 seconds for unit tests)
- [ ] Tests are isolated and order-independent

## Questions?

- For generator testing patterns: See [Code Generation Standards](code-generation.instructions.md)
- For performance testing: See [Performance Optimization](performance.instructions.md)
- For diagnostic patterns: See [Architecture Guide](architecture.instructions.md)
