// -----------------------------------------------------------------------------
//  <copyright file="MetadataHelpers.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Render;

namespace Phx.Inject.Common;

internal static class MetadataHelpers {
    public static IReadOnlyList<ITypeSymbol> GetTypeSymbolsFromDeclarations(
        IEnumerable<TypeDeclarationSyntax> syntaxNodes,
        GeneratorExecutionContext context
    ) {
        return syntaxNodes.Select(syntaxNode => {
                var semanticModel = context.Compilation.GetSemanticModel(syntaxNode.SyntaxTree);
                return semanticModel.GetDeclaredSymbol(syntaxNode) as ITypeSymbol;
            })
            .Where(symbol => symbol != null)
            .Select(symbol => symbol!)
            .ToImmutableList();
    }

    public static IReadOnlyList<ITypeSymbol> GetDependencyTypes(ISymbol injectorSymbol) {
        var dependencyAttributes = injectorSymbol.GetDependencyAttributes();
        return dependencyAttributes.SelectMany(attributeData => {
                return attributeData.ConstructorArguments
                    .Where(argument => argument.Kind == TypedConstantKind.Type)
                    .Select(argument => argument.Value)
                    .OfType<ITypeSymbol>();
            })
            .ToImmutableList();
    }

    public static IReadOnlyList<QualifiedTypeModel> GetMethodParametersQualifiedTypes(IMethodSymbol methodSymbol, GeneratorExecutionContext context) {
        return methodSymbol.Parameters.Select(parameter => {
                var qualifier = GetQualifier(parameter, context);
                return new QualifiedTypeModel(
                    TypeModel.FromTypeSymbol(parameter.Type),
                    qualifier);
            })
            .ToImmutableList();
    }

    public static IReadOnlyList<QualifiedTypeModel> GetConstructorParameterQualifiedTypes(ITypeSymbol type, GeneratorExecutionContext context) {
        var typeLocation = type.Locations.First();

        if (type.IsStatic || type.IsAbstract || type.TypeKind == TypeKind.Interface) {
            return ImmutableList<QualifiedTypeModel>.Empty;
        }

        IReadOnlyList<IMethodSymbol> constructors = type
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Constructor && m.DeclaredAccessibility == Accessibility.Public)
            .ToImmutableList();
        if (constructors.Count != 1) {
            throw new InjectionException(
                context,
                Diagnostics.InvalidSpecification,
                $"Auto injected type '{type.Name}' must contain exactly one public constructor",
                typeLocation);
        }

        var constructorMethod = constructors.Single();

