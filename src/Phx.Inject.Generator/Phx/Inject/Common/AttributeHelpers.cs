// -----------------------------------------------------------------------------
//  <copyright file="AttributeHelpers.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Phx.Inject.Common;

internal static class AttributeHelpers {
    public const string InjectorAttributeShortName = "Injector";
    public const string InjectorAttributeBaseName = $"{InjectorAttributeShortName}Attribute";
    public const string SpecificationAttributeShortName = "Specification";
    public const string SpecificationAttributeBaseName = $"{SpecificationAttributeShortName}Attribute";

    private const string BuilderAttributeClassName = "Phx.Inject.BuilderAttribute";
    private const string BuilderReferenceAttributeClassName = "Phx.Inject.BuilderReferenceAttribute";
    private const string ChildInjectorAttributeClassName = "Phx.Inject.ChildInjectorAttribute";
    private const string DependencyAttributeClassName = "Phx.Inject.DependencyAttribute";
    private const string FactoryAttributeClassName = "Phx.Inject.FactoryAttribute";
    private const string FactoryReferenceAttributeClassName = "Phx.Inject.FactoryReferenceAttribute";
    private const string InjectorAttributeClassName = $"Phx.Inject.{InjectorAttributeBaseName}";
    private const string LabelAttributeClassName = "Phx.Inject.LabelAttribute";
    private const string LinkAttributeClassName = "Phx.Inject.LinkAttribute";
    private const string PartialAttributeClassName = "Phx.Inject.PartialAttribute";
    private const string QualifierAttributeClassName = "Phx.Inject.QualifierAttribute";
    private const string SpecificationAttributeClassName = $"Phx.Inject.{SpecificationAttributeBaseName}";

    public static AttributeData? GetInjectorAttribute(this ISymbol injectorInterfaceSymbol) {
        var injectorAttributes =
            GetAttributes(injectorInterfaceSymbol, InjectorAttributeClassName);
        return injectorAttributes.Count switch {
            0 => null,
            1 => injectorAttributes.Single(),
            _ => throw new InjectionException(
                Diagnostics.InvalidSpecification,
                $"Injector type {injectorInterfaceSymbol.Name} can only have one Injector attribute. Found {injectorAttributes.Count}.",
                injectorInterfaceSymbol.Locations.First())
        };
    }

    public static IReadOnlyList<AttributeData> GetDependencyAttributes(this ISymbol injectorSymbol) {
        return GetAttributes(injectorSymbol, DependencyAttributeClassName);
    }

    public static IReadOnlyList<AttributeData> GetLabelAttributes(this ISymbol symbol) {
        return GetAttributes(symbol, LabelAttributeClassName);
    }

    public static IReadOnlyList<AttributeData> GetQualifierAttributes(this ISymbol symbol) {
        return GetAttributedAttributes(symbol, QualifierAttributeClassName);
    }

    public static AttributeData? GetSpecificationAttribute(this ISymbol specificationSymbol) {
        var specificationAttributes =
            GetAttributes(specificationSymbol, SpecificationAttributeClassName);
        return specificationAttributes.Count switch {
            0 => null,
            1 => specificationAttributes.Single(),
            _ => throw new InjectionException(
                Diagnostics.InvalidSpecification,
                $"Specification type {specificationSymbol.Name} can only have one Specification attribute. Found {specificationAttributes.Count}.",
                specificationSymbol.Locations.First())
        };
    }

    public static IReadOnlyList<AttributeData> GetLinkAttributes(this ISymbol specificationSymbol) {
        return GetAttributes(specificationSymbol, LinkAttributeClassName);
    }

    public static IReadOnlyList<AttributeData> GetFactoryAttributes(this ISymbol factoryMethodSymbol) {
        return GetAttributes(factoryMethodSymbol, FactoryAttributeClassName);
    }

    public static IReadOnlyList<AttributeData> GetFactoryReferenceAttributes(this ISymbol factoryMethodSymbol) {
        return GetAttributes(factoryMethodSymbol, FactoryReferenceAttributeClassName);
    }

    public static AttributeData? GetBuilderAttribute(this ISymbol builderSymbol) {
        var builderAttributes = GetAttributes(builderSymbol, BuilderAttributeClassName);
        var numBuilderAttributes = builderAttributes.Count;
        if (numBuilderAttributes == 0) {
            return null;
        }

        if (numBuilderAttributes > 1) {
            throw new InjectionException(
                Diagnostics.InvalidSpecification,
                "Builders can only have a single builder attribute.",
                builderSymbol.Locations.First());
        }

        if (!builderSymbol.IsStatic
            || builderSymbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)
        ) {
            throw new InjectionException(
                Diagnostics.InvalidSpecification,
                "Builders must be public or internal static methods.",
                builderSymbol.Locations.First());
        }

        return builderAttributes.First();
    }

    public static AttributeData? GetBuilderReferenceAttributes(this ISymbol builderReferenceSymbol) {
        var builderReferenceAttribute =
            GetAttributes(builderReferenceSymbol, BuilderReferenceAttributeClassName);
        var numBuilderReferenceAttributes = builderReferenceAttribute.Count;
        if (numBuilderReferenceAttributes == 0) {
            return null;
        }

        if (numBuilderReferenceAttributes > 1) {
            throw new InjectionException(
                Diagnostics.InvalidSpecification,
                "Builder references can only have a single builder reference attribute.",
                builderReferenceSymbol.Locations.First());
        }

        if (!builderReferenceSymbol.IsStatic
            || builderReferenceSymbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)
        ) {
            throw new InjectionException(
                Diagnostics.InvalidSpecification,
                "Builders references must be public or internal static methods.",
                builderReferenceSymbol.Locations.First());
        }

        return builderReferenceAttribute.First();
    }

    public static IReadOnlyList<AttributeData> GetChildInjectorAttributes(this ISymbol childInjectorMethodSymbol) {
        return GetAttributes(childInjectorMethodSymbol, ChildInjectorAttributeClassName);
    }

    public static IReadOnlyList<AttributeData> GetPartialAttributes(this ISymbol partialMethodSymbol) {
        return GetAttributes(partialMethodSymbol, PartialAttributeClassName);
    }

    private static IReadOnlyList<AttributeData> GetAttributes(ISymbol symbol, string attributeClassName) {
        return symbol.GetAttributes()
            .Where(attributeData => attributeData.AttributeClass!.ToString() == attributeClassName)
            .ToImmutableList();
    }

    private static IReadOnlyList<AttributeData> GetAttributedAttributes(
        ISymbol symbol,
        string attributeAttributeClassName) {
        return symbol.GetAttributes()
            .Where(attributeData => {
                var attributeAttributes = GetAttributes(
                    attributeData.AttributeClass!,
                    attributeAttributeClassName);
                return attributeAttributes.Count > 0;
            })
            .ToImmutableList();
    }
}
