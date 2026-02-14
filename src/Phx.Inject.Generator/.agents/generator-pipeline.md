# Generator Pipeline Architecture and Patterns

Detailed guide to the five-stage generator pipeline. Reference this when implementing new analysis rules, domain models, linking strategies, or code generation features.

## Pipeline Overview

The generator processes user code through five sequential transformation stages:

```
User Code (Compilation)
  ↓ [Stage 1: Metadata]
  ├─ Parse user code for [Specification] and [Injector] types
  ├─ Extract syntactic metadata (method signatures, parameters, attributes)
  └─ Metadata model mirrors code structure
  ↓ [Stage 2: Core]
  ├─ Transform metadata into domain models
  ├─ Create Specification, Injector, Factory, Builder, Dependency domain objects
  ├─ Capture semantic meaning without linking or validation logic
  └─ Core models are self-contained and reusable
  ↓ [Stage 3: Linking]
  ├─ Build complete dependency graph
  ├─ Match injector methods to factories/builders
  ├─ Resolve parameter dependencies recursively
  ├─ Detect cycles, conflicts, missing dependencies
  └─ Produce fully-linked dependency graph
  ↓ [Stage 4: Code Generation]
  ├─ Process linked dependency graph
  ├─ Produce template model describing code structure
  ├─ Template mirrors what generated code will look like
  └─ Templates are independent of rendering
  ↓ [Stage 5: Rendering]
  ├─ Transform template model to C# code
  ├─ Write files, apply formatting
  └─ Output `.g.cs` files
```

## Stage 1: Metadata

**Responsibility**: Extract a model of injection-related types from the user's source code. This model mirrors the syntactic structure to facilitate processing.

**Input**: User code AST (via Roslyn)

**Output**: Metadata model describing specifications, injectors, factories, and builders

### Metadata Model Characteristics

- Syntax-derived: mirrors the actual code structure
- Type-safe: references actual ISymbol/ITypeSymbol from Roslyn where appropriate
- Declarative: records what is in the code, not what it means
- Flat: minimal hierarchy, just the metadata extracted from code

Example metadata:

```csharp
public record SpecificationMetadata(
    string Name,
    INamedTypeSymbol Symbol,
    ImmutableArray<MethodMetadata> Methods,
    ImmutableArray<AttributeMetadata> Attributes
);

public record MethodMetadata(
    string Name,
    IMethodSymbol Symbol,
    ITypeSymbol ReturnType,
    ImmutableArray<ParameterMetadata> Parameters,
    ImmutableArray<AttributeMetadata> Attributes
);

public record ParameterMetadata(
    string Name,
    ITypeSymbol Type,
    int Position
);
```

### Implementation Pattern

Use syntax-level predicates for performance, then extract metadata:

```csharp
var metadataProvider = context.SyntaxProvider.CreateSyntaxProvider(
    predicate: (syntax, _) => {
        // Fast: check syntax node type only
        return syntax is ClassDeclarationSyntax { AttributeLists: not [] };
    },
    transform: (ctx, _) => {
        // Expensive: only on filtered candidates
        return ExtractSpecificationMetadata(ctx);
    }
);
```

### Key Points

- **No validation yet**: Metadata extraction is purely syntactic
- **No linking**: Don't try to resolve dependencies in this stage
- **Performance critical**: Use fast predicates to filter candidates
- **Diagnostic errors**: Report only syntactic issues (missing attributes, invalid symbols)

## Stage 2: Core

**Responsibility**: Transform metadata into domain models that represent the business concepts of dependency injection.

**Input**: Metadata model from Stage 1

**Output**: Core domain models (Specification, Injector, Factory, Builder, etc.)

### Core Model Characteristics

- **Domain-focused**: Models represent dependency injection concepts, not syntax
- **Self-contained**: No references to other core models yet (that comes in Stage 3)
- **Semantic**: Captures the meaning of metadata without implementation details
- **Non-prescriptive**: Doesn't enforce linking rules or business logic

Example core models:

```csharp
public record Specification(
    string Name,
    ImmutableArray<Factory> Factories,
    ImmutableArray<Builder> Builders
);

public record Factory(
    string Name,
    ITypeSymbol ReturnType,
    ImmutableArray<Parameter> Parameters
);

public record Builder(
    string Name,
    ITypeSymbol TargetType,
    ImmutableArray<Parameter> Parameters
);

public record Injector(
    string Name,
    ImmutableArray<ITypeSymbol> SpecificationTypes,  // Just the types, no links yet
    ImmutableArray<InjectorMethod> Methods
);

public record InjectorMethod(
    string Name,
    ITypeSymbol ReturnType
);
```

