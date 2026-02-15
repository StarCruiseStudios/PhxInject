# PhxInject Testing Quick Reference

Essential guide for writing tests in PhxInject projects using **Phx.Test** and **PhxValidation**.

## Two Test Suites

### Phx.Inject.Tests - Runtime Behavior Testing
Test the **runtime behavior** of generated injectors when instantiated and executed.

```csharp
public class MyFeatureTests : LoggingTestClass
{
    [Test]
    public void MethodName_Scenario_ExpectedOutcome()
    {
        // Given - Setup
        var injector = Given("A test injector", 
            () => new MyTestInjector());
        
        // When - Execute
        var result = When("Getting instance",
            () => injector.GetMyService());
        
        // Then - Verify
        Then("Instance is correct type",
            () => Verify.That(result.IsType<MyService>()));
    }
}
```

**Key Files:**
- Test classes: `{Feature}Tests.cs` in `Phx.Inject.Tests/`
- Test specifications: `{Purpose}Specification.cs` in `Phx.Inject.Tests/Data/`
- Shared fixtures: `CommonTestValueSpecification.cs`
- Inherit from: `LoggingTestClass`

### Phx.Inject.Generator.Tests - Compile-Time Generation Testing
Test the **source generator's** ability to analyze code and generate valid injectors.

```csharp
public class MyGeneratorTests : LoggingTestClass
{
    [Test]
    public void GeneratorTest_Scenario_ProducesCorrectCode()
    {
        var source = @"
            using Phx.Inject;
            [Specification]
            public class TestSpec { [Factory] public int Get() => 1; }
        ";
        
        // Compile source with generator
        var compilation = Given("Source code",
            () => TestCompiler.CompileText(
                source,
                ReferenceAssemblies.Net.Net90,
                new IncrementalSourceGenerator()));
        
        // Check results
        var diagnostics = When("Running generator",
            () => compilation.GetDiagnostics());
        
        Then("No errors",
            () => Verify.That(diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Count().IsEqualTo(0)));
    }
}
```

**Key Files:**
- Test classes: `{Feature}Tests.cs` in `Phx.Inject.Generator.Tests/`
- Test data: Source files in `Phx.Inject.Generator.Tests/Phx/Inject/Tests/Data/`
- Helper: `TestCompiler` in `Phx.Inject.Generator.Tests/Helpers/`
- Inherit from: `LoggingTestClass`

---

## Phx.Test Framework - Given/When/Then Pattern

Use these methods for readable, well-logged tests:

```csharp
// Setup - returns T for use in subsequent code
var dependency = Given("A dependency",
    () => CreateDependency());

// Execute - returns T for verification
var result = When("Calling the method",
    () => dependency.DoSomething());

// Verify - logs and executes assertion
Then("Expected outcome occurred",
    () => Verify.That(result.IsNotNull()));

// Parameterized assertion
Then("Value matches expected",
    expectedValue,
    expected => Verify.That(result.Value.IsEqualTo(expected)));
```

**Standard Pattern:**
1. `Given()` - Setup phase (may be multiple)
2. `When()` - Execute phase (typically one)
3. `Then()` - Assert phase (may be multiple)

All calls are logged automatically with "GIVEN:", "WHEN:", "THEN:" prefixes.

---

## PhxValidation Framework - Fluent Assertions

Use these assertion methods (all return bool):

### Type Checking
```csharp
Verify.That(value.IsType<MyType>());
Verify.That(value.IsType<IMyInterface>());
Verify.That(value.IsNotType<OtherType>());
```

### Equality
```csharp
Verify.That(value.IsEqualTo(expected));
Verify.That(value.IsNotEqualTo(unexpected));
```

### Null Checking
```csharp
Verify.That(value.IsNotNull());
Verify.That(value.IsNull());
```

### Boolean
```csharp
Verify.That(condition.IsTrue());
Verify.That(condition.IsFalse());
```

### Collections
```csharp
Verify.That(items.IsEmpty());
Verify.That(items.Count().IsEqualTo(3));
Verify.That(items.Contains(item));
Verify.That(items.Any(predicate));
```

### Strings
```csharp
Verify.That(text.IsEmpty());
Verify.That(text.Contains("substring"));
Verify.That(text.StartsWith("prefix"));
Verify.That(text.Matches(regex));
```

