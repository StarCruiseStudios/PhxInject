// -----------------------------------------------------------------------------
//  <copyright file="InjectionMapper.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Map {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Phx.Inject.Generator.Construct.Definitions;
    using Phx.Inject.Generator.Extract.Model;

    internal class InjectionMapper : IInjectionMapper {
        private readonly IInjectorMapper injectorMapper;
        private readonly ISpecContainerMapper specContainerMapper;

        private readonly IDictionary<TypeDefinition, FactoryRegistration> factoryRegistrations
            = new Dictionary<TypeDefinition, FactoryRegistration>();

        // TODO:
        private readonly IDictionary<TypeDefinition, BuilderRegistration> builderRegistrations
            = new Dictionary<TypeDefinition, BuilderRegistration>();

        public InjectionMapper(IInjectorMapper injectorMapper, ISpecContainerMapper specContainerMapper) {
            this.injectorMapper = injectorMapper;
            this.specContainerMapper = specContainerMapper;
        }

        public InjectionDefinition MapToDefinition(InjectorModel injectorModel, IEnumerable<SpecificationModel> specModels) {
            foreach (var specModel in specModels) {
                foreach (var factory in specModel.Factories) {
                    factoryRegistrations.Add(factory.ReturnType.ToTypeDefinition(), new FactoryRegistration(specModel.SpecificationType, factory));
                }
            }

            foreach (var specModel in specModels) {
                foreach (var link in specModel.Links) {
                    if (factoryRegistrations.TryGetValue(link.InputType.ToTypeDefinition(), out var targetRegistration)) {
                        factoryRegistrations.Add(link.ReturnType.ToTypeDefinition(), targetRegistration);
                    } else {
                        throw new InvalidOperationException($"Cannot link {link.ReturnType} to unknown type {link.InputType}.");
                    }
                }
            }

            var injectorDefinition = injectorMapper.MapToDefinition(injectorModel, factoryRegistrations, builderRegistrations);
            var specContainerDefintions = specModels.Where(specModel => specModel.Factories.Count > 0)
                .Select(specModel =>
                    specContainerMapper.MapToDefinition(specModel, injectorModel, factoryRegistrations));

            return new InjectionDefinition(
                injectorDefinition,
                specContainerDefintions);
        }
    }
}