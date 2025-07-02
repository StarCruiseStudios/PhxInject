// -----------------------------------------------------------------------------
//  <copyright file="AttributeHelpers.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Phx.Inject.Common;

internal static class AttributeHelpers {
    public static AttributeData? GetPhxInjectAttribute(this ISymbol symbol, GeneratorExecutionContext context) {
        var phxInjectAttributes =
            GetAttributes(symbol, PhxInjectNames.Attributes.PhxInjectAttributeClassName);
        return phxInjectAttributes.Count switch {
            0 => null,
            1 => phxInjectAttributes.Single(),
            _ => throw new InjectionException(
                context,
                Diagnostics.InvalidSpecification,
                $"Symbol {symbol.Name} can only have one PhxInject attribute. Found {phxInjectAttributes.Count}.",
                symbol.Locations.First())
        };
    }
    
    public static AttributeData? GetInjectorAttribute(this ISymbol injectorInterfaceSymbol, GeneratorExecutionContext context) {
        var injectorAttributes =
            GetAttributes(injectorInterfaceSymbol, PhxInjectNames.Attributes.InjectorAttributeClassName);
        return injectorAttributes.Count switch {
            0 => null,
            1 => injectorAttributes.Single(),
            _ => throw new InjectionException(
                context,
                Diagnostics.InvalidSpecification,
                $"Injector type {injectorInterfaceSymbol.Name} can only have one Injector attribute. Found {injectorAttributes.Count}.",
                injectorInterfaceSymbol.Locations.First())
        };
    }

    public static IReadOnlyList<AttributeData> GetDependencyAttributes(this ISymbol injectorSymbol) {
        return GetAttributes(injectorSymbol, PhxInjectNames.Attributes.DependencyAttributeClassName);
    }

    public static IReadOnlyList<AttributeData> GetLabelAttributes(this ISymbol symbol) {
        return GetAttributes(symbol, PhxInjectNames.Attributes.LabelAttributeClassName);
    }

    public static IReadOnlyList<AttributeData> GetQualifierAttributes(this ISymbol symbol) {
        return GetAttributedAttributes(symbol, PhxInjectNames.Attributes.QualifierAttributeClassName);
    }
    
    public static AttributeData? GetSpecificationAttribute(this ISymbol specificationSymbol, GeneratorExecutionContext context) {
        var specificationAttributes =
            GetAttributes(specificationSymbol, PhxInjectNames.Attributes.SpecificationAttributeClassName);
        return specificationAttributes.Count switch {
            0 => null,
            1 => specificationAttributes.Single(),
            _ => throw new InjectionException(
                context,
                Diagnostics.InvalidSpecification,
                $"Specification type {specificationSymbol.Name} can only have one Specification attribute. Found {specificationAttributes.Count}.",
                specificationSymbol.Locations.First())
        };
    }

    public static IReadOnlyList<AttributeData> GetLinkAttributes(this ISymbol specificationSymbol) {
        return GetAttributes(specificationSymbol, PhxInjectNames.Attributes.LinkAttributeClassName);
    }

    public static IReadOnlyList<AttributeData> GetFactoryAttributes(this ISymbol factoryMethodSymbol) {
        return GetAttributes(factoryMethodSymbol, PhxInjectNames.Attributes.FactoryAttributeClassName);
    }

    public static IReadOnlyList<AttributeData> GetFactoryReferenceAttributes(this ISymbol factoryMethodSymbol) {
        return GetAttributes(factoryMethodSymbol, PhxInjectNames.Attributes.FactoryReferenceAttributeClassName);
    }

    public static AttributeData? GetBuilderAttribute(this ISymbol builderSymbol, GeneratorExecutionContext context) {
        var builderAttributes = GetAttributes(builderSymbol, PhxInjectNames.Attributes.BuilderAttributeClassName);
        var numBuilderAttributes = builderAttributes.Count;
        if (numBuilderAttributes == 0) {
            return null;
        }

        if (numBuilderAttributes > 1) {
            throw new InjectionException(
                context,
                Diagnostics.InvalidSpecification,
                "Builders can only have a single builder attribute.",
                builderSymbol.Locations.First());
        }

        if (!builderSymbol.IsStatic
            || builderSymbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)
        ) {
            throw new InjectionException(
                context,
                Diagnostics.InvalidSpecification,
                "Builders must be public or internal static methods.",
                builderSymbol.Locations.First());
        }

        return builderAttributes.First();
    }

    public static AttributeData? GetBuilderReferenceAttributes(this ISymbol builderReferenceSymbol, GeneratorExecutionContext context) {
        var builderReferenceAttribute =
            GetAttributes(builderReferenceSymbol, PhxInjectNames.Attributes.BuilderReferenceAttributeClassName);
        var numBuilderReferenceAttributes = builderReferenceAttribute.Count;
        if (numBuilderReferenceAttributes == 0) {
            return null;
        }

        if (numBuilderReferenceAttributes > 1) {
            throw new InjectionException(
                context,
                Diagnostics.InvalidSpecification,
                "Builder references can only have a single builder reference attribute.",
                builderReferenceSymbol.Locations.First());
        }

        if (!builderReferenceSymbol.IsStatic
            || builderReferenceSymbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)
        ) {
            throw new InjectionException(
                context,
                Diagnostics.InvalidSpecification,
                "Builders references must be public or internal static methods.",
                builderReferenceSymbol.Locations.First());
        }

        return builderReferenceAttribute.First();
    }

    public static IReadOnlyList<AttributeData> GetChildInjectorAttributes(this ISymbol childInjectorMethodSymbol) {
        return GetAttributes(childInjectorMethodSymbol, PhxInjectNames.Attributes.ChildInjectorAttributeClassName);
    }

    public static IReadOnlyList<AttributeData> GetPartialAttributes(this ISymbol partialMethodSymbol) {
        return GetAttributes(partialMethodSymbol, PhxInjectNames.Attributes.PartialAttributeClassName);
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
