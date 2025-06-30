// -----------------------------------------------------------------------------
//  <copyright file="InjectionDefMapper.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Map.Definitions;

namespace Phx.Inject.Generator.Map;

internal class InjectionDefMapper {
    private readonly InjectionContextDef.IBuilder injectionContextDefBuilder;

    public InjectionDefMapper(InjectionContextDef.IBuilder injectionContextDefBuilder) {
        this.injectionContextDefBuilder = injectionContextDefBuilder;
    }

    public InjectionDefMapper() : this(new InjectionContextDef.Builder()) { }

    public InjectionContextDef Map(DefGenerationContext context) {
        return injectionContextDefBuilder.Build(context.Injector, context);
    }
}
