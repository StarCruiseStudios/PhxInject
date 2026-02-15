# Code Review Standards for PhxInject

Code review guidelines adapted for PhxInject's source generator and dependency injection framework. Use this checklist for self-review and peer review.

## Priority System

Reviews use a three-tier priority system:

- 🔴 **Critical**: Must be addressed before merge
- 🟡 **Important**: Should be addressed, document if deferred
- 🟢 **Suggestion**: Nice-to-have improvement

## General Review Checklist

### 🔴 Critical Issues

These must be fixed before merging:

- [ ] **No compiler errors or warnings** - All code must compile cleanly
- [ ] **All tests pass** - Unit, integration, and snapshot tests
- [ ] **No security vulnerabilities** - See [Security Standards](security.instructions.md)
- [ ] **No circular dependencies** - Dependency graphs must be acyclic
- [ ] **Diagnostics never throw** - Generator must catch exceptions and report diagnostics
- [ ] **Performance regressions** - No significant build time increases
- [ ] **Breaking changes documented** - API changes noted in commit message

### 🟡 Important Issues

Should be addressed, defer only with justification:

- [ ] **Code coverage ≥85%** - See [Testing Standards](testing.instructions.md)
- [ ] **XML documentation complete** - See [Documentation Standards](documentation.instructions.md)
- [ ] **Follows coding standards** - See [Coding Standards](coding-standards.instructions.md)
- [ ] **Error messages are clear** - Diagnostics provide actionable guidance
- [ ] **Generated code is readable** - Human-readable and debuggable
- [ ] **Incremental generator patterns used** - Proper caching for performance
- [ ] **No obvious performance issues** - No N+1 queries, excessive allocations

### 🟢 Suggestions

Nice-to-have improvements:

- [ ] **Code simplification opportunities** - LINQ instead of loops, etc.
- [ ] **Naming improvements** - More descriptive variable/method names
- [ ] **Additional test cases** - Edge cases, boundary conditions
- [ ] **Refactoring opportunities** - Extract methods, reduce complexity
- [ ] **Documentation enhancements** - More examples, clarifications

## Source Generator Specific Checklist

### 🔴 Generator Critical

- [ ] **Never throws to compiler** - All exceptions caught and converted to diagnostics
- [ ] **Incremental providers used** - Using `Select`, `Combine`, `Where` from incremental APIs
- [ ] **Deterministic output** - Same input always produces same generated code
- [ ] **Syntax predicates filter early** - Avoid expensive semantic analysis when possible
- [ ] **All diagnostics have location** - Users can navigate to error source
- [ ] **Generated code compiles** - Verify output compiles without errors

### 🟡 Generator Important

- [ ] **Caching expensive operations** - Fully qualified names, symbol lookups
- [ ] **Not enumerating all symbols** - Avoid `compilation.GetSymbolsWithName()` when possible
- [ ] **Pipeline model types use EquatableList** - Not `ImmutableArray` (lacks structural equality)
- [ ] **Diagnostic messages are actionable** - Tell user how to fix the problem
- [ ] **Generated code follows standards** - See [Code Generation Standards](code-generation.instructions.md)

### 🟢 Generator Suggestions

- [ ] **Could use more specific predicates** - Filter earlier in pipeline
- [ ] **Opportunity for shared caching** - Cache results across specifications
- [ ] **Generated code formatting** - Could improve readability

## Domain Logic Checklist

### 🔴 Domain Critical

- [ ] **Type safety maintained** - No unsafe casts in generated code
- [ ] **Factory return types validated** - Non-void, constructible types
- [ ] **Builder signatures validated** - Correct parameter types
- [ ] **Specification visibility correct** - Public or internal as required
- [ ] **Link attributes resolved** - All linked specifications exist

### 🟡 Domain Important

- [ ] **Dependency resolution complete** - All dependencies have resolution paths
- [ ] **Specification instantiation mode correct** - Static, Instantiated, or Dependency
- [ ] **Factory method accessibility** - Public or internal as required
- [ ] **Parameter types resolvable** - All factory parameters can be resolved

## Testing Review Checklist

### 🔴 Testing Critical

- [ ] **Tests exist for new functionality** - Unit tests at minimum
- [ ] **Diagnostic validation tests** - Error paths tested
- [ ] **Tests are isolated** - No dependencies between tests
- [ ] **All tests pass** - No flaky tests

### 🟡 Testing Important

- [ ] **Edge cases tested** - Null, empty, boundary values
- [ ] **Integration tests for pipeline changes** - End-to-end validation
- [ ] **Snapshot tests for generated code** - If output format changed
- [ ] **Code coverage meets 85% threshold** - Use `dotnet test --collect:"XPlat Code Coverage"`

### 🟢 Testing Suggestions

- [ ] **Additional test cases** - More scenarios covered
- [ ] **Theory tests for similar cases** - Use `[Theory]` instead of multiple `[Fact]`
- [ ] **Performance tests** - For critical path changes

## Documentation Review Checklist

### 🔴 Documentation Critical

- [ ] **Public API documented** - All public types and members
- [ ] **Breaking changes documented** - In commit message and XML docs
- [ ] **Diagnostic IDs documented** - In diagnostic descriptor

