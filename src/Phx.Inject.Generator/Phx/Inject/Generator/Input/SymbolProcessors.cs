// -----------------------------------------------------------------------------
//  <copyright file="SymbolProcessors.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Input {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model;

    internal static class SymbolProcessors {
        public const string BuilderAttributeClassName = "Phx.Inject.BuilderAttribute";
        public const string FactoryAttributeClassName = "Phx.Inject.FactoryAttribute";
        public const string InjectorAttributeClassName = "Phx.Inject.InjectorAttribute";
        public const string LabelAttributeClassName = "Phx.Inject.LabelAttribute";
        public const string LinkAttributeClassName = "Phx.Inject.LinkAttribute";
        public const string QualifierAttributeClassName = "Phx.Inject.QualifierAttribute";
        public const string SpecificationAttributeClassName = "Phx.Inject.SpecificationAttribute";

        private const string GeneratedInjectorClassPrefix = "Generated";
        private const string NoQualifier = "";

        public static IList<AttributeData> GetAttribute(ISymbol symbol, string attributeClassName) {
            return symbol.GetAttributes()
                    .Where(attributeData => attributeData.AttributeClass!.ToString() == attributeClassName)
                    .ToImmutableList();
        }

        public static IList<AttributeData> GetAttributedAttributes(ISymbol symbol, string attributeAttributeClassName) {
            return symbol.GetAttributes()
                    .SelectMany(
                            attributeData => GetAttribute(attributeData.AttributeClass!, attributeAttributeClassName))
                    .ToImmutableList();
        }

        public static AttributeData? GetInjectorAttribute(ISymbol injectorInterfaceSymbol) {
            var injectorAttributes = GetAttribute(injectorInterfaceSymbol, InjectorAttributeClassName);
            return injectorAttributes.Count switch {
                0 => null,
                1 => injectorAttributes.Single(),
                _ => throw new InjectionException(
                        Diagnostics.InvalidSpecification,
                        $"Injector type {injectorInterfaceSymbol.Name} can only have one Injector attribute. Found {injectorAttributes.Count}.",
                        injectorInterfaceSymbol.Locations.First())
            };
        }

        public static string GetGeneratedInjectorClassName(ISymbol injectorInterfaceSymbol) {
            var injectorAttribute = GetInjectorAttribute(injectorInterfaceSymbol);
            if (injectorAttribute == null) {
                throw new InjectionException(
                        Diagnostics.InternalError,
                        $"Injector type {injectorInterfaceSymbol.Name} must have one Injector attribute.",
                        injectorInterfaceSymbol.Locations.First());
            }

            var generatedClassName = injectorAttribute.ConstructorArguments
                    .FirstOrDefault(argument => argument.Kind != TypedConstantKind.Array)
                    .Value as string;

            if (generatedClassName == null) {
                generatedClassName = injectorInterfaceSymbol.Name;

                // Remove the "I" prefix from interface names.
                if (generatedClassName.StartsWith("I")) {
                    generatedClassName = generatedClassName[1..];
                }

                // Add the generated prefix
                generatedClassName = $"{GeneratedInjectorClassPrefix}{generatedClassName}";
            }

            return generatedClassName;
        }

        public static IEnumerable<ITypeSymbol> GetInjectorSpecificationTypes(ISymbol injectorInterfaceSymbol) {
            var injectorAttribute = GetInjectorAttribute(injectorInterfaceSymbol);
            if (injectorAttribute == null) {
                throw new InjectionException(
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

        public static string GetQualifier(ISymbol symbol) {
            var labelAttributes = GetAttribute(symbol, LabelAttributeClassName);
            var qualifierAttributes = GetAttributedAttributes(symbol, QualifierAttributeClassName);
            var numLabels = labelAttributes.Count;
            var numQualifiers = qualifierAttributes.Count;

            if (numLabels + numQualifiers > 1) {
                throw new InjectionException(
                        Diagnostics.InvalidSpecification,
                        $"Symbol {symbol.Name} can only have one Label or Qualifier attribute. Found {numLabels + numQualifiers}.",
                        symbol.Locations.First());
            }

            if (numLabels > 0) {
                var labels = labelAttributes.Single()
                        .ConstructorArguments.Where(argument => argument.Type!.Name == "String")
                        .Select(argument => (string)argument.Value!);
                return labels.Count() switch {
                    1 => labels.Single(),
                    _ => throw new InjectionException(
                            Diagnostics.InternalError,
                            $"Label for symbol {symbol.Name} must have exactly one label value.",
                            symbol.Locations.First()) // This should never happen
                };
            }

            if (numQualifiers > 0) {
                return qualifierAttributes.Single()
                        .AttributeClass!.ToString();
            }

            return NoQualifier;
        }
    }
}
