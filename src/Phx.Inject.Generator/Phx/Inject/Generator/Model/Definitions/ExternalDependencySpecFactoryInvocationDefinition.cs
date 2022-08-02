// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencySpecFactoryInvocationDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Definitions {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Controller;

    internal delegate ExternalDependencySpecFactoryInvocationDefinition
            CreateExternalDependencySpecFactoryInvocationDefinition(
                    FactoryRegistration factoryRegistration,
                    Location location
            );

    internal record ExternalDependencySpecFactoryInvocationDefinition(
            TypeModel SpecType,
            string FactoryMethodName,
            Location Location
    ) : IDefinition {
        public class Builder {
            public ExternalDependencySpecFactoryInvocationDefinition Build(
                    FactoryRegistration factoryRegistration,
                    Location location
            ) {
                return new ExternalDependencySpecFactoryInvocationDefinition(
                        factoryRegistration.Specification.SpecType,
                        factoryRegistration.FactoryDescriptor.FactoryMethodName,
                        location);
            }
        }
    }
}
