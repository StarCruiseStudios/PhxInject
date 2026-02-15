# Security Guidelines for PhxInject

Security guidelines for PhxInject source generators and dependency injection framework. Covers source generator security, generated code safety, and DI-specific concerns.

## Security Philosophy

1. **Validate Early**: Catch malicious input at generation time, not runtime
2. **Fail Safe**: Invalid input should produce diagnostics, never unsafe code
3. **Principle of Least Privilege**: Generated code should have minimal capabilities
4. **Defense in Depth**: Multiple layers of validation
5. **No Trust Boundaries**: Source generators run with full compiler trust

## Source Generator Security

### Trust Model

**Critical Understanding**: Source generators execute with full trust during compilation:

- Run in the compiler process with full file system access
- Can access entire source tree
- Can execute arbitrary code during build
- No sandboxing or isolation

**Implications**:
- Validate all user input (attributes, code structures)
- Never execute user-provided code paths
- Be cautious with file I/O and external dependencies

### Input Validation

**Critical**: Validate all user-provided input at generation time:

```csharp
// ✅ GOOD: Validate before processing
public IResult ValidateSpecification(INamedTypeSymbol symbol)
{
    return DiagnosticsRecorder.Capture(diagnostics => {
        // Validate visibility
        if (!symbol.DeclaredAccessibility.HasFlag(Accessibility.Public) &&
            !symbol.DeclaredAccessibility.HasFlag(Accessibility.Internal))
        {
            diagnostics.ReportError(
                DiagnosticId.SpecificationMustBePublicOrInternal,
                $"Specification '{symbol.Name}' must be public or internal",
                symbol.Locations[0]);
        }
        
        // Validate it's not in a restricted namespace
        if (symbol.ContainingNamespace.ToDisplayString().StartsWith("System."))
        {
            diagnostics.ReportError(
                DiagnosticId.InvalidNamespace,
                "Specifications cannot be in System namespace",
                symbol.Locations[0]);
        }
        
        return new SpecificationMetadata(symbol);
    });
}

// ❌ AVOID: Trusting user input
public void GenerateCode(INamedTypeSymbol symbol)
{
    // No validation - assumes input is safe
    var code = $"public class {symbol.Name}Injector {{ }}";
    EmitCode(code);
}
```

### Prevent Code Injection

**Critical**: Never use string concatenation for user-provided names:

```csharp
// ❌ DANGER: Code injection vulnerability
public string GenerateMethod(string methodName)
{
    return $"public void {methodName}() {{ }}"; // methodName could be "Foo() {{ MaliciousCode(); }} void Bar"
}

// ✅ GOOD: Validate identifiers
public string GenerateMethod(string methodName)
{
    if (!IsValidIdentifier(methodName))
    {
        throw new ArgumentException($"Invalid identifier: {methodName}");
    }
    
    return $"public void {methodName}() {{ }}";
}

private bool IsValidIdentifier(string name)
{
    if (string.IsNullOrWhiteSpace(name)) return false;
    if (!char.IsLetter(name[0]) && name[0] != '_') return false;
    
    return name.All(c => char.IsLetterOrDigit(c) || c == '_');
}

// ✅ BETTER: Use Roslyn syntax factory
public MethodDeclarationSyntax GenerateMethod(string methodName)
{
    return SyntaxFactory.MethodDeclaration(
        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
        SyntaxFactory.Identifier(methodName)) // Roslyn validates identifier
        .WithModifiers(SyntaxFactory.TokenList(
            SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
        .WithBody(SyntaxFactory.Block());
}
```

### Avoid Path Traversal

**If generator accesses files** (generally avoid):

```csharp
// ❌ DANGER: Path traversal vulnerability
public void LoadConfig(string fileName)
{
    var path = Path.Combine(projectDirectory, fileName); // fileName could be "../../etc/passwd"
    var content = File.ReadAllText(path);
}

// ✅ GOOD: Validate path stays within project
public void LoadConfig(string fileName)
{
    var fullPath = Path.GetFullPath(Path.Combine(projectDirectory, fileName));
    if (!fullPath.StartsWith(projectDirectory))
    {
        throw new SecurityException($"Path traversal detected: {fileName}");
    }
    
    var content = File.ReadAllText(fullPath);
}

// ✅ BETTER: Don't access files directly
// Use additional files or analyzer config instead
public void LoadConfig(IncrementalGeneratorInitializationContext context)
{
    var configProvider = context.AdditionalTextsProvider
        .Where(text => Path.GetFileName(text.Path).Equals("phxinject.config.json"))
        .Select((text, _) => text.GetText()?.ToString());
    
    // Process config safely
}
```

## Generated Code Security

### Avoid Generating Unsafe Code

