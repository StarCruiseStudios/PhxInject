// -----------------------------------------------------------------------------
// <copyright file="InjectorChildConstructedSpecConstructorArgumentTemplate.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Generator.Project.Templates;

internal record InjectorChildConstructedSpecConstructorArgumentTemplate(
    string ArgumentName,
    string SpecParameterName,
    Location Location
) : IInjectorChildConstructorArgumentTemplate {
    public string OrderKey { get; } = "argument";
    public void Render(IRenderWriter writer, RenderContext renderCtx) {
        writer.Append($"{ArgumentName}: {SpecParameterName}");
    }
}
