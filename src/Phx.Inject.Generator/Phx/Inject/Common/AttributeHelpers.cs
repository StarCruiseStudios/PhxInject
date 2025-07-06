// -----------------------------------------------------------------------------
// <copyright file="AttributeHelpers.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Common;

internal static class AttributeHelpers {
    public static IResult<PhxInjectAttributeDesc?> TryGetPhxInjectAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, TypeNames.PhxInjectAttributeClassName)
            .MapNullable(it => {
                var tabSize = it.NamedArguments
                    .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.TabSize))
                    .Value.Value as int?;

                var generatedFileExtension = it.NamedArguments
                    .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.GeneratedFileExtension))
                    .Value.Value as string;

                var nullableEnabled = it.NamedArguments
                    .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.NullableEnabled))
                    .Value.Value as bool?;

                var allowConstructorFactories = it.NamedArguments
                    .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.AllowConstructorFactories))
                    .Value.Value as bool?;
                
                return Result.Ok(new PhxInjectAttributeDesc(tabSize, generatedFileExtension, nullableEnabled, allowConstructorFactories, symbol, it));
            });
    }

    public static IResult<InjectorAttributeDesc?> TryGetInjectorAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, TypeNames.InjectorAttributeClassName)
            .MapNullable(it => {
                var generatedClassName = it.ConstructorArguments
                    .FirstOrDefault(argument => argument.Kind != TypedConstantKind.Array)
                    .Value as string;

                var specifications = it.ConstructorArguments
                    .Where(argument => argument.Kind == TypedConstantKind.Array)
                    .SelectMany(argument => argument.Values)
                    .Where(type => type.Value is ITypeSymbol)
                    .Select(type => (type.Value as ITypeSymbol)!)
                    .ToImmutableList();
                
                return Result.Ok(new InjectorAttributeDesc(generatedClassName, specifications, symbol, it));
            });
    }

    public static IResult<InjectorAttributeDesc> ExpectInjectorAttribute(this ISymbol symbol) {
        return symbol.TryGetInjectorAttribute()
            .Map(attributeData => Result.ErrorIfNull(
                attributeData,
                $"Expected type {symbol.Name} to have one {TypeNames.InjectorAttributeClassName}.",
                symbol.Locations.First(),
                Diagnostics.InvalidSpecification));
    }

    public static IResult<SpecificationAttributeDesc?> TryGetSpecificationAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, TypeNames.SpecificationAttributeClassName)
            .MapNullable(it => Result.Ok(new SpecificationAttributeDesc(symbol, it)));
    }

    public static IResult<DependencyAttributeDesc?> TryGetDependencyAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, TypeNames.DependencyAttributeClassName)
            .MapNullable(it => {
                var constructorArgument = it.ConstructorArguments
                    .Where(argument => argument.Kind == TypedConstantKind.Type)
                    .Select(argument => argument.Value)
                    .OfType<ITypeSymbol>()
                    .ToImmutableList();

                if (constructorArgument.Count != 1) {
                    return Result.Error<DependencyAttributeDesc?>(
                        $"Dependency for symbol {symbol.Name} must provide a dependency type.",
                        it.GetLocation() ?? symbol.Locations.First(),
                        Diagnostics.InvalidSpecification);
                }
                
                return Result.Ok(new DependencyAttributeDesc(constructorArgument.Single(), symbol, it));
            });
    }

    public static IResult<LabelAttributeDesc?> TryGetLabelAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, TypeNames.LabelAttributeClassName)
            .MapNullable(it => {
                var labels = it.ConstructorArguments.Where(argument => argument.Type!.ToString() == TypeNames.StringClassName)
                    .Select(argument => (string)argument.Value!)
                    .ToImmutableList();

                if (labels.Count == 1) {
                    return Result.Ok(new LabelAttributeDesc(labels.Single(), symbol, it));
                }
                
                return Result.Error<LabelAttributeDesc?>(
                    $"Label for symbol {symbol.Name} must provide one label value.",
                    it.GetLocation() ?? symbol.Locations.First(),
                    Diagnostics.InvalidSpecification);
            });
    }

    public static IResult<QualifierAttributeDesc?> TryGetQualifierAttribute(this ISymbol symbol) {
        return TryGetSingleAttributedAttribute(symbol, TypeNames.QualifierAttributeClassName)
            .MapNullable(it => Result.Ok(new QualifierAttributeDesc(symbol, it)));
    }
    
    public static IResult<QualifierAttributeDesc?> TryGetQualifierAttributeFromAttributeType(this INamedTypeSymbol attributeTypeSymbol, AttributeDesc attribute) {
        if (attributeTypeSymbol.BaseType?.ToString() != TypeNames.AttributeClassName) {
            return Result.Error<QualifierAttributeDesc?>(
                $"Expected qualifier type {attributeTypeSymbol.Name} to be an Attribute type.",
                attribute.Location,
                Diagnostics.InvalidSpecification);
        }
                
        return TryGetSingleAttribute(attributeTypeSymbol, TypeNames.QualifierAttributeClassName)
            .Map(it => it == null
                ? Result.Error<AttributeData?>(
                    $"Expected attribute type {attributeTypeSymbol.Name} to have one {TypeNames.QualifierAttributeClassName}.",
                    attribute.Location,
                    Diagnostics.InvalidSpecification)
                : Result.Ok(it))
            .MapNullable(it => Result.Ok(new QualifierAttributeDesc(attribute.AttributedSymbol, attributeTypeSymbol)));
    }

    public static IReadOnlyList<IResult<LinkAttributeDesc>> GetAllLinkAttributes(this ISymbol symbol) {
        return TryGetAttributes(symbol, TypeNames.LinkAttributeClassName)
            .Select(it => {
                if (it.ConstructorArguments.Length != 2) {
                    return Result.Error<LinkAttributeDesc>(
                        "Link attribute must have only an input and return type specified.",
                        it.GetLocation() ?? symbol.Locations.First(),
                        Diagnostics.InternalError);
                }

                var inputTypeArgument = it.ConstructorArguments[0].Value as ITypeSymbol;
                var returnTypeArgument = it.ConstructorArguments[1].Value as ITypeSymbol;

                if (inputTypeArgument == null || returnTypeArgument == null) {
                    return Result.Error<LinkAttributeDesc>(
                        "Link attribute must specify non-null types.",
                        it.GetLocation() ?? symbol.Locations.First(),
                        Diagnostics.InvalidSpecification);
                }
                
                var inputLabel = it.NamedArguments
                    .FirstOrDefault(arg => arg.Key == nameof(LinkAttribute.InputLabel))
                    .Value.Value as string;

                var inputQualifier = it.NamedArguments
                    .FirstOrDefault(arg => arg.Key == nameof(LinkAttribute.InputQualifier))
                    .Value.Value as INamedTypeSymbol;

                var outputLabel = it.NamedArguments
                    .FirstOrDefault(arg => arg.Key == nameof(LinkAttribute.OutputLabel))
                    .Value.Value as string;

                var outputQualifier = it.NamedArguments
                    .FirstOrDefault(arg => arg.Key == nameof(LinkAttribute.OutputQualifier))
                    .Value.Value as INamedTypeSymbol;
                
                if (inputLabel != null && inputQualifier != null) {
                    return Result.Error<LinkAttributeDesc>(
                        "Link attribute cannot specify both input label and qualifier.",
                        it.GetLocation() ?? symbol.Locations.First(),
                        Diagnostics.InvalidSpecification);
                }
                
                if (outputLabel != null && outputQualifier != null) {
                    return Result.Error<LinkAttributeDesc>(
                        "Link attribute cannot specify both input label and qualifier.",
                        it.GetLocation() ?? symbol.Locations.First(),
                        Diagnostics.InvalidSpecification);
                }
                
                return Result.Ok(new LinkAttributeDesc(
                    inputTypeArgument, 
                    returnTypeArgument, 
                    inputQualifier, 
                    inputLabel, 
                    outputQualifier, 
                    outputLabel, 
                    symbol, 
                    it));
            }).ToImmutableList();
    }

    public static IResult<FactoryAttributeDesc?> TryGetFactoryAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, TypeNames.FactoryAttributeClassName)
            .MapNullable(it => {
                IReadOnlyList<FactoryFabricationMode> fabricationModes = it.ConstructorArguments
                    .Where(argument => argument.Type!.ToString() == TypeNames.FabricationModeClassName)
                    .Select(argument => (FactoryFabricationMode)argument.Value!)
                    .ToImmutableList();

                var fabricationMode = FactoryFabricationMode.Recurrent;
                switch (fabricationModes.Count) {
                    case > 1:
                        return Result.Error<FactoryAttributeDesc?>(
                            "Factories can only have a single fabrication mode.",
                            it.GetLocation() ?? symbol.Locations.First(),
                            Diagnostics.InternalError);
                    case 1:
                        fabricationMode = fabricationModes.Single();
                        break;
                }

                return Result.Ok(new FactoryAttributeDesc(fabricationMode, symbol, it));
            });
    }

    public static IResult<FactoryReferenceAttributeDesc?> TryGetFactoryReferenceAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, TypeNames.FactoryReferenceAttributeClassName)
            .MapNullable(it => {
                IReadOnlyList<FactoryFabricationMode> fabricationModes = it.ConstructorArguments
                    .Where(argument => argument.Type!.ToString() == TypeNames.FabricationModeClassName)
                    .Select(argument => (FactoryFabricationMode)argument.Value!)
                    .ToImmutableList();

                var fabricationMode = FactoryFabricationMode.Recurrent;
                switch (fabricationModes.Count) {
                    case > 1:
                        return Result.Error<FactoryReferenceAttributeDesc?>(
                            "Factory references can only have a single fabrication mode.",
                            it.GetLocation() ?? symbol.Locations.First(),
                            Diagnostics.InternalError);
                    case 1:
                        fabricationMode = fabricationModes.Single();
                        break;
                }

                return Result.Ok(new FactoryReferenceAttributeDesc(fabricationMode, symbol, it));
            });
    }

    public static IResult<BuilderAttributeDesc?> TryGetBuilderAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, TypeNames.BuilderAttributeClassName)
            .MapNullable(it => Result.Ok(new BuilderAttributeDesc(symbol, it)));
    }

    public static IResult<BuilderReferenceAttributeDesc?> TryGetBuilderReferenceAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, TypeNames.BuilderReferenceAttributeClassName)
            .MapNullable(it => Result.Ok(new BuilderReferenceAttributeDesc(symbol, it)));
    }

    public static IResult<ChildInjectorAttributeDesc?> TryGetChildInjectorAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, TypeNames.ChildInjectorAttributeClassName)
            .MapNullable(it => Result.Ok(new ChildInjectorAttributeDesc(symbol, it)));
    }

    public static IResult<PartialAttributeDesc?> TryGetPartialAttribute(this ISymbol symbol) {
        return TryGetSingleAttribute(symbol, TypeNames.PartialAttributeClassName)
            .MapNullable(it => Result.Ok(new PartialAttributeDesc(symbol, it)));
    }

    private static IReadOnlyList<AttributeData> TryGetAttributes(ISymbol symbol, string attributeClassName) {
        return symbol.GetAttributes()
            .Where(attributeData => attributeData.AttributeClass?.ToString() == attributeClassName)
            .ToImmutableList();
    }

    private static IResult<AttributeData?> TryGetSingleAttribute(ISymbol symbol, string attributeClassName) {
        var attributes = TryGetAttributes(symbol, attributeClassName);
        return attributes.Count switch {
            > 1 => Result.Error<AttributeData?>(
                $"Type {symbol.Name} can only have one {attributeClassName}. Found {attributes.Count}.",
                symbol.Locations.First(),
                Diagnostics.InvalidSpecification),
            _ => Result.Ok<AttributeData?>(attributes.SingleOrDefault())
        };
    }
    
    private static IReadOnlyList<AttributeData> TryGetAttributedAttributes(
        ISymbol symbol,
        string parentAttributeClassName) {
        return symbol.GetAttributes()
            .Where(attributeData => TryGetAttributes(attributeData.AttributeClass!, parentAttributeClassName).Any())
            .ToImmutableList();
    }

    private static IResult<AttributeData?> TryGetSingleAttributedAttribute(
        ISymbol symbol,
        string parentAttributeClassName) {
        var attributes = TryGetAttributedAttributes(symbol, parentAttributeClassName);
        return attributes.Count switch {
            > 1 => Result.Error<AttributeData?>(
                $"Type {symbol.Name} can only have one {parentAttributeClassName}. Found {attributes.Count}: [{string.Join(", ", attributes.Select(it => it.AttributeClass!.Name))}].",
                symbol.Locations.First(),
                Diagnostics.InvalidSpecification),
            _ => Result.Ok<AttributeData?>(attributes.SingleOrDefault())
        };
    }

    public static Location? GetLocation(this AttributeData attributeData) {
        return attributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation();
    }
}