**Critical**: Never generate `unsafe` code blocks:

```csharp
// ❌ DANGER: Generating unsafe code
var code = @"
    public unsafe int* GetPointer() {
        int value = 42;
        return &value;
    }
";

// ✅ GOOD: Only generate safe, managed code
var code = @"
    public int GetValue() {
        return 42;
    }
";
```

### Validate Type Resolution

**Ensure all generated code uses valid, resolvable types**:

```csharp
// ✅ GOOD: Validate type exists and is accessible
public IResult<string> GenerateFactoryCall(IMethodSymbol method)
{
    return DiagnosticsRecorder.Capture(diagnostics => {
        var returnType = method.ReturnType;
        
        // Check type is accessible
        if (!IsAccessible(returnType))
        {
            diagnostics.ReportError(
                DiagnosticId.TypeNotAccessible,
                $"Return type '{returnType}' is not accessible",
                method.Locations[0]);
        }
        
        // Check type can be constructed
        if (returnType is INamedTypeSymbol namedType && 
            !CanConstruct(namedType))
        {
            diagnostics.ReportError(
                DiagnosticId.TypeNotConstructible,
                $"Type '{returnType}' cannot be constructed",
                method.Locations[0]);
        }
        
        return $"return {method.Name}();";
    });
}

// ❌ AVOID: Assuming type is valid
public string GenerateFactoryCall(IMethodSymbol method)
{
    return $"return {method.Name}();"; // No validation
}
```

### No Dynamic Code Execution

**Never generate reflection or dynamic invocation**:

```csharp
// ❌ DANGER: Runtime code execution
var code = @"
    public object GetService(string typeName) {
        var type = Type.GetType(typeName); // User controls type name
        return Activator.CreateInstance(type); // Arbitrary code execution
    }
";

// ✅ GOOD: Compile-time type resolution only
var code = @"
    public IService GetService() {
        return new ServiceImplementation(); // Known type at compile time
    }
";
```

## Dependency Injection Security

### Prevent Dependency Confusion

**Validate dependency resolution is unambiguous**:

```csharp
// ✅ GOOD: Detect ambiguous dependencies
public IResult ResolveDependency(ITypeSymbol dependencyType, IEnumerable<FactoryMetadata> factories)
{
    var candidates = factories
        .Where(f => SymbolEqualityComparer.Default.Equals(f.ReturnType, dependencyType))
        .ToList();
    
    if (candidates.Count == 0)
    {
        return Result.Error(DiagnosticId.UnresolvableDependency,
            $"No factory found for type '{dependencyType}'");
    }
    
    if (candidates.Count > 1)
    {
        return Result.Error(DiagnosticId.AmbiguousDependency,
            $"Multiple factories found for type '{dependencyType}'. Specify which to use.");
    }
    
    return Result.Success(candidates[0]);
}

// ❌ AVOID: Taking first matching dependency without validation
public FactoryMetadata ResolveDependency(ITypeSymbol dependencyType, IEnumerable<FactoryMetadata> factories)
{
    return factories.FirstOrDefault(f => 
        SymbolEqualityComparer.Default.Equals(f.ReturnType, dependencyType));
}
```

### Prevent Circular Dependencies

**Critical**: Detect circular dependencies before generating code:

```csharp
// ✅ GOOD: Detect cycles
public IResult<DependencyGraph> BuildDependencyGraph(IEnumerable<FactoryMetadata> factories)
{
    var graph = new DependencyGraph();
    
    foreach (var factory in factories)
    {
        graph.AddNode(factory);
        foreach (var dependency in factory.Dependencies)
        {
            graph.AddEdge(factory, dependency);
        }
    }
    
    // Check for cycles
    if (graph.HasCycle())
    {
        var cycle = graph.FindCycle();
        return Result.Error(DiagnosticId.CircularDependency,
            $"Circular dependency detected: {string.Join(" -> ", cycle)}");
    }
    
    return Result.Success(graph);
}
```

### Validate Lifetimes

**Ensure shorter-lived dependencies don't capture longer-lived ones**:

```csharp
// ✅ GOOD: Validate lifetime constraints
public IResult ValidateLifetime(FactoryMetadata factory, DependencyMetadata dependency)
{
    // Singleton can't depend on Scoped or Transient
    if (factory.Lifetime == Lifetime.Singleton &&
        dependency.Lifetime != Lifetime.Singleton)
    {
        return Result.Error(DiagnosticId.InvalidLifetimeDependency,
            $"Singleton '{factory.Name}' cannot depend on {dependency.Lifetime} '{dependency.Name}'");
    }
    
    // Scoped can't depend on Transient (potentially unexpected behavior)
    if (factory.Lifetime == Lifetime.Scoped &&
        dependency.Lifetime == Lifetime.Transient)
    {
        return Result.Warning(DiagnosticId.ScopedDependsOnTransient,
            $"Scoped '{factory.Name}' depends on Transient '{dependency.Name}' - new instance created each time");
    }
    
    return Result.Success();
}
```

