// -----------------------------------------------------------------------------
//  <copyright file="TemplateGenerationContext.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.External.Definitions;
    using Phx.Inject.Generator.Model.Injectors.Definitions;
    using Phx.Inject.Generator.Model.Specifications.Definitions;

    internal record TemplateGenerationContext(
            IReadOnlyDictionary<TypeModel, InjectorDefinition> Injectors,
            IReadOnlyDictionary<TypeModel, SpecContainerDefinition> SpecContainers,
            IReadOnlyDictionary<TypeModel, ExternalDependencyImplementationDefinition> ExternalDependencyImplementations,
            GeneratorExecutionContext GenerationContext
    );
}
