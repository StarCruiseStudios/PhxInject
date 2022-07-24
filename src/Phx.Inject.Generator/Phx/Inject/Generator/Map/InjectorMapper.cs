// -----------------------------------------------------------------------------
//  <copyright file="InjectorMapper.cs" company="Star Cruise Studios LLC">
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
    using static Phx.Inject.Generator.Construct.GenerationConstants;

    internal class InjectorMapper : IInjectorMapper {
        public InjectorDefinition MapToDefinition(
            InjectorModel injectorModel,
            IDictionary<TypeDefinition, FactoryRegistration> factoryRegistrations,
            IDictionary<TypeDefinition, BuilderRegistration> builderRegistrations
        ) {
            var injectorMethods = injectorModel.InjectionMethods.Select(method => {
                if (!factoryRegistrations.TryGetValue(method.ReturnType.ToTypeDefinition(), out var factoryMethodRegistration)) {
                    throw new InvalidOperationException($"No Factory found for type {method.ReturnType.QualifiedName}.");
                }

                var factoryMethodContainerInvocation = new FactoryMethodContainerInvocationDefinition(
                    factoryMethodRegistration.SpecificationType.Name + SpecificationContainerSuffix,
                    factoryMethodRegistration.FactoryModel.Name
                );
                return new InjectorMethodDefinition(
                    method.ReturnType.ToTypeDefinition(),
                    method.Name,
                    factoryMethodContainerInvocation
                );
            });

            var injectorBuilderMethods = injectorModel.InjectionBuilderMethods.Select(method => {
                if (!builderRegistrations.TryGetValue(method.BuiltType.ToTypeDefinition(), out var builderMethodRegistration)) {
                    throw new InvalidOperationException($"No Builder found for type {method.BuiltType.QualifiedName}.");
                }

                var builderMethodContainerInvocation = new BuilderMethodContainerInvocationDefinition(
                    builderMethodRegistration.SpecificationType.Name + SpecificationContainerSuffix,
                    builderMethodRegistration.BuilderModel.Name
                );
                return new InjectorBuilderMethodDefinition(
                    method.BuiltType.ToTypeDefinition(),
                    method.Name,
                    builderMethodContainerInvocation
                );
            });

            var specContainerTypes = injectorModel.Specifications.Select(spec => {
                var specContainerName = spec.Name + SpecificationContainerSuffix;
                return (spec with { Name = specContainerName }).ToTypeDefinition();
            });

            return new InjectorDefinition(
                injectorModel.InjectorType.ToTypeDefinition(),
                injectorModel.InjectorInterface.ToTypeDefinition(),
                specContainerTypes,
                injectorMethods,
                injectorBuilderMethods);
        }
    }
}
