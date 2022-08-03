// -----------------------------------------------------------------------------
//  <copyright file="DefinitionGenerationContext.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model {
    using System.Collections.Generic;
    using Phx.Inject.Generator.Controller;

    internal interface IDefinitionGenerationContext {
        public IDictionary<RegistrationIdentifier, FactoryRegistration> FactoryRegistrations { get; }
        public TypeModel InjectorType { get; }
        public TypeModel SpecContainerCollectionType { get; }
    }
}
