// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyProviderMethodDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.External.Definitions {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Specifications.Definitions;

    internal record ExternalDependencyProviderMethodDefinition(
            TypeModel ProvidedType,
            string ProviderMethodName,
            SpecContainerFactoryInvocationDefinition SpecContainerFactoryInvocation,
            Location Location
    ) : IDefinition;
}
