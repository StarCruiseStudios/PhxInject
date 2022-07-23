// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerMapper.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------


namespace Phx.Inject.Generator.Map {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Phx.Inject.Generator.Construct.Definitions;
    using Phx.Inject.Generator.Extract.Model;
    using static Phx.Inject.Generator.Construct.GenerationConstants;
    
    internal class SpecContainerMapper : ISpecContainerMapper {
        private const string InstanceHolderSuffix = "Instance";
        public SpecContainerDefinition MapToDefinition(
            SpecificationModel specModel,
            InjectorModel injectorModel,
            IDictionary<TypeDefinition, FactoryRegistration> factoryRegistrations
        ) {
            var specContainerName = specModel.SpecificationType.Name + SpecificationContainerSuffix;
            var specContainerType = specModel.SpecificationType with { Name = specContainerName };

            var instanceHolders = new List<InstanceHolderDefinition>();
            var factoryMethodContainers = new List<FactoryMethodContainerDefinition>();

            foreach (var factory in specModel.Factories) {
                var specContainerCollectionName = $"{injectorModel.InjectorType.Name}.{SpecContainerCollectionInterfaceName}";
                TypeModel specContainerCollectionType = injectorModel.InjectorType with { Name = specContainerCollectionName };
                InstanceHolderDefinition? instanceHolderDefinition = null;
                if (factory.FabricationMode == FabricationMode.Scoped) {
                    var instanceHolderName = factory.Name + InstanceHolderSuffix;
                    if (instanceHolderName.StartsWith("Get", ignoreCase: true, CultureInfo.InvariantCulture)) {
                        instanceHolderName = instanceHolderName[3..];
                    }
                    instanceHolderName = char.ToLower(instanceHolderName[0]) + instanceHolderName[1..];

                    instanceHolderDefinition = new InstanceHolderDefinition(
                        factory.ReturnType.ToTypeDefinition(),
                        instanceHolderName);
                    instanceHolders.Add(instanceHolderDefinition);
                }

                var arguments = factory.Arguments.Select(argumentType => {
                    if (!factoryRegistrations.TryGetValue(argumentType.ToTypeDefinition(), out var factoryMethodRegistration)) {
                        throw new InvalidOperationException($"No Factory found for type {argumentType.QualifiedName}.");
                    }

                    return new FactoryMethodContainerInvocationDefinition(
                        factoryMethodRegistration.SpecificationType.Name + SpecificationContainerSuffix,
                        factoryMethodRegistration.FactoryModel.Name
                    );
                });

                var factoryMethodContainerDefinition = new FactoryMethodContainerDefinition(
                    factory.ReturnType.ToTypeDefinition(),
                    specModel.SpecificationType.ToTypeDefinition(),
                    specContainerCollectionType.ToTypeDefinition(),
                    factory.Name,
                    instanceHolderDefinition,
                    arguments);
                factoryMethodContainers.Add(factoryMethodContainerDefinition);
            }

            return new SpecContainerDefinition(
                specContainerType.ToTypeDefinition(),
                instanceHolders,
                factoryMethodContainers);
        }
    }
}