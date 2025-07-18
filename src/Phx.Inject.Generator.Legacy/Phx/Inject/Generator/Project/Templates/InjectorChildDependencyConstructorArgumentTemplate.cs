// -----------------------------------------------------------------------------
//  <copyright file="InjectorChildDependencyConstructorArgumentTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Generator.Project.Templates;

internal record InjectorChildDependencyConstructorArgumentTemplate(
    string ArgumentName,
    string DependencyImplementationTypeQualifiedName,
    string SpecContainerCollectionReferenceName,
    Location Location
) : IInjectorChildConstructorArgumentTemplate {
    public string OrderKey { get; } = "argument";
    public void Render(IRenderWriter writer, RenderContext renderCtx) {
        writer.Append(
            $"{ArgumentName}: new {DependencyImplementationTypeQualifiedName}({SpecContainerCollectionReferenceName})");
    }
}
