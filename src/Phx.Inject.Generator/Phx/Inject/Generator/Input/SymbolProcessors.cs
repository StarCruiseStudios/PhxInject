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
    using System.Text.RegularExpressions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Phx.Inject.Generator.Model;
    using Phx.Inject.Generator.Model.Specifications;

    internal static class SymbolProcessors {
        public const string BuilderAttributeClassName = "Phx.Inject.BuilderAttribute";
        public const string ChildInjectorAttributeClassName = "Phx.Inject.ChildInjectorAttribute";
        public const string ExternalDependencyAttributeClassName = "Phx.Inject.ExternalDependencyAttribute";
        public const string FactoryAttributeClassName = "Phx.Inject.FactoryAttribute";
        public const string InjectorAttributeClassName = "Phx.Inject.InjectorAttribute";
        public const string LabelAttributeClassName = "Phx.Inject.LabelAttribute";
        public const string LinkAttributeClassName = "Phx.Inject.LinkAttribute";
        public const string QualifierAttributeClassName = "Phx.Inject.QualifierAttribute";
        public const string SpecificationAttributeClassName = "Phx.Inject.SpecificationAttribute";

        private const string GeneratedInjectorClassPrefix = "Generated";

        private static Regex validCharsRegex = new Regex(@"[^a-zA-Z0-9_]");

        public static string GetSpecContainerReferenceName(TypeModel specContainerType) {
            return GetValidReferenceName(specContainerType.TypeName, startLowercase: false);
        }

        public static string GetInstanceHolderName(QualifiedTypeModel heldInstanceType) {
            string referenceName = string.IsNullOrEmpty(heldInstanceType.Qualifier)
                    ? heldInstanceType.TypeModel.TypeName
                    : $"{heldInstanceType.Qualifier}_{heldInstanceType.TypeModel.TypeName}";
            referenceName = GetValidReferenceName(referenceName, startLowercase: true);
            return referenceName;
        }

        public static IEnumerable<ITypeSymbol> GetTypeSymbolsFromDeclarations(
                IEnumerable<TypeDeclarationSyntax> syntaxNodes,
                GeneratorExecutionContext context
        ) {
            return syntaxNodes.Select(
                            syntaxNode => {
                                var semanticModel = context.Compilation.GetSemanticModel(syntaxNode.SyntaxTree);
                                return semanticModel.GetDeclaredSymbol(syntaxNode) as ITypeSymbol;
                            })
                    .Where(symbol => symbol != null)
                    .Select(symbol => symbol!)
                    .ToImmutableList();
        }

        public static IList<AttributeData> GetAttributes(ISymbol symbol, string attributeClassName) {
            return symbol.GetAttributes()
                    .Where(attributeData => attributeData.AttributeClass!.ToString() == attributeClassName)
                    .ToImmutableList();
        }

        public static IList<AttributeData> GetAttributedAttributes(ISymbol symbol, string attributeAttributeClassName) {
            return symbol.GetAttributes()
                    .Where(attributeData => {
                        var attributeAttributes = GetAttributes(
                                attributeData.AttributeClass!,
                                attributeAttributeClassName);
                        return attributeAttributes.Count > 0;
                    })
                    .ToImmutableList();
        }

        public static AttributeData? GetInjectorAttribute(ISymbol injectorInterfaceSymbol) {
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

        public static AttributeData? GetSpecificationAttribute(ISymbol specificationSymbol) {
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

        public static IList<AttributeData> GetLinkAttributes(ISymbol specificationSymbol) {
            return GetAttributes(specificationSymbol, LinkAttributeClassName);
        }

        public static IList<AttributeData> GetFactoryAttributes(ISymbol factoryMethodSymbol) {
            return GetAttributes(factoryMethodSymbol, FactoryAttributeClassName);
        }

        public static IList<AttributeData> GetBuilderAttributes(ISymbol builderMethodSymbol) {
            return GetAttributes(builderMethodSymbol, BuilderAttributeClassName);
        }

        public static IList<AttributeData> GetChildInjectorAttributes(ISymbol childInjectorMethodSymbol) {
            return GetAttributes(childInjectorMethodSymbol, ChildInjectorAttributeClassName);
        }

        public static IEnumerable<ITypeSymbol> GetExternalDependencyTypes(ISymbol injectorSymbol) {
            var externalDependencyAttributes = GetAttributes(injectorSymbol, ExternalDependencyAttributeClassName);
            return externalDependencyAttributes.SelectMany(
                    attributeData => {
                        return attributeData.ConstructorArguments
                                .Where(argument => argument.Kind == TypedConstantKind.Type)
                                .Select(argument => argument.Value)
                                .OfType<ITypeSymbol>()
                                .ToImmutableList();
                    }).ToImmutableList();
        }

        public static ImmutableList<QualifiedTypeModel> GetMethodParametersQualifiedTypes(IMethodSymbol methodSymbol) {
            return methodSymbol.Parameters.Select(
                            parameter => {
                                var qualifier = GetQualifier(parameter);
                                return new QualifiedTypeModel(
                                        TypeModel.FromTypeSymbol(parameter.Type),
                                        qualifier,
                                        parameter.Locations.First());
                            })
                    .ToImmutableList();
        }

        public static string GetValidReferenceName(string baseName, bool startLowercase) {
            var referenceName = baseName;

            referenceName = referenceName.Replace(".", "_");
            referenceName = validCharsRegex.Replace(referenceName, "");

            // Start with a lowercase letter.
            if (startLowercase) {
                referenceName = StartLowercase(referenceName);
            }

            return referenceName;
        }

        public static string StartLowercase(string input) {
            return char.ToLower(input[0]) + input[1..];
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
                generatedClassName = GetValidReferenceName($"{GeneratedInjectorClassPrefix}{generatedClassName}", startLowercase: false);
            }

            return generatedClassName;
        }

        public static IEnumerable<IMethodSymbol> GetChildInjectors(ITypeSymbol injectorInterfaceSymbol) {
            return injectorInterfaceSymbol.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(methodSymbol => GetAttributes(methodSymbol, ChildInjectorAttributeClassName).Any())
                    .ToImmutableList();
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

        public static SpecFactoryMethodFabricationMode GetFactoryFabricationMode(AttributeData factoryAttribute, Location location) {
            var fabricationModes = factoryAttribute.ConstructorArguments.Where(argument => argument.Type!.Name == "FabricationMode")
                    .Select(argument => (SpecFactoryMethodFabricationMode)argument.Value!)
                    .ToImmutableList();

            return fabricationModes.Count switch {
                0 => SpecFactoryMethodFabricationMode.Recurrent, // The default
                1 => fabricationModes.Single(),
                _ => throw new InjectionException(
                        Diagnostics.InternalError,
                        $"Factories can only have a single fabrication mode.",
                        location)
            };
        }

        public static string GetQualifier(ISymbol symbol) {
            var labelAttributes = GetAttributes(symbol, LabelAttributeClassName);
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

            return QualifiedTypeModel.NoQualifier;
        }

        public static TypeModel CreateSpecContainerType(TypeModel injectorType, TypeModel specType) {
            return specType with {
                TypeName = GetValidReferenceName($"{injectorType.TypeName}_{specType.TypeName}", startLowercase: false)
            };
        }

        public static TypeModel CreateExternalDependencyImplementationType(
                TypeModel injectorType,
                TypeModel dependencyInterfaceType
        ) {
            var implementationName = dependencyInterfaceType.TypeName;
            if (implementationName.StartsWith("I")) {
                implementationName = implementationName[1..];
            }

            implementationName = GetValidReferenceName($"{injectorType.TypeName}_{implementationName}", startLowercase: false);
            return injectorType with { TypeName = implementationName };
        }
    }
}
