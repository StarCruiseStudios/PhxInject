# Taming GitHub Copilot: Controlled Code Modification

Guidelines for controlling GitHub Copilot's behavior to ensure minimal, surgical code changes rather than overly aggressive refactoring or rewriting.

## Core Directives

### 1. Code on Request Only

**Default Mode: Analysis and Explanation**

When asked about code, Copilot should:
- Analyze and explain what exists
- Answer questions about current implementation
- Describe how things work

**Only write code when explicitly requested**:
- "implement..."
- "add..."
- "fix..."
- "create..."
- "modify..."
- "refactor..."

```
User: "How does the linking stage work?"
Copilot: [Explains linking stage architecture from code and docs]

User: "Implement validation for circular dependencies in the linking stage"
Copilot: [Writes code to add validation]
```

### 2. Surgical Modifications

**Make the minimal change necessary**

When modifying code:
- Change only what's required for the stated goal
- Preserve existing code structure
- Don't refactor unrelated code
- Don't reformat unrelated lines
- Don't "improve" code that wasn't mentioned

```csharp
// User asks: "Add null check for symbol parameter"

// ❌ AVOID: Refactoring entire method
public IResult Analyze(INamedTypeSymbol? symbol)
{
    // Added null check
    if (symbol is null)
    {
        return Result.Error(...);
    }
    
    // Also refactored these lines unnecessarily:
    var factories = symbol.GetMembers()
        .OfType<IMethodSymbol>()
        .Where(m => HasFactoryAttribute(m))
        .Select(m => new FactoryMetadata(m))
        .ToImmutableArray();
    
    var builders = GetBuilders(symbol);
    return AnalyzeCore(factories, builders);
}

// ✅ GOOD: Only add null check
public IResult Analyze(INamedTypeSymbol? symbol)
{
    if (symbol is null)
    {
        return Result.Error(DiagnosticId.NullSymbol, "Symbol cannot be null", Location.None);
    }
    
    // Rest of method unchanged
    var factories = GetFactories(symbol);
    var builders = GetBuilders(symbol);
    return AnalyzeCore(factories, builders);
}
```

### 3. Preserve Existing Patterns

**Match the style and patterns already in use**

- Use existing helper methods rather than inline
- Follow established naming conventions
- Match error handling patterns
- Use project's existing abstractions

```csharp
// File uses DiagnosticsRecorder.Capture pattern throughout

// ❌ AVOID: Introducing different pattern
public IResult ValidateFactory(IMethodSymbol method)
{
    var diagnostics = new List<DiagnosticData>();
    
    if (method.ReturnsVoid)
    {
        diagnostics.Add(new DiagnosticData(...));
    }
    
    return diagnostics.Any() 
        ? Result.Failure(diagnostics)
        : Result.Success();
}

// ✅ GOOD: Use established pattern
public IResult ValidateFactory(IMethodSymbol method)
{
    return DiagnosticsRecorder.Capture(diagnostics => {
        if (method.ReturnsVoid)
        {
            diagnostics.ReportError(
                DiagnosticId.FactoryReturnsVoid,
                "Factory cannot return void",
                method.Locations[0]);
        }
    });
}
```

### 4. Explicit About Changes

**Clearly state what will be modified**

Before modifying code:
- List the files that will be changed
- Describe the specific changes
- Explain why each change is necessary
- Ask for confirmation if ambiguous

```
User: "Fix the circular dependency detection"

Copilot:
I'll modify LinkResolver.cs to add circular dependency detection:

1. Add private method DetectCycle() to check for cycles
2. Call DetectCycle() after building dependency graph
3. Report DiagnosticId.CircularDependency if cycle found

This addresses the issue where circular dependencies weren't being caught.

Proceeding with changes...
```

## Specific Guidelines for PhxInject

### Generator Code Changes

When modifying source generator code:

**DO**:
- Preserve incremental provider chains
- Maintain existing pipeline stage boundaries
- Keep diagnostic reporting patterns consistent
- Reuse existing helper methods

**DON'T**:
- Change pipeline structure unnecessarily
- Introduce new caching mechanisms without discussion
- Modify symbol traversal patterns
- Change diagnostic IDs (breaks user code)

### Generated Code Changes

When modifying code generation logic:

**DO**:
- Preserve output formatting
- Maintain existing code structure
- Update snapshot tests when output changes
- Document why generated code changed

