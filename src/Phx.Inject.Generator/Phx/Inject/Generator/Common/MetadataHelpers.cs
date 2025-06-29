// -----------------------------------------------------------------------------
//  <copyright file="MetadataHelpers.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Common {
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Phx.Inject.Generator.Model;

    internal static class MetadataHelpers {
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

        public static IEnumerable<ITypeSymbol> GetExternalDependencyTypes(ISymbol injectorSymbol) {
            var externalDependencyAttributes = injectorSymbol.GetExternalDependencyAttributes();
            return externalDependencyAttributes.SelectMany(
                    attributeData => {
                        return attributeData.ConstructorArguments
                            .Where(argument => argument.Kind == TypedConstantKind.Type)
                            .Select(argument => argument.Value)
                            .OfType<ITypeSymbol>()
                            .ToImmutableList();
                    })
                .ToImmutableList();
        }

        public static ImmutableList<QualifiedTypeModel> GetMethodParametersQualifiedTypes(IMethodSymbol methodSymbol) {
            return methodSymbol.Parameters.Select(
                    parameter => {
                        var qualifier = GetQualifier(parameter);
                        return new QualifiedTypeModel(
                            TypeModel.FromTypeSymbol(parameter.Type),
                            qualifier);
                    })
                .ToImmutableList();
        }

        public static ImmutableList<QualifiedTypeModel> GetConstructorParameterQualifiedTypes(ITypeSymbol type) {
            var typeLocation = type.Locations.First();

            var isVisible = type.DeclaredAccessibility == Accessibility.Public
                || type.DeclaredAccessibility == Accessibility.Internal;
            if (!isVisible || type.IsStatic || type.IsAbstract) {
                throw new InjectionException(
                    Diagnostics.InvalidSpecification,
                    $"Auto injected type '{type.Name}' must be public or internal, non-static, and non-abstract.",
                    typeLocation);
            }

            var constructors = type
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.MethodKind == MethodKind.Constructor && m.DeclaredAccessibility == Accessibility.Public)
                .ToList();
            if (constructors.Count != 1) {
                throw new InjectionException(
                    Diagnostics.InvalidSpecification,
                    $"Auto injected type '{type.Name}' must contain exactly one public constructor",
                    typeLocation);
            }

            var constructorMethod = constructors.Single();

            return GetMethodParametersQualifiedTypes(constructorMethod);
        }

        public static string GetGeneratedInjectorClassName(ITypeSymbol injectorInterfaceSymbol) {
            var injectorAttribute = injectorInterfaceSymbol.GetInjectorAttribute();
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
                generatedClassName = TypeModel.FromTypeSymbol(injectorInterfaceSymbol).GetInjectorClassName();
            } else {
                generatedClassName = generatedClassName.AsValidIdentifier().StartUppercase();
            }

            return generatedClassName;
        }

        public static IEnumerable<ITypeSymbol> GetInjectorSpecificationTypes(ISymbol injectorInterfaceSymbol) {
            var injectorAttribute = injectorInterfaceSymbol.GetInjectorAttribute();
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

        public static SpecFactoryMethodFabricationMode GetFactoryFabricationMode(
            AttributeData factoryAttribute,
            Location location
        ) {
            var fabricationModes = factoryAttribute.ConstructorArguments
                .Where(argument => argument.Type!.Name == "FabricationMode")
                .Select(argument => (SpecFactoryMethodFabricationMode)argument.Value!)
                .ToImmutableList();

            return fabricationModes.Count switch {
                0 => SpecFactoryMethodFabricationMode.Recurrent, // The default
                1 => fabricationModes.Single(),
                _ => throw new InjectionException(
                    Diagnostics.InternalError,
                    "Factories can only have a single fabrication mode.",
                    location)
            };
        }

        public static string GetQualifier(ISymbol symbol) {
            var labelAttributes = symbol.GetLabelAttributes();
            var qualifierAttributes = symbol.GetQualifierAttributes();
            var numLabels = labelAttributes.Count();
            var numQualifiers = qualifierAttributes.Count();

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

        public static ImmutableDictionary<string, QualifiedTypeModel> GetRequiredPropertyQualifiedTypes(ITypeSymbol type) {
            return type.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.SetMethod != null && p.SetMethod.DeclaredAccessibility == Accessibility.Public)
                .Where(p => p.IsRequired)
                .ToImmutableDictionary(
                    property => property.Name,
                    property => new QualifiedTypeModel(
                        TypeModel.FromTypeSymbol(property.Type),
                        GetQualifier(property)
                    )
                );
        }

        public static ImmutableList<IMethodSymbol> GetDirectBuilderMethods(ITypeSymbol type) {
            return type.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => AttributeHelpers.GetBuilderAttribute(m) != null)
                .ToImmutableList();
        }
    }
}
