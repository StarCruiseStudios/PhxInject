// -----------------------------------------------------------------------------
//  <copyright file="AttributeHelpers.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Common {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static class AttributeHelpers {
        private const string BuilderAttributeClassName = "Phx.Inject.BuilderAttribute";
        private const string ChildInjectorAttributeClassName = "Phx.Inject.ChildInjectorAttribute";
        private const string ExternalDependencyAttributeClassName = "Phx.Inject.ExternalDependencyAttribute";
        private const string FactoryAttributeClassName = "Phx.Inject.FactoryAttribute";
        private const string InjectorAttributeClassName = "Phx.Inject.InjectorAttribute";
        private const string LabelAttributeClassName = "Phx.Inject.LabelAttribute";
        private const string LinkAttributeClassName = "Phx.Inject.LinkAttribute";
        private const string QualifierAttributeClassName = "Phx.Inject.QualifierAttribute";
        private const string SpecificationAttributeClassName = "Phx.Inject.SpecificationAttribute";

        public static AttributeData? GetInjectorAttribute(this ISymbol injectorInterfaceSymbol) {
            var injectorAttributes = GetAttributes(injectorInterfaceSymbol, InjectorAttributeClassName);
            return injectorAttributes.Count switch {
                0 => null,
                1 => injectorAttributes.Single(),
                _ => throw new InjectionException(
                        Diagnostics.InvalidSpecification,
                        $"Injector type {injectorInterfaceSymbol.Name} can only have one Injector attribute. Found {injectorAttributes.Count}.",
                        injectorInterfaceSymbol.Locations.First())
            };
        }

        public static IEnumerable<AttributeData> GetExternalDependencyAttributes(this ISymbol injectorSymbol) {
            return GetAttributes(injectorSymbol, ExternalDependencyAttributeClassName);
        }

        public static IEnumerable<AttributeData> GetLabelAttributes(this ISymbol symbol) {
            return GetAttributes(symbol, LabelAttributeClassName);
        }

        public static IEnumerable<AttributeData> GetQualifierAttributes(this ISymbol symbol) {
            return GetAttributedAttributes(symbol, QualifierAttributeClassName);
        }

        public static AttributeData? GetSpecificationAttribute(this ISymbol specificationSymbol) {
            var specificationAttributes = GetAttributes(specificationSymbol, SpecificationAttributeClassName);
            return specificationAttributes.Count switch {
                0 => null,
                1 => specificationAttributes.Single(),
                _ => throw new InjectionException(
                        Diagnostics.InvalidSpecification,
                        $"Specification type {specificationSymbol.Name} can only have one Specification attribute. Found {specificationAttributes.Count}.",
                        specificationSymbol.Locations.First())
            };
        }

        public static IList<AttributeData> GetLinkAttributes(this ISymbol specificationSymbol) {
            return GetAttributes(specificationSymbol, LinkAttributeClassName);
        }

        public static IList<AttributeData> GetFactoryAttributes(this ISymbol factoryMethodSymbol) {
            return GetAttributes(factoryMethodSymbol, FactoryAttributeClassName);
        }

        public static IList<AttributeData> GetBuilderAttributes(this ISymbol builderMethodSymbol) {
            return GetAttributes(builderMethodSymbol, BuilderAttributeClassName);
        }

        public static IList<AttributeData> GetChildInjectorAttributes(this ISymbol childInjectorMethodSymbol) {
            return GetAttributes(childInjectorMethodSymbol, ChildInjectorAttributeClassName);
        }

        private static IList<AttributeData> GetAttributes(ISymbol symbol, string attributeClassName) {
            return symbol.GetAttributes()
                    .Where(attributeData => attributeData.AttributeClass!.ToString() == attributeClassName)
                    .ToImmutableList();
        }

        private static IList<AttributeData> GetAttributedAttributes(ISymbol symbol, string attributeAttributeClassName) {
            return symbol.GetAttributes()
                    .Where(
                            attributeData => {
                                var attributeAttributes = GetAttributes(
                                        attributeData.AttributeClass!,
                                        attributeAttributeClassName);
                                return attributeAttributes.Count > 0;
                            })
                    .ToImmutableList();
        }
    }
}
