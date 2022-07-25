// -----------------------------------------------------------------------------
//  <copyright file="InjectorModelBuilder.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Extract {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Extract.Model;
    using static Phx.Inject.Generator.Construct.GenerationConstants;

    internal class InjectorModelBuilder : IModelBuilder<InjectorModel> {
        private const string GeneratedInjectorClassPrefix = "Generated";

        public InjectorModel Build(ITypeSymbol symbol) {
            var injectorInterfaceType = symbol.ToTypeModel();
            var injectorClassName = GetGeneratedClassName(symbol);
            var injectorType = injectorInterfaceType with { Name = injectorClassName };
            var injectionMethods = GetInjectionMethods(symbol);
            var injectionBuilderMethods = GetInjectionBuilderMethods(symbol);
            var specifications = GetSpecificationTypes(symbol);
            return new InjectorModel(injectorType, injectorInterfaceType, injectionMethods, injectionBuilderMethods, specifications);
        }

        private List<InjectionMethodModel> GetInjectionMethods(ITypeSymbol injectorInterfaceSymbol) {
            var injectorMethods = new List<InjectionMethodModel>();
            foreach (var member in injectorInterfaceSymbol.GetMembers()) {
                if (member is IMethodSymbol methodSymbol) {
                    if (methodSymbol.ReturnsVoid) {
                        continue;
                    }

                    var returnTypeSymbol = methodSymbol.ReturnType;
                    var returnType = returnTypeSymbol.ToTypeModel();
                    var methodName = methodSymbol.Name;
                    injectorMethods.Add(new InjectionMethodModel(
                        returnType,
                        methodName
                    ));
                }
            }

            return injectorMethods;
        }

        private List<InjectionBuilderMethodModel> GetInjectionBuilderMethods(ITypeSymbol injectorInterfaceSymbol) {
            var injectorBuilderMethods = new List<InjectionBuilderMethodModel>();
            foreach (var member in injectorInterfaceSymbol.GetMembers()) {
                if (member is IMethodSymbol methodSymbol) {
                    if (!methodSymbol.ReturnsVoid) {
                        continue;
                    }
                    if (methodSymbol.Parameters.Length != 1) {
                        continue;
                    }

                    var builtTypeSymbol = methodSymbol.Parameters[0].Type;
                    var builtType = builtTypeSymbol.ToTypeModel();
                    var methodName = methodSymbol.Name;
                    injectorBuilderMethods.Add(new InjectionBuilderMethodModel(
                        builtType,
                        methodName
                    ));
                }
            }

            return injectorBuilderMethods;
        }

        private IReadOnlyList<TypeModel> GetSpecificationTypes(ITypeSymbol injectorInterfaceSymbol) {
            return GetSpecifications(injectorInterfaceSymbol)
                .Select(specification => specification.Value as ITypeSymbol)
                .Select(specificationType => specificationType!.ToTypeModel())
                .ToImmutableList();
        }
        private IReadOnlyList<TypedConstant> GetSpecifications(ITypeSymbol interfaceModel) {
            var injectorAttribute = GetInjectorAttribute(interfaceModel);
            var specifications = new List<TypedConstant>();
            foreach (var argument in injectorAttribute.ConstructorArguments) {
                if (argument.Kind == TypedConstantKind.Array) {
                    specifications.AddRange(argument.Values);
                }
            }

            return specifications;
        }

        private AttributeData GetInjectorAttribute(ITypeSymbol interfaceModel) {
            return interfaceModel.GetAttributes()
                .First((attributeData) => attributeData.AttributeClass!.ToString() == InjectorAttributeClassName);
        }

        private string GetGeneratedClassName(ITypeSymbol interfaceModel) {
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
