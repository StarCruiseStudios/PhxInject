// -----------------------------------------------------------------------------
//  <copyright file="SpecFactoryDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Specifications.Descriptors {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Descriptors;

    internal delegate SpecFactoryDescriptor? CreateSpecFactoryDescriptor(
            IMethodSymbol factoryMethod,
            DescriptorGenerationContext context
    );

    internal record SpecFactoryDescriptor(
            QualifiedTypeModel ReturnType,
            string FactoryMethodName,
            IEnumerable<QualifiedTypeModel> Parameters,
            SpecFactoryMethodFabricationMode FabricationMode,
            Location Location
    ) : IDescriptor {
        public class Builder {
            public SpecFactoryDescriptor? Build(IMethodSymbol factoryMethod, DescriptorGenerationContext context) {
                var factoryAttributes = SymbolProcessors.GetFactoryAttributes(factoryMethod);

                var numFactoryAttributes = factoryAttributes.Count;
                if (numFactoryAttributes == 0) {
                    // This is not a factory method.
                    return null;
                }

                var factoryLocation = factoryMethod.Locations.First();

                if (numFactoryAttributes > 1) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            "Method can only have a single builder attribute.",
                            factoryLocation);
                }

                var factoryAttribute = factoryAttributes.Single();
                var fabricationMode = SymbolProcessors.GetFactoryFabricationMode(
                        factoryAttribute,
                        factoryLocation);

                var methodParameterTypes = SymbolProcessors.GetMethodParametersQualifiedTypes(factoryMethod);

                var qualifier = SymbolProcessors.GetQualifier(factoryMethod);
                var returnTypeModel = TypeModel.FromTypeSymbol(factoryMethod.ReturnType);
                var returnType = new QualifiedTypeModel(
                        returnTypeModel,
                        qualifier);

                return new SpecFactoryDescriptor(
                        returnType,
                        factoryMethod.Name,
                        methodParameterTypes,
                        fabricationMode,
                        factoryLocation);
            }
        }
    }
}