### Transformation Example

```csharp
private Specification TransformMetadata(SpecificationMetadata metadata)
{
    var factories = metadata.Methods
        .Where(m => m.Attributes.Any(a => a.Name == "Factory"))
        .Select(m => new Factory(
            Name: m.Name,
            ReturnType: m.ReturnType,
            Parameters: TransformParameters(m.Parameters)))
        .ToImmutableArray();
    
    return new Specification(
        Name: metadata.Name,
        Factories: factories,
        Builders: [ /*...*/ ]);
}
```

### Key Points

- **Domain knowledge**: Only encode domain concepts here
- **No cross-references**: Don't link Injector to Specification yet
- **Immutable**: All models are immutable records
- **Error-free**: Core transformation assumes metadata is valid

## Stage 3: Linking

**Responsibility**: Build the complete dependency graph by linking core models together.

**Input**: Core models from Stage 2

**Output**: Fully-linked dependency graph with resolved dependencies

### Linking Operations

1. **Specification Linking**: Organize factories and builders by type for lookup
2. **Injector Specification Linking**: Link injectors to their specification models
3. **Dependency Resolution**: For each injector method, resolve its dependency chain
4. **Validation**: Detect cycles, missing dependencies, unresolvable types

### Linked Model

```csharp
public record LinkedInjector(
    Injector Injector,
    ImmutableArray<LinkedSpecification> LinkedSpecifications,
    ImmutableArray<LinkedInjectorMethod> LinkedMethods,
    ImmutableArray<Diagnostic> LinkingErrors
);

public record LinkedInjectorMethod(
    string MethodName,
    ITypeSymbol ReturnType,
    Factory LinkedFactory,  // The factory that resolves this method
    DependencyGraph Dependencies
);

public record DependencyGraph(
    ImmutableArray<DependencyNode> Nodes
);

public record DependencyNode(
    Factory Factory,
    ImmutableArray<DependencyNode> Dependencies
);
```

### Linking Logic

```csharp
private LinkedInjector LinkInjector(
    Injector injector,
    ImmutableArray<Specification> specifications)
{
    var errors = new List<Diagnostic>();
    
    // Link to specification models
    var linkedSpecs = LinkSpecifications(injector.SpecificationTypes, specifications, errors);
    
    // Link injector methods to factories
    var linkedMethods = injector.Methods
        .Select(method => LinkInjectorMethod(method, linkedSpecs, errors))
        .ToImmutableArray();
    
    return new LinkedInjector(
        Injector: injector,
        LinkedSpecifications: linkedSpecs,
        LinkedMethods: linkedMethods,
        LinkingErrors: errors.ToImmutableArray());
}

private LinkedInjectorMethod LinkInjectorMethod(
    InjectorMethod method,
    ImmutableArray<LinkedSpecification> specifications,
    List<Diagnostic> errors)
{
    // Find factory matching method's return type
    var factory = FindFactoryByType(method.ReturnType, specifications);
    
    if (factory == null) {
        errors.Add(CreateUnresolvableTypeDiagnostic(method));
        return null!; // Skip this method
    }
    
    // Recursively resolve factory's dependencies
    var dependencyGraph = ResolveDependencies(factory, specifications, errors);
    
    return new LinkedInjectorMethod(
        MethodName: method.Name,
        ReturnType: method.ReturnType,
        LinkedFactory: factory,
        Dependencies: dependencyGraph);
}
```

### Key Points

- **Collect all errors**: Report all linking issues before stopping
- **Cycle detection**: Recursive dependency resolution can create cycles; detect them
- **Ambiguity resolution**: Multiple factories matching same type must be disambiguated
- **Immutable results**: Linked models are immutable for caching/reuse

## Stage 4: Code Generation

**Responsibility**: Process the linked dependency graph and produce a template model describing what code will be generated.

**Input**: Linked dependency graph from Stage 3

**Output**: Template model describing generated code structure

### Template Model

The template model mirrors the structure of the code that will be generated, without yet rendering it as text:

```csharp
public record GeneratedInjectorTemplate(
    string ClassName,
    ITypeSymbol ImplementedInterface,
    ImmutableArray<GeneratedMethodTemplate> Methods,
    ImmutableArray<Diagnostic> GenerationErrors
);

public record GeneratedMethodTemplate(
    string MethodName,
    ITypeSymbol ReturnType,
    ImmutableArray<MethodCallTemplate> MethodCalls,
    string? ReturnedVariable
);

public record MethodCallTemplate(
    string TargetTypeName,
    string MethodName,
    ImmutableArray<string> ArgumentVariables
);

public record VariableTemplate(
    string VariableName,
    ITypeSymbol Type,
    MethodCallTemplate Initializer
);
```

