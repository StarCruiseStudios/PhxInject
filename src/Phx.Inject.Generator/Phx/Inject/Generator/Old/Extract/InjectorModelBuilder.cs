// -----------------------------------------------------------------------------
//  <copyright file="InjectorModelBuilder.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Extract {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Extract.Model;
    using Phx.Inject.Generator.Map;
    using static Construct.GenerationConstants;

    internal class InjectorModelBuilder : IModelBuilder<InjectorModel> {
        private const string GeneratedInjectorClassPrefix = "Generated";

        public InjectorModel Build(ITypeSymbol symbol) {
            var injectorInterfaceType = symbol.ToTypeModel();
            var injectorClassName = GetGeneratedClassName(symbol);
            var injectorType = injectorInterfaceType with { Name = injectorClassName };
            var injectionMethods = GetInjectionMethods(symbol);
            var injectionBuilderMethods = GetInjectionBuilderMethods(symbol);
            var specifications = GetSpecificationTypes(symbol);
            var constructedSpecifications = GetConstructedSpecificationTypes(symbol);
            return new InjectorModel(
                    injectorType,
                    injectorInterfaceType,
                    injectionMethods,
                    injectionBuilderMethods,
                    specifications,
                    constructedSpecifications);
        }

        private static List<InjectionMethodModel> GetInjectionMethods(INamespaceOrTypeSymbol injectorInterfaceSymbol) {
            var injectorMethods = new List<InjectionMethodModel>();
            foreach (var member in injectorInterfaceSymbol.GetMembers()) {
                if (member is not IMethodSymbol methodSymbol) {
                    continue;
                }

                if (methodSymbol.ReturnsVoid) {
                    continue;
                }

                var returnTypeSymbol = methodSymbol.ReturnType;
                var returnType = returnTypeSymbol.ToTypeModel();
                var qualifier = GetMethodQualifier(methodSymbol);
                var methodName = methodSymbol.Name;
                injectorMethods.Add(
                        new InjectionMethodModel(
                                new QualifiedTypeModel(returnType, qualifier),
                                methodName));
            }

            return injectorMethods;
        }

        private static List<InjectionBuilderMethodModel> GetInjectionBuilderMethods(
                INamespaceOrTypeSymbol injectorInterfaceSymbol
        ) {
            var injectorBuilderMethods = new List<InjectionBuilderMethodModel>();
            foreach (var member in injectorInterfaceSymbol.GetMembers()) {
                if (member is not IMethodSymbol methodSymbol) {
                    continue;
                }

                if (!methodSymbol.ReturnsVoid) {
                    continue;
                }

                if (methodSymbol.Parameters.Length != 1) {
                    continue;
                }

                var builtTypeSymbol = methodSymbol.Parameters[0]
                        .Type;
                var builtType = builtTypeSymbol.ToTypeModel();
                var qualifier = GetMethodQualifier(methodSymbol);
                var methodName = methodSymbol.Name;
                injectorBuilderMethods.Add(
                        new InjectionBuilderMethodModel(
                                new QualifiedTypeModel(builtType, qualifier),
                                methodName));
            }

            return injectorBuilderMethods;
        }

        private static IReadOnlyList<TypeModel> GetSpecificationTypes(ISymbol injectorInterfaceSymbol) {
            return GetSpecifications(injectorInterfaceSymbol)
                    .Select(specification => specification.Value as ITypeSymbol)
                    .Where(specificationType => specificationType!.IsStatic)
                    .Select(specificationType => specificationType!.ToTypeModel())
                    .ToImmutableList();
        }

        private static IReadOnlyList<TypeModel> GetConstructedSpecificationTypes(ISymbol injectorInterfaceSymbol) {
            return GetSpecifications(injectorInterfaceSymbol)
                    .Select(specification => specification.Value as ITypeSymbol)
                    .Where(specificationType => specificationType!.IsAbstract)
                    .Select(specificationType => specificationType!.ToTypeModel())
                    .ToImmutableList();
        }

        private static IReadOnlyList<TypedConstant> GetSpecifications(ISymbol model) {
            var injectorAttribute = GetInjectorAttribute(model);
            var specifications = new List<TypedConstant>();
            foreach (var argument in injectorAttribute.ConstructorArguments) {
                if (argument.Kind == TypedConstantKind.Array) {
                    specifications.AddRange(argument.Values);
                }
            }

            return specifications;
        }

        private static AttributeData GetInjectorAttribute(ISymbol interfaceModel) {
            return interfaceModel.GetAttributes()
                    .First(attributeData => attributeData.AttributeClass!.ToString() == InjectorAttributeClassName);
        }

        private static string GetMethodQualifier(ISymbol symbol) {
            var labelAttributes = symbol.GetAttributes()
                    .Where(attributeData => attributeData.AttributeClass!.ToString() == LabelAttributeClassName);

            var qualifierAttributes = symbol.GetAttributes()
                    .Where(
                            attributeData => {
                                return attributeData.AttributeClass!.GetAttributes()
                                        .Any(
                                                parentAttributeData =>
                                                        parentAttributeData.AttributeClass!.ToString() ==
                                                        QualifierAttributeClassName);
                            });

            var numLabels = labelAttributes.Count();
            var numQualifiers = qualifierAttributes.Count();

            if (numLabels + numQualifiers > 1) {
                throw new InvalidOperationException(
                        $"Method {symbol.Name} can only have one Label or Qualifier attribute.");
            }

            if (numLabels > 0) {
                foreach (var argument in labelAttributes.Single()
                                 .ConstructorArguments) {
                    if (argument.Type!.Name == "String") {
                        return (string)argument.Value!;
                    }
                }

                throw new InvalidOperationException(
                        $"Method {symbol.Name} label must provide a value."); // This should never happen.
            }

            if (numQualifiers > 0) {
                return qualifierAttributes.Single()
                        .AttributeClass!.ToString();
            }

            return RegistrationIdentifier.DefaultQualifier;
        }

        private static string GetGeneratedClassName(ISymbol interfaceModel) {
            var injectorAttribute = GetInjectorAttribute(interfaceModel);

            if (injectorAttribute.ConstructorArguments
                        .FirstOrDefault(argument => argument.Kind != TypedConstantKind.Array)
                        .Value is not string injectorClassName
               ) {
                injectorClassName = interfaceModel.Name;
                if (injectorClassName.StartsWith("I")) {
                    injectorClassName = injectorClassName[1..];
                }

                injectorClassName = $"{GeneratedInjectorClassPrefix}{injectorClassName}";
            }

            return injectorClassName ?? throw new InvalidOperationException("Could not determine injector class name.");
        }
    }
}
