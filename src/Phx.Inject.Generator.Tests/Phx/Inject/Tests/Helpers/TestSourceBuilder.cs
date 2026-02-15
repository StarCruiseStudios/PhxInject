// -----------------------------------------------------------------------------
// <copyright file="TestSourceBuilder.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Helpers;

/// <summary>
/// Builder for creating test source code with common patterns to reduce duplication.
/// </summary>
public class TestSourceBuilder {
    private readonly List<string> _usings = new() { "using Phx.Inject;" };
    private readonly List<string> _types = new();
    private string _namespace = "TestNamespace";

    public static TestSourceBuilder Create() => new();

    public TestSourceBuilder WithNamespace(string namespaceName) {
        _namespace = namespaceName;
        return this;
    }

    public TestSourceBuilder AddUsing(string usingStatement) {
        if (!usingStatement.StartsWith("using ")) {
            usingStatement = $"using {usingStatement};";
        }
        if (!_usings.Contains(usingStatement)) {
            _usings.Add(usingStatement);
        }
        return this;
    }

    public TestSourceBuilder WithSpecification(
        string className = "TestSpec",
        string factoryReturnType = "int",
        string factoryMethodName = "GetInt",
        string factoryReturnValue = "42") {
        
        _types.Add($@"
            [Specification]
            public static class {className} {{
                [Factory]
                public static {factoryReturnType} {factoryMethodName}() => {factoryReturnValue};
            }}");
        return this;
    }

    public TestSourceBuilder WithInjector(
        string interfaceName = "ITestInjector",
        string specType = "TestSpec",
        params string[] methods) {
        
        var methodsStr = methods.Length > 0 
            ? string.Join("\n                ", methods)
            : "int GetInt();";
            
        _types.Add($@"
            [Injector(typeof({specType}))]
            public interface {interfaceName} {{
                {methodsStr}
            }}");
        return this;
    }

    public TestSourceBuilder WithAutoFactory(
        string className = "TestClass",
        string fabricationMode = "FabricationMode.Recurrent",
        string constructorParams = "int value") {
        
        _types.Add($@"
            [AutoFactory({fabricationMode})]
            public class {className} {{
                public {className}({constructorParams}) {{ }}
            }}");
        return this;
    }

    public TestSourceBuilder WithAutoBuilder(
        string className = "TestClass",
        string methodName = "BuildTestClass",
        string parameters = "TestClass instance") {
        
        _types.Add($@"
            public static class {className}Builder {{
                [AutoBuilder]
                public static void {methodName}({parameters}) {{ }}
            }}");
        return this;
    }

    public TestSourceBuilder WithCustomType(string typeDefinition) {
        _types.Add(typeDefinition);
        return this;
    }

    public string Build() {
        var usingsSection = string.Join("\n", _usings);
        var typesSection = string.Join("\n", _types);

        return $@"{usingsSection}

namespace {_namespace} {{{typesSection}
}}";
    }
}
