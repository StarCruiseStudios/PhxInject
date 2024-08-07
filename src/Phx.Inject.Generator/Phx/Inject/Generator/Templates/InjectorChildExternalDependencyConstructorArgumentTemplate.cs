// -----------------------------------------------------------------------------
//  <copyright file="InjectorChildExternalDependencyConstructorArgumentTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Templates {
    using Microsoft.CodeAnalysis;

    internal record InjectorChildExternalDependencyConstructorArgumentTemplate(
        string ArgumentName,
        string ExternalDependencyImplementationTypeQualifiedName,
        string SpecContainerCollectionReferenceName,
        Location Location
    ) : IInjectorChildConstructorArgumentTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append(
                $"{ArgumentName}: new {ExternalDependencyImplementationTypeQualifiedName}({SpecContainerCollectionReferenceName})");
        }
    }
}