**All methods chain**: `.IsEqualTo(5).IsTrue()`

---

## Phx.Inject.Tests Patterns

### 1. Simple Injector Test

```csharp
[Test]
public void GetValue_WhenCalled_ReturnsInstance()
{
    IMyTestInjector injector = Given("A test injector",
        () => new MyTestInjector());
    
    var value = When("Getting value",
        () => injector.GetValue());
    
    Then("Value is not null",
        () => Verify.That(value.IsNotNull()));
}
```

### 2. Define Test Injector Interface

Use `[Injector]` attribute on test interface:

```csharp
[Injector(typeof(MyTestSpecification))]
public interface IMyTestInjector
{
    MyService GetService();
    MyRepository GetRepository();
}
```

The source generator will create `MyTestInjector` implementation.

### 3. Create Test Specification

Minimal specification for testing:

```csharp
[Specification]
public static class MyTestSpecification
{
    [Factory]
    public static MyService GetService() 
        => new MyService();
    
    [Factory]
    public static MyRepository GetRepository() 
        => new MyRepository();
}
```

### 4. Test with Qualifiers/Labels

```csharp
[Test]
public void GetLabeledValue_WithLabel_ReturnCorrectInstance()
{
    var primary = Given("Injector for labeled values",
        () => new LabelTestInjector())
        .When("Getting primary", i => i.GetPrimary());
    
    Then("Primary label value is correct",
        ExpectedPrimary,
        expected => Verify.That(primary.Value
            .IsEqualTo(expected)));
}
```

### 5. Test Nested Injectors

```csharp
[Test]
public void ChildInjector_DependsOnParent_SharesDependencies()
{
    var parent = Given("Parent injector",
        () => new ParentTestInjector());
    
    var child = When("Getting child",
        () => parent.GetChildInjector());
    
    var parentValue = When("Getting parent value",
        () => parent.GetSharedValue());
    var childValue = When("Getting child value",
        () => child.GetSharedValue());
    
    Then("Shared dependency is same instance",
        () => Verify.That(parentValue.Equals(childValue)));
}
```

---

## Phx.Inject.Generator.Tests Patterns

### 1. Simple Code Generation Test

```csharp
[Test]
public void Generator_SimpleSpec_GeneratesValidCode()
{
    var source = @"
        using Phx.Inject;
        
        [Specification]
        public class TestSpec
        {
            [Factory]
            public int GetInt() => 42;
        }
        
        [Injector(typeof(TestSpec))]
        public interface ITestInjector
        {
            int GetInt();
        }
    ";
    
    var compilation = Given("Source with specification",
        () => TestCompiler.CompileText(
            source,
            ReferenceAssemblies.Net.Net90,
            new IncrementalSourceGenerator()));
    
    var errors = When("Getting diagnostics",
        () => compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList());
    
    Then("No errors occur",
        () => Verify.That(errors.Count().IsEqualTo(0)));
}
```

### 2. Multi-Framework Testing

```csharp
public class GeneratorTests : LoggingTestClass
{
    public static IEnumerable<TestCaseData> AllFrameworks =>
        new[]
        {
            ReferenceAssemblies.NetStandard.NetStandard20,
            ReferenceAssemblies.Net.Net90
        }.Select(rf => new TestCaseData(rf)
            .SetName($"Framework_{rf.TargetFramework}"));
    
    [TestCaseSource(nameof(AllFrameworks))]
    public void Generator_AllFrameworks_Succeeds(
        ReferenceAssemblies referenceAssemblies)
    {
        var compilation = TestCompiler.CompileText(
            sourceCode, referenceAssemblies, generator);
        
        Then("Compiles for {0}",
            referenceAssemblies.TargetFramework,
            framework => {
                var errors = compilation.GetDiagnostics()
                    .Where(d => d.Severity == DiagnosticSeverity.Error);
                Verify.That(errors.Count().IsEqualTo(0));
            });
    }
}
```

### 3. Test Generated Types Exist

```csharp
[Test]
public void Generator_CreatesExpectedTypes()
{
    var compilation = TestCompiler.CompileText(source, frameworks, generator);
    
    var ns = When("Finding injector namespace",
        () => compilation.GlobalNamespace
            .GetMembers("Phx")
            .First() as INamespaceSymbol);
    
    Then("InjectorType exists",
        () => {
            var injectorType = ns!
                .GetMembers("MyInjector")
                .FirstOrDefault();
            Verify.That(injectorType.IsNotNull());
        });
}
```

