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

        public static IList<AttributeData> GetAttribute(ISymbol symbol, string attributeClassName) {
            return symbol.GetAttributes()
                    .Where(attributeData => attributeData.AttributeClass!.ToString() == attributeClassName)
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
    }
}
