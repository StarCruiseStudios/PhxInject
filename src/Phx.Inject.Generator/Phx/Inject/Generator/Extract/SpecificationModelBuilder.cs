// -----------------------------------------------------------------------------
//  <copyright file="SpecificationModelBuilder.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Extract {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Extract.Model;
    using static Phx.Inject.Generator.Construct.GenerationConstants;
    
    internal class SpecificationModelBuilder : IModelBuilder<SpecificationModel> {
        public SpecificationModel Build(ITypeSymbol symbol) {
            var links = GetLinks(symbol);

            var specType = symbol.ToTypeModel();
            var factories = new List<FactoryModel>();

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
                        factories.Add(new FactoryModel(
                            returnType,
                            methodName,
                            argumentTypes,
                            fabricationMode
                        ));
                    }
                }
            }

            return new SpecificationModel(specType, factories, links);
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
                links.Add(new LinkModel(inputType, returnType));
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
    }
}
