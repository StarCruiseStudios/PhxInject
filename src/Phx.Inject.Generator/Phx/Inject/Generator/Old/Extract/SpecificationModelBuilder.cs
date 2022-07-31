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
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Extract.Model;
    using Phx.Inject.Generator.Map;
    using static Construct.GenerationConstants;

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
                    var argumentTypes = new List<QualifiedTypeModel>();
                    foreach (var argument in methodSymbol.Parameters) {
                        var argumentQualifier = GetQualifier(argument);
                        argumentTypes.Add(
                                new QualifiedTypeModel(
                                        argument.Type.ToTypeModel(),
                                        argumentQualifier));
                    }

                    if (GetFactoryFabricationMode(methodSymbol) is FabricationMode fabricationMode) {
                        var qualifier = GetQualifier(methodSymbol);

                        factories.Add(
                                new FactoryModel(
                                        new QualifiedTypeModel(returnType, qualifier),
                                        methodName,
                                        argumentTypes,
                                        fabricationMode));
                    } else if (IsBuilder(methodSymbol)) {
                        var builtType = argumentTypes[0];
                        var builderArguments = argumentTypes.GetRange(index: 1, argumentTypes.Count - 1);
                        var qualifier = GetQualifier(methodSymbol);
                        builders.Add(
                                new BuilderModel(
                                        builtType with {
                                            Qualifier = qualifier
                                        }, // Use qualifier from method not from parameter
                                        methodName,
                                        builderArguments));
                    }
                }
            }

            return new SpecificationModel(specType, factories, builders, links);
        }

        private static IReadOnlyList<LinkModel> GetLinks(ISymbol symbol) {
            var links = new List<LinkModel>();
            var linkAttributes = symbol.GetAttributes()
                    .Where(attributeData => attributeData.AttributeClass!.ToString() == LinkAttributeClassName);
            foreach (var linkAttribute in linkAttributes) {
                var inputTypeArgument = linkAttribute.ConstructorArguments[0]
                        .Value as ITypeSymbol;
                var returnTypeArgument = linkAttribute.ConstructorArguments[1]
                        .Value as ITypeSymbol;

                var inputType = new TypeModel(
                        inputTypeArgument!.ContainingNamespace.ToString(),
                        inputTypeArgument.Name);
                var returnType = new TypeModel(
                        returnTypeArgument!.ContainingNamespace.ToString(),
                        returnTypeArgument.Name);
                links.Add(
                        new LinkModel(
                                inputType,
                                RegistrationIdentifier.DefaultQualifier,
                                returnType,
                                RegistrationIdentifier.DefaultQualifier));
            }

            return links;
        }

        private static FabricationMode? GetFactoryFabricationMode(ISymbol model) {
            var factoryAttributes = model.GetAttributes()
                    .Where(attributeData => attributeData.AttributeClass!.ToString() == FactoryAttributeClassName)
                    .ToImmutableList();
            if (!factoryAttributes.Any()) {
                return null;
            }

            if (!model.IsStatic) {
                return null;
            }

            foreach (var attribute in factoryAttributes) {
                foreach (var argument in attribute.ConstructorArguments) {
                    if (argument.Type!.Name == "FabricationMode") {
                        return (FabricationMode)argument.Value!;
                    }
                }
            }

            return FabricationMode.Recurrent;
        }

        private static string GetQualifier(ISymbol qualifiedSymbol) {
            var labelAttributes = qualifiedSymbol.GetAttributes()
                    .Where(attributeData => attributeData.AttributeClass!.ToString() == LabelAttributeClassName);

            var qualifierAttributes = qualifiedSymbol.GetAttributes()
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
                        $"Method {qualifiedSymbol.Name} can only have one Label or Qualifier attribute.");
            }

            if (numLabels > 0) {
                foreach (var argument in labelAttributes.Single()
                                 .ConstructorArguments) {
                    if (argument.Type!.Name == "String") {
                        return (string)argument.Value!;
                    }
                }

                throw new InvalidOperationException(
                        $"Method {qualifiedSymbol.Name} label must provide a value."); // This should never happen.
            }

            if (numQualifiers > 0) {
                return qualifierAttributes.Single()
                        .AttributeClass!.ToString();
            }

            return RegistrationIdentifier.DefaultQualifier;
        }

        private static bool IsBuilder(ISymbol model) {
            var builderAttributes = model.GetAttributes()
                    .Where(attributeData => attributeData.AttributeClass!.ToString() == BuilderAttributeClassName);
            if (!builderAttributes.Any()) {
                return false;
            }

            return model.IsStatic;
        }
    }
}