        return GetMethodParametersQualifiedTypes(constructorMethod, context);
    }

    public static string GetGeneratedInjectorClassName(ITypeSymbol injectorInterfaceSymbol, GeneratorExecutionContext context) {
        var injectorAttribute = injectorInterfaceSymbol.GetInjectorAttribute(context);
        if (injectorAttribute == null) {
            throw new InjectionException(
                context,
                Diagnostics.InternalError,
                $"Injector type {injectorInterfaceSymbol.Name} must have one Injector attribute.",
                injectorInterfaceSymbol.Locations.First());
        }

        var generatedClassName = injectorAttribute.ConstructorArguments
            .FirstOrDefault(argument => argument.Kind != TypedConstantKind.Array)
            .Value as string;

        if (generatedClassName == null) {
            generatedClassName = TypeModel.FromTypeSymbol(injectorInterfaceSymbol).GetInjectorClassName();
        } else {
            generatedClassName = generatedClassName.AsValidIdentifier().StartUppercase();
        }

        return generatedClassName;
    }

    public static IReadOnlyList<ITypeSymbol> GetInjectorSpecificationTypes(ISymbol injectorInterfaceSymbol, GeneratorExecutionContext context) {
        var injectorAttribute = injectorInterfaceSymbol.GetInjectorAttribute(context);
        if (injectorAttribute == null) {
            throw new InjectionException(
                context,
                Diagnostics.InternalError,
                $"Injector type {injectorInterfaceSymbol.Name} must have one Injector attribute.",
                injectorInterfaceSymbol.Locations.First());
        }

        return injectorAttribute.ConstructorArguments
            .Where(argument => argument.Kind == TypedConstantKind.Array)
            .SelectMany(argument => argument.Values)
            .Where(type => type.Value is ITypeSymbol)
            .Select(type => (type.Value as ITypeSymbol)!)
            .ToImmutableList();
    }

    public static SpecFactoryMethodFabricationMode GetFactoryFabricationMode(
        AttributeData factoryAttribute,
        Location location,
        GeneratorExecutionContext context
    ) {
        IReadOnlyList<SpecFactoryMethodFabricationMode> fabricationModes = factoryAttribute.ConstructorArguments
            .Where(argument => argument.Type!.Name == "FabricationMode")
            .Select(argument => (SpecFactoryMethodFabricationMode)argument.Value!)
            .ToImmutableList();

        return fabricationModes.Count switch {
            0 => SpecFactoryMethodFabricationMode.Recurrent, // The default
            1 => fabricationModes.Single(),
            _ => throw new InjectionException(
                context,
                Diagnostics.InternalError,
                "Factories can only have a single fabrication mode.",
                location)
        };
    }
    
    public static GeneratorSettings GetGeneratorSettings(AttributeData phxInjectAttribute, GeneratorExecutionContext context) {
        var tabSize = phxInjectAttribute.NamedArguments
            .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.TabSize))
            .Value.Value is int value ? value : 4;

        var generatedFileExtension = phxInjectAttribute.NamedArguments
            .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.GeneratedFileExtension))
            .Value.Value as string ?? "generated.cs";

        var nullableEnabled = phxInjectAttribute.NamedArguments
            .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.NullableEnabled))
            .Value.Value as bool? ?? true;

        var allowConstructorFactories = phxInjectAttribute.NamedArguments
            .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.AllowConstructorFactories))
            .Value.Value as bool? ?? true;

        return new GeneratorSettings(
            tabSize,
            generatedFileExtension,
            nullableEnabled,
            allowConstructorFactories
        );
    }

    public static string GetQualifier(ISymbol symbol, GeneratorExecutionContext context) {
        var labelAttributes = symbol.GetLabelAttributes();
        var qualifierAttributes = symbol.GetQualifierAttributes();
        var numLabels = labelAttributes.Count();
        var numQualifiers = qualifierAttributes.Count();

        if (numLabels + numQualifiers > 1) {
            throw new InjectionException(
                context,
                Diagnostics.InvalidSpecification,
                $"Symbol {symbol.Name} can only have one Label or Qualifier attribute. Found {numLabels + numQualifiers}.",
                symbol.Locations.First());
        }

        if (numLabels > 0) {
            IReadOnlyList<string> labels = labelAttributes.Single()
                .ConstructorArguments.Where(argument => argument.Type!.Name == "String")
                .Select(argument => (string)argument.Value!)
                .ToImmutableList();
            return labels.Any()
                ? labels.Single()
                : throw new InjectionException(
                    context,
                    Diagnostics.InternalError,
                    $"Label for symbol {symbol.Name} must have exactly one label value.",
                    symbol.Locations.First()); // This should never happen
        }

        if (numQualifiers > 0) {
            return qualifierAttributes.Single()
                .AttributeClass!.ToString();
        }

        return QualifiedTypeModel.NoQualifier;
    }

    public static IReadOnlyDictionary<string, QualifiedTypeModel> GetRequiredPropertyQualifiedTypes(ITypeSymbol type, GeneratorExecutionContext context) {
        return type.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.SetMethod != null && p.SetMethod.DeclaredAccessibility == Accessibility.Public)
            .Where(p => p.IsRequired)
            .ToImmutableDictionary(
                property => property.Name,
                property => new QualifiedTypeModel(
                    TypeModel.FromTypeSymbol(property.Type),
                    GetQualifier(property, context)
                )
            );
    }

    public static IReadOnlyList<IMethodSymbol> GetDirectBuilderMethods(ITypeSymbol type, GeneratorExecutionContext context) {
        return type.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.GetBuilderAttribute(context) != null)
            .ToImmutableList();
    }
}
