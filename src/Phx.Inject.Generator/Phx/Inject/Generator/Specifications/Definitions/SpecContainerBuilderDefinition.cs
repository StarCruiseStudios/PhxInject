// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerBuilderDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Specifications.Definitions {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Definitions;

    internal record SpecContainerBuilderDefinition(
            TypeModel BuiltType,
            string MethodName,
            IEnumerable<SpecContainerFactoryInvocationDefinition> Arguments,
            Location Location
    ) : IDefinition;
}