### 🟡 Documentation Important

- [ ] **Internal pipeline types documented** - Critical functionality explained
- [ ] **Architectural role explained** - Pipeline stage mentioned
- [ ] **Non-obvious behavior documented** - Edge cases, invariants
- [ ] **Examples included for complex features** - Code samples in remarks

### 🟢 Documentation Suggestions

- [ ] **More examples** - Additional usage scenarios
- [ ] **Better parameter descriptions** - More detail on constraints
- [ ] **See also references** - Links to related types

## Example Review Comments

### Good Review Comments

**Critical Issue (🔴)**:
```
🔴 This method doesn't catch exceptions and will throw to the compiler.
All pipeline stage methods must use try-catch and convert to diagnostics.

See: architecture.instructions.md - "Diagnostic-Driven Design"
```

**Important Issue (🟡)**:
```
🟡 This factory method isn't covered by tests.
All factory validation logic should have unit tests.

Could you add a test in `FactoryValidatorTests.cs`?
See: testing.instructions.md - "Unit Testing"
```

**Suggestion (🟢)**:
```
🟢 Suggestion: This LINQ query could be simplified.

Current:
var results = items.Where(x => x.IsValid).Select(x => x.Name).ToList();

Suggested:
var results = items.Where(x => x.IsValid)
    .Select(x => x.Name)
    .ToImmutableArray(); // Prefer immutable collections
```

### Example Comment Formats

**For Code Issues**:
```
[Priority] [Issue description]

[Explanation of why this matters]

[Suggested fix or reference to standards]
```

**For Questions**:
```
❓ Could you explain why this approach was chosen over [alternative]?
```

**For Positive Feedback**:
```
✅ Nice use of incremental providers here! This will cache results efficiently.
```

## Self-Review Process

Before requesting review:

1. **Run all tests locally**: `dotnet test`
2. **Check code coverage**: `dotnet test --collect:"XPlat Code Coverage"`
3. **Review your own diff**: Look for obvious issues
4. **Run formatter**: Ensure consistent formatting
5. **Check for warnings**: `dotnet build -warnaserror`
6. **Validate diagnostics**: Test error reporting paths
7. **Review generated code**: Check output is readable

## Reviewer Guidelines

When reviewing code:

1. **Start with critical issues** - Security, correctness, compilation errors
2. **Check tests early** - Validate coverage and quality
3. **Validate generator patterns** - Incremental APIs, exception handling
4. **Review diagnostics** - Error messages clear and actionable
5. **Check documentation** - Public APIs documented
6. **Suggest improvements** - After critical/important issues addressed

## Responding to Review Feedback

As the author:

- **Address critical items immediately** - No exceptions
- **Discuss important items** - Explain if deferring
- **Acknowledge suggestions** - Implement if time permits
- **Ask for clarification** - If feedback is unclear
- **Update PR description** - Document deferred items

## Common Pitfalls

### Generator-Specific

❌ **Don't**: Throw exceptions to compiler
```csharp
public void Analyze(Compilation compilation) {
    throw new Exception("Analysis failed"); // NO!
}
```

✅ **Do**: Report diagnostics
```csharp
public IResult Analyze(Compilation compilation) {
    return DiagnosticsRecorder.Capture(diagnostics => {
        // Analysis logic
        diagnostics.ReportError(DiagnosticId.AnalysisFailed, "...", location);
    });
}
```

❌ **Don't**: Use ImmutableArray in pipeline models
```csharp
public record PipelineData(ImmutableArray<SpecFactoryMethodMetadata> Factories);
```

✅ **Do**: Use EquatableList for structural equality
```csharp
public record PipelineData(EquatableList<SpecFactoryMethodMetadata> Factories);
```

### Testing

❌ **Don't**: Test implementation details
```csharp
[Fact]
public void Analyzer_CallsPrivateMethod() { ... } // Testing implementation
```

✅ **Do**: Test observable behavior
```csharp
[Fact]
public void Analyzer_WhenInvalidInput_ReportsDiagnostic() { ... } // Testing behavior
```

### Documentation

❌ **Don't**: State the obvious
```csharp
/// <summary> Gets the name. </summary>
public string Name { get; }
```

✅ **Do**: Explain intent
```csharp
/// <summary>
/// The fully qualified type name used for code generation and type resolution.
/// </summary>
public string Name { get; }
```

## Validation Checklist

Before marking PR as ready for review:

- [ ] All 🔴 Critical items in self-review checklist addressed
- [ ] All tests pass locally
- [ ] Code coverage ≥85%
- [ ] No compiler warnings
- [ ] Generated code compiles and is readable
- [ ] Diagnostics tested and messages clear
- [ ] Public APIs documented
- [ ] Breaking changes noted in PR description
- [ ] Self-reviewed own diff for obvious issues

## Questions?

- For coding standards: See [Coding Standards](coding-standards.instructions.md)
- For testing patterns: See [Testing Standards](testing.instructions.md)
- For generator patterns: See [Code Generation Standards](code-generation.instructions.md)
- For security concerns: See [Security Standards](security.instructions.md)
