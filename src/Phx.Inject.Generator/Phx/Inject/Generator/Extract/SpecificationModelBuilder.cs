// -----------------------------------------------------------------------------
//  <copyright file="SpecificationModelBuilder.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Extract {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Extract.Model;
    using Phx.Inject.Generator.Map;
    using static Phx.Inject.Generator.Construct.GenerationConstants;

    internal class SpecificationModelBuilder : IModelBuilder<SpecificationModel> {
        public SpecificationModel Build(ITypeSymbol symbol) {
            var links = GetLinks(symbol);

            var specType = symbol.ToTypeModel();
            var factories = new List<FactoryModel>();
            var builders = new List<BuilderModel>();

            foreach (var member in symbol.GetMembers()) {
                if (member is IMethodSymbol methodSymbol) {
                    var returnTypeSymbol = methodSymbol.ReturnType;
                    var returnType = returnTypeSymbol.ToTypeModel();
                    var methodName = methodSymbol.Name;
                    var argumentTypes = new List<TypeModel>();
                    foreach (var argument in methodSymbol.Parameters) {
                        argumentTypes.Add(argument.Type.ToTypeModel());
                    }

                    if (GetFactoryFabricationMode(methodSymbol) is FabricationMode fabricationMode) {
                        var qualifier = GetMethodQualifier(methodSymbol);

                        factories.Add(new FactoryModel(
                            returnType,
                            qualifier,
                            methodName,
                            argumentTypes,
                            fabricationMode
                        ));
                    } else if (IsBuilder(methodSymbol)) {
                        var builtType = argumentTypes[0];
                        var builderArguments = argumentTypes.GetRange(1, argumentTypes.Count - 1);
                        var qualifier = GetMethodQualifier(methodSymbol);
                        builders.Add(new BuilderModel(
                            builtType,
                            qualifier,
                            methodName,
                            builderArguments
                        ));
                    }
                }
            }

            return new SpecificationModel(specType, factories, builders, links);
        }

        private IReadOnlyList<LinkModel> GetLinks(ITypeSymbol symbol) {
            var links = new List<LinkModel>();
            var linkAttributes = symbol.GetAttributes()
                .Where((attributeData) => attributeData.AttributeClass!.ToString() == LinkAttributeClassName);
            foreach (var linkAttribute in linkAttributes) {
                var inputTypeArgument = linkAttribute.ConstructorArguments[0].Value as ITypeSymbol;
                var returnTypeArgument = linkAttribute.ConstructorArguments[1].Value as ITypeSymbol;

                var inputType = new TypeModel(
                    inputTypeArgument!.ContainingNamespace.ToString(),
                    inputTypeArgument.Name);
                var returnType = new TypeModel(
                    returnTypeArgument!.ContainingNamespace.ToString(),
                    returnTypeArgument.Name);
                links.Add(new LinkModel(inputType, RegistrationIdentifier.DefaultQualifier, returnType, RegistrationIdentifier.DefaultQualifier));
            }

            return links;
        }

        private FabricationMode? GetFactoryFabricationMode(IMethodSymbol factoryModel) {
            var factoryAttributes = factoryModel.GetAttributes()
                .Where((attributeData) => attributeData.AttributeClass!.ToString() == FactoryAttributeClassName);
            if (!factoryAttributes.Any()) {
                return null;
            }

            if (!factoryModel.IsStatic) {
                return null;
            }

            var specifications = new List<TypedConstant>();
            foreach (var attribute in factoryAttributes) {
                foreach (var argument in attribute.ConstructorArguments) {
                    if (argument.Type!.Name == "FabricationMode") {
                        return (FabricationMode) argument.Value!;
                    }
                }
            }

            return FabricationMode.Recurrent;
        }

        private string GetMethodQualifier(IMethodSymbol factoryModel) {
            var labelAttributes = factoryModel.GetAttributes()
                .Where((attributeData) => attributeData.AttributeClass!.ToString() == LabelAttributeClassName);

            var qualifierAttributes = factoryModel.GetAttributes()
                .Where((attributeData) => {
                    return attributeData.AttributeClass!.GetAttributes()
                            .Any((parentAttributeData) => parentAttributeData.AttributeClass!.ToString() == QualifierAttributeClassName);
                });

            var numLabels = labelAttributes.Count();
            var numQualifiers = qualifierAttributes.Count();

            if (numLabels + numQualifiers > 1) {
                throw new InvalidOperationException($"Factory {factoryModel.Name} can only have one Label or Qualifier attribute.");
            } else if (numLabels > 0) {
                foreach (var argument in labelAttributes.Single().ConstructorArguments) {
                    if (argument.Type!.Name == "String") {
                        return (string) argument.Value!;
                    }
                }
                throw new InvalidOperationException($"Factory {factoryModel.Name} label must provide a value."); // This should never happen.
            } else if (numQualifiers > 0) {
                return qualifierAttributes.Single().AttributeClass!.ToString();
            } else {
                return RegistrationIdentifier.DefaultQualifier;
            }
        }

        private bool IsBuilder(IMethodSymbol builderModel) {
            var builderAttributes = builderModel.GetAttributes()
                .Where((attributeData) => attributeData.AttributeClass!.ToString() == BuilderAttributeClassName);
            if (!builderAttributes.Any()) {
                return false;
            }

            return builderModel.IsStatic;
        }
    }
}