## Exception Safety

### Never Throw to Compiler

**Critical**: Catch all exceptions and convert to diagnostics:

```csharp
// ✅ GOOD: Exception handling with diagnostics
public IResult Analyze(INamedTypeSymbol symbol)
{
    try
    {
        var metadata = ExtractMetadata(symbol);
        return Result.Success(metadata);
    }
    catch (Exception ex)
    {
        return Result.Error(DiagnosticId.InternalError,
            $"Internal error analyzing '{symbol.Name}': {ex.Message}",
            symbol.Locations[0]);
    }
}

// ❌ AVOID: Unhandled exceptions
public SpecificationMetadata Analyze(INamedTypeSymbol symbol)
{
    return ExtractMetadata(symbol); // May throw
}
```

## Security Checklist

### Generator Security

- [ ] **All user input validated** - Attribute arguments, type names, identifiers
- [ ] **No code injection vulnerabilities** - Use Roslyn syntax factories, not string concatenation
- [ ] **No path traversal** - Validate file paths if accessing files
- [ ] **No execution of user code** - Never invoke user-provided delegates or reflection
- [ ] **Exception handling complete** - All exceptions caught and converted to diagnostics
- [ ] **No unsafe file operations** - Use `AdditionalTextsProvider` instead of direct file access

### Generated Code Security

- [ ] **No unsafe code blocks** - Only generate safe, managed code
- [ ] **Type resolution validated** - All types exist and are accessible
- [ ] **No dynamic execution** - No reflection, `Activator.CreateInstance`, or dynamic
- [ ] **Dependency resolution unambiguous** - No dependency confusion
- [ ] **Circular dependencies detected** - Graph validation before generation
- [ ] **Lifetime validation** - Shorter-lived dependencies don't capture longer-lived

### Input Validation

- [ ] **Identifier validation** - All user-provided names are valid C# identifiers
- [ ] **Namespace validation** - Specifications not in restricted namespaces
- [ ] **Visibility validation** - Types have appropriate accessibility
- [ ] **Attribute argument validation** - All attribute arguments validated
- [ ] **Type safety** - Generated code doesn't require unsafe casts

## Common Security Pitfalls

### Code Injection

❌ **Vulnerable**:
```csharp
var code = $"public class {user.ClassName}Injector {{ }}";
```

✅ **Safe**:
```csharp
if (!IsValidIdentifier(user.ClassName))
    throw new ArgumentException("Invalid class name");

var declaration = SyntaxFactory.ClassDeclaration(
    SyntaxFactory.Identifier($"{user.ClassName}Injector"));
```

### Path Traversal

❌ **Vulnerable**:
```csharp
File.ReadAllText(Path.Combine(baseDir, user.ConfigFile));
```

✅ **Safe**:
```csharp
var configProvider = context.AdditionalTextsProvider
    .Where(text => Path.GetFileName(text.Path) == "config.json");
```

### Type Confusion

❌ **Vulnerable**:
```csharp
var code = $"return ({user.TypeName})factory.Create();";
```

✅ **Safe**:
```csharp
if (!IsValidType(user.TypeSymbol) || !IsAccessible(user.TypeSymbol))
{
    diagnostics.ReportError(...);
    return;
}

var code = GenerateCastExpression(user.TypeSymbol);
```

## Threat Model

### Threats We Protect Against

1. **Malicious Code Injection**: Attacker provides malicious type/method names
2. **Path Traversal**: Attacker tries to access files outside project
3. **Denial of Service**: Attacker provides input causing infinite loops/excessive memory
4. **Information Disclosure**: Attacker tries to read sensitive files or data

### Threats Outside Scope

1. **Compromised Build Environment**: If build machine is compromised, generator can't protect
2. **Malicious Dependencies**: If malicious packages in dependency tree, generator can't detect
3. **Supply Chain Attacks**: Trust NuGet package integrity (use package signing)

## Incident Response

If security issue discovered:

1. **Report privately** - Email security@project.com (not public GitHub issue)
2. **Assess severity** - CVSS score, affected versions
3. **Develop fix** - Create patch in private branch
4. **Coordinate disclosure** - 90-day window for users to update
5. **Release security advisory** - After fix available

## Questions?

- For input validation patterns: See [Coding Standards](coding-standards.instructions.md)
- For error reporting: See [Architecture Guide](architecture.instructions.md)
- For testing security: See [Testing Standards](testing.instructions.md)