### 4. Test Diagnostic Reporting

```csharp
[Test]
public void Generator_WithCircularDependency_ReportsDiagnostic()
{
    var source = @"
        [Specification]
        public class BadSpec
        {
            [Factory]
            public A GetA(B b) => new A(b);
            
            [Factory]
            public B GetB(A a) => new B(a);
        }
    ";
    
    var diagnostics = TestCompiler.CompileText(
        source, frameworks, generator)
        .GetDiagnostics();
    
    var error = When("Finding error",
        () => diagnostics.FirstOrDefault(
            d => d.GetMessage().Contains("circular")));
    
    Then("Circular dependency detected",
        () => Verify.That(error.IsNotNull()));
    
    Then("Severity is Error",
        () => Verify.That(error!.Severity
            .IsEqualTo(DiagnosticSeverity(Error))));
}
```

---

## Test File Template

### Phx.Inject.Tests Template

```csharp
// <copyright file="MyFeatureTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
// </copyright>

using NUnit.Framework;
using Phx.Inject.Tests.Data;
using Phx.Inject.Tests.Data.Model;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests;

/// <summary>
/// Tests for [feature description].
/// </summary>
public class MyFeatureTests : LoggingTestClass
{
    [Injector(typeof(MyTestSpecification))]
    public interface IMyTestInjector
    {
        MyType GetInstance();
    }

    [Test]
    public void MethodName_Scenario_ExpectedOutcome()
    {
        var injector = Given("A test injector",
            () => new MyTestInjector());

        var result = When("Calling method",
            () => injector.GetInstance());

        Then("Result is correct",
            () => Verify.That(result.IsNotNull()));
    }
}

#region Test Specification

[Specification]
internal static class MyTestSpecification
{
    [Factory]
    internal static MyType GetMyType() => new MyType();
}

#endregion
```

### Phx.Inject.Generator.Tests Template

```csharp
// <copyright file="MyGeneratorTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
// </copyright>

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Phx.Inject.Generator.Incremental;
using Phx.Inject.Tests.Helpers;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests;

/// <summary>
/// Tests for [generator feature description].
/// </summary>
public class MyGeneratorTests : LoggingTestClass
{
    [Test]
    public void GeneratorTest_Scenario_ProducesCorrectCode()
    {
        var source = @"
            using Phx.Inject;
            
            [Specification]
            public class TestSpec { }
        ";

        var compilation = Given("Source code",
            () => TestCompiler.CompileText(
                source,
                ReferenceAssemblies.Net.Net90,
                new IncrementalSourceGenerator()));

        var diagnostics = When("Compiling with generator",
            () => compilation.GetDiagnostics());

        Then("No errors",
            () => Verify.That(diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Count().IsEqualTo(0)));
    }
}
```

---

## Checklist Before Submitting

- [ ] Tests use `Given/When/Then` pattern with `LoggingTestClass`
- [ ] Assertions use `PhxValidation` fluent methods (`.IsEqualTo()`, `.IsType<>()`, etc.)
- [ ] Test names follow `MethodName_Scenario_ExpectedOutcome` pattern
- [ ] Tests cover happy paths AND error conditions
- [ ] Edge cases tested (null, empty, boundary values)
- [ ] Tests are isolated (no execution order dependencies)
- [ ] Tests execute quickly (<5 seconds for unit suite)
- [ ] Code coverage meets 85% minimum
- [ ] All tests pass locally
- [ ] Generator tests test multiple frameworks (TestCaseSource)
- [ ] Error diagnostics have helpful messages

---

## See Also

- [Detailed Test Plan](../TEST_PLAN.md)
- [Integration Guide](../TEST_PLAN_INTEGRATION.md)
- [Architecture Guide](.github/instructions/architecture.instructions.md)
- [Code Generation Standards](.github/instructions/code-generation.instructions.md)
- [Phx.Test GitHub](https://github.com/StarCruiseStudios/PhxTest)
- [Phx.Validation GitHub](https://github.com/StarCruiseStudios/PhxValidation)