### Template Generation Process

```csharp
private GeneratedInjectorTemplate GenerateTemplate(LinkedInjector linkedInjector)
{
    var methodTemplates = linkedInjector.LinkedMethods
        .Select(method => GenerateMethodTemplate(method))
        .ToImmutableArray();
    
    return new GeneratedInjectorTemplate(
        ClassName: $"Generated{linkedInjector.Injector.Name}",
        ImplementedInterface: linkedInjector.Injector.InterfaceType,
        Methods: methodTemplates,
        GenerationErrors: [ /* validation errors */ ]);
}

private GeneratedMethodTemplate GenerateMethodTemplate(LinkedInjectorMethod method)
{
    var variables = new List<VariableTemplate>();
    var methodCalls = new List<MethodCallTemplate>();
    
    // Generate variable assignments for all dependencies
    GenerateDependencyVariables(method.Dependencies, variables, methodCalls);
    
    // Final call to factory method
    var factoryCall = new MethodCallTemplate(
        TargetTypeName: method.LinkedFactory.SourceTypeName,
        MethodName: method.LinkedFactory.Name,
        ArgumentVariables: ExtractArgumentVariables(variables));
    
    return new GeneratedMethodTemplate(
        MethodName: method.MethodName,
        ReturnType: method.ReturnType,
        MethodCalls: [ /*... all calls ...*/ ],
        ReturnedVariable: GenerateReturnVariable(factoryCall));
}
```

### Key Points

- **Declarative structure**: Template describes what will be rendered, not how
- **No string building**: Templates are objects, not text
- **Validation**: Check for issues (null returns, invalid types) and record errors
- **Independent of rendering**: Any language/format could render these templates

## Stage 5: Rendering

**Responsibility**: Transform template models into actual C# code.

**Input**: Template model from Stage 4

**Output**: Generated C# code written to `.g.cs` files

### Rendering Process

```csharp
public string RenderTemplate(GeneratedInjectorTemplate template)
{
    var code = new StringBuilder();
    
    code.AppendLine("// <auto-generated />");
    code.AppendLine("#nullable enable");
    code.AppendLine();
    code.AppendLine($"public class {template.ClassName} : {template.ImplementedInterface.Name}");
    code.AppendLine("{");
    
    foreach (var method in template.Methods) {
        RenderMethod(code, method);
    }
    
    code.AppendLine("}");
    code.AppendLine("#nullable restore");
    
    return code.ToString();
}

private void RenderMethod(StringBuilder code, GeneratedMethodTemplate method)
{
    code.AppendLine($"    public {method.ReturnType.Name} {method.MethodName}()");
    code.AppendLine("    {");
    
    // Render variable declarations
    int varIndex = 0;
    foreach (var call in method.MethodCalls) {
        code.AppendLine($"        var _{method.ReturnType.Name}{varIndex} = {call.TargetTypeName}.{call.MethodName}({string.Join(", ", call.ArgumentVariables)});");
        varIndex++;
    }
    
    // Render return statement
    code.AppendLine($"        return {method.ReturnedVariable};");
    code.AppendLine("    }");
}
```

### Rendering Characteristics

- **Simple transformation**: Template → text
- **Formatting**: Apply consistent indentation and spacing
- **Safe**: No logic here; all decisions made in Stage 4
- **Testable**: Output is verifiable, stable, readable

### Generated Code Standards

All rendered code must follow [Code Generation Practices](../../.agents/code-generation.md):

- Include `// <auto-generated />` header
- Include nullability directives `#nullable enable/restore`
- Apply consistent indentation (4 spaces)
- Use readable variable names (e.g., `_int0`, `_myService1`)
- No runtime validation (all validation done at generation time)

## Validation Checklist

When implementing pipeline stages:

- **Stage 1**: Metadata uses syntax-level predicates; performance is optimized
- **Stage 2**: Core models are domain-focused, immutable, self-contained
- **Stage 3**: Linking collects all errors before stopping; cycles detected
- **Stage 4**: Templates describe code structure; no string building
- **Stage 5**: Rendering is simple transformation; all logic in Stage 4
- **Cross-stage**: Each stage is independent; outputs consumable by next stage
- **Testing**: Unit tests per stage; integration tests across stages
- **Performance**: Large specifications generate quickly

## References

- **[Architecture Guide](../../.agents/architecture.md)**: System overview and design goals
- **[Code Generation Practices](../../.agents/code-generation.md)**: Generated code standards
- **[Coding Standards](../../.agents/coding-standards.md)**: C# guidelines