**DON'T**:
- Reformat entire generated output
- Change naming patterns
- Modify file organization
- Remove comments from generated code

### Test Changes

When modifying tests:

**DO**:
- Add tests for new functionality
- Update specific failing tests
- Maintain test naming conventions
- Preserve test organization

**DON'T**:
- Rewrite passing tests
- Change test structure gratuitously
- Consolidate tests without request
- Remove test cases

## Interaction Patterns

### Requesting Changes

**Effective requests**:
- "Add null validation for the symbol parameter in SpecificationAnalyzer.Analyze()"
- "Fix bug where circular dependencies aren't detected in LinkResolver"
- "Implement caching for fully qualified names in TypeNameResolver"

**Less effective requests**:
- "Make the code better" (too vague)
- "Fix the generator" (what specifically?)
- "Improve performance" (where, how?)

### Responding to Overreach

If Copilot suggests too many changes:

```
User: "This seems like more changes than necessary. 
Can we just add the null check without modifying the rest of the method?"

Copilot: "You're right. I'll add only the null check at the start of the method 
and leave everything else unchanged."
```

## Validation Rules

Before making changes, verify:

- [ ] **Change is requested** - Not just analysis
- [ ] **Scope is clear** - Know exactly what to modify
- [ ] **Minimal impact** - Change only necessary code
- [ ] **Preserves patterns** - Matches existing style
- [ ] **Tests updated** - Only tests affected by change
- [ ] **Snapshots updated** - Only if generated code changed

## Common Overreach Scenarios

### Scenario 1: "Fix this bug"

❌ **Overreach**: Rewrite entire file, refactor unrelated code, change formatting
✅ **Appropriate**: Fix the specific bug, add test, update documentation if needed

### Scenario 2: "Add this feature"

❌ **Overreach**: Redesign architecture, change APIs, refactor existing code
✅ **Appropriate**: Add feature using existing patterns, minimal new code, preserve existing behavior

### Scenario 3: "Improve performance"

❌ **Overreach**: Rewrite everything with different algorithms/data structures
✅ **Appropriate**: Profile, identify bottleneck, optimize specific hot path

### Scenario 4: "Update documentation"

❌ **Overreach**: Rewrite all docs, change formatting, reorganize structure
✅ **Appropriate**: Update specific doc section, fix inaccuracies, add missing details

## Escalation

If unsure about scope:

1. **Ask for clarification**: "Should я also modify [related code]?"
2. **Propose minimal change**: "I'll just add the null check. Sound good?"
3. **List alternatives**: "Option 1: Quick fix. Option 2: Larger refactor. Which?"

## Examples

### Example 1: Adding Validation

**Request**: "Add validation that factory return types are not void"

✅ **Good Response**:
```
I'll add void return validation to CoreValidator.ValidateFactoryReturnType():

1. Add check: if (method.ReturnsVoid) report diagnostic
2. Add diagnostic DiagnosticId.FactoryReturnsVoid to DiagnosticDescriptors.cs
3. Add test case to CoreValidatorTests.cs

This preserves existing validation structure and uses established patterns.
```

❌ **Overreach Response**:
```
I'll refactor the entire validation system to use a specification pattern, 
add validation for return types, parameter types, visibility, and naming conventions,
reorganize the test structure, and update documentation.
```

### Example 2: Fixing Bug

**Request**: "Fix the bug where linked specifications aren't resolved correctly"

✅ **Good Response**:
```
I found the issue in LinkResolver.ResolveSpecification():
The type index lookup is case-sensitive but should be case-insensitive.

I'll change:
- Line 42: _typeIndex.TryGetValue(name, out var spec)
+ Line 42: _typeIndex.TryGetValue(name, StringComparer.OrdinalIgnoreCase, out var spec)

And add a test in LinkResolverTests.cs to verify case-insensitive lookup works.
```

❌ **Overreach Response**:
```
I'll redesign the type resolution system to use semantic equality comparison,
refactor the type index to use a trie data structure for better performance,
update all call sites, and rewrite the tests for better coverage.
```

## Questions?

- For coding patterns to preserve: See [Coding Standards](coding-standards.instructions.md)
- For generator patterns to maintain: See [Code Generation Standards](code-generation.instructions.md)
- For architectural boundaries: See [Architecture Guide](architecture.instructions.md)
