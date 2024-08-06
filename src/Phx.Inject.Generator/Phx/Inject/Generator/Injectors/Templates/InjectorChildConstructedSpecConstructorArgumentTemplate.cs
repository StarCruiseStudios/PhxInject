// -----------------------------------------------------------------------------
//  <copyright file="InjectorChildConstructedSpecConstructorArgumentTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Injectors.Templates {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common.Templates;

    internal record InjectorChildConstructedSpecConstructorArgumentTemplate(
        string ArgumentName,
        string SpecParameterName,
        Location Location
    ) : IInjectorChildConstructorArgumentTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append($"{ArgumentName}: {SpecParameterName}");
        }
    }
}
