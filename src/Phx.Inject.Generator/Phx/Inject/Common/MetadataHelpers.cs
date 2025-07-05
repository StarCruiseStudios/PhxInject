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
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator;
using Phx.Inject.Generator.Render;

namespace Phx.Inject.Common;

internal static class MetadataHelpers {
    public static IResult<ITypeSymbol> ExpectTypeSymbolFromDeclaration(
        TypeDeclarationSyntax syntaxNode,
        IGeneratorContext generatorCtx
    ) {
        var symbol = generatorCtx.ExecutionContext.Compilation
            .GetSemanticModel(syntaxNode.SyntaxTree)
            .GetDeclaredSymbol(syntaxNode);
        return Result.FromNullable(
            symbol as ITypeSymbol,
            $"Expected a type declaration, but found {symbol?.Kind.ToString() ?? "null"} for {syntaxNode.Identifier.Text}.",
            syntaxNode.GetLocation(),
            Diagnostics.InternalError);
    }

    public static IResult<ITypeSymbol?> TryGetDependencyType(ISymbol symbol) {
        return symbol.TryGetDependencyAttribute()
            .Map(attributeData => {
                if (attributeData == null) {
                    return Result.Ok<ITypeSymbol?>(null);
                }

                var constructorArgument = attributeData.ConstructorArguments
                    .Where(argument => argument.Kind == TypedConstantKind.Type)
                    .Select(argument => argument.Value)
                    .OfType<ITypeSymbol>();

                return constructorArgument.Count() == 1
                    ? Result.Ok(constructorArgument.Single())
                    : Result.Error<ITypeSymbol?>(
                        $"DependencyAttribute for symbol {symbol.Name} must provide a dependency type.",
                        symbol.Locations.First(),
                        Diagnostics.InvalidSpecification);
            });
    }

    public static IReadOnlyList<QualifiedTypeModel> TryGetMethodParametersQualifiedTypes(
        IMethodSymbol methodSymbol,
        IGeneratorContext generatorCtx) {
        return methodSymbol.Parameters.Select(parameter => {
                var qualifier = TryGetQualifier(parameter).GetOrThrow(generatorCtx);
                return new QualifiedTypeModel(
                    TypeModel.FromTypeSymbol(parameter.Type),
                    qualifier);
            })
            .ToImmutableList();
    }

    public static IReadOnlyList<QualifiedTypeModel> TryGetConstructorParameterQualifiedTypes(
        ITypeSymbol type,
        IGeneratorContext generatorCtx) {
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
            throw Diagnostics.InvalidSpecification.AsException(
                $"Auto injected type '{type.Name}' must contain exactly one public constructor",
                typeLocation,
                generatorCtx);
        }

        var constructorMethod = constructors.Single();

        return TryGetMethodParametersQualifiedTypes(constructorMethod, generatorCtx);
    }

    public static string GetGeneratedInjectorClassName(
        ITypeSymbol injectorInterfaceSymbol,
        IGeneratorContext generatorCtx) {
        var injectorAttribute = injectorInterfaceSymbol.ExpectInjectorAttribute().GetOrThrow(generatorCtx);
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

    public static IReadOnlyList<ITypeSymbol> TryGetInjectorSpecificationTypes(
        ISymbol injectorInterfaceSymbol,
        IGeneratorContext generatorCtx) {
        var injectorAttribute = injectorInterfaceSymbol.ExpectInjectorAttribute().GetOrThrow(generatorCtx);
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
        IGeneratorContext generatorCtx
    ) {
        IReadOnlyList<SpecFactoryMethodFabricationMode> fabricationModes = factoryAttribute.ConstructorArguments
            .Where(argument => argument.Type!.Name == "FabricationMode")
            .Select(argument => (SpecFactoryMethodFabricationMode)argument.Value!)
            .ToImmutableList();

        return fabricationModes.Count switch {
            0 => SpecFactoryMethodFabricationMode.Recurrent, // The default
            1 => fabricationModes.Single(),
            _ => throw Diagnostics.InternalError.AsException(
                "Factories can only have a single fabrication mode.",
                location,
                generatorCtx)
        };
    }

    public static GeneratorSettings GetGeneratorSettings(AttributeData phxInjectAttribute) {
        var tabSize = phxInjectAttribute.NamedArguments
            .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.TabSize))
            .Value.Value is int value
            ? value
            : 4;

        var generatedFileExtension = phxInjectAttribute.NamedArguments
                .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.GeneratedFileExtension))
                .Value.Value as string
            ?? "generated.cs";

        var nullableEnabled = phxInjectAttribute.NamedArguments
                .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.NullableEnabled))
                .Value.Value as bool?
            ?? true;

        var allowConstructorFactories = phxInjectAttribute.NamedArguments
                .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.AllowConstructorFactories))
                .Value.Value as bool?
            ?? true;

        return new GeneratorSettings(
            tabSize,
            generatedFileExtension,
            nullableEnabled,
            allowConstructorFactories
        );
    }

    public static IResult<string> TryGetQualifier(ISymbol symbol) {
        var labelAttributeResult = symbol.TryGetLabelAttribute();
        if (!labelAttributeResult.IsOk) {
            return labelAttributeResult.MapError<string>();
        }

        var qualifierAttributeResult = symbol.TryGetQualifierAttribute();
        if (!qualifierAttributeResult.IsOk) {
            return qualifierAttributeResult.MapError<string>();
        }

        var labelAttribute = labelAttributeResult.GetValue();
        var qualifierAttribute = qualifierAttributeResult.GetValue();

        if (labelAttribute != null) {
            if (qualifierAttribute != null) {
                return Result.Error<string>(
                    $"Symbol {symbol.Name} can only have one Label or Qualifier attribute.",
                    symbol.Locations.First(),
                    Diagnostics.InvalidSpecification);
            }

            IReadOnlyList<string> labels = labelAttribute
                .ConstructorArguments.Where(argument => argument.Type!.Name == "String")
                .Select(argument => (string)argument.Value!)
                .ToImmutableList();
            return labels.Any()
                ? Result.Ok(labels.Single())
                : Result.Error<string>(
                    $"LabelAttribute for symbol {symbol.Name} must provide a label value.",
                    symbol.Locations.First(),
                    Diagnostics.InvalidSpecification);
        }

        return Result.Ok(
            qualifierAttribute != null
                ? qualifierAttribute.AttributeClass!.ToString()
                : QualifiedTypeModel.NoQualifier);
    }

    public static IReadOnlyDictionary<string, QualifiedTypeModel> GetRequiredPropertyQualifiedTypes(
        ITypeSymbol type,
        IGeneratorContext generatorCtx) {
        return type.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.SetMethod != null && p.SetMethod.DeclaredAccessibility == Accessibility.Public)
            .Where(p => p.IsRequired)
            .ToImmutableDictionary(
                property => property.Name,
                property => new QualifiedTypeModel(
                    TypeModel.FromTypeSymbol(property.Type),
                    TryGetQualifier(property).GetOrThrow(generatorCtx)
                )
            );
    }

    public static IReadOnlyList<IMethodSymbol> GetDirectBuilderMethods(
        ITypeSymbol type,
        IGeneratorContext generatorCtx) {
        return type.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.TryGetBuilderAttribute().GetOrThrow(generatorCtx) != null)
            .ToImmutableList();
    }
}
