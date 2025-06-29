// -----------------------------------------------------------------------------
//  <copyright file="InjectionDefinitionMapper.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Definitions {
    internal class InjectionDefinitionMapper {
        private readonly InjectionContextDefinition.IBuilder injectionContextDefinitionBuilder;

        public InjectionDefinitionMapper(InjectionContextDefinition.IBuilder injectionContextDefinitionBuilder) {
            this.injectionContextDefinitionBuilder = injectionContextDefinitionBuilder;
        }

        public InjectionDefinitionMapper() : this(new InjectionContextDefinition.Builder()) { }

        public InjectionContextDefinition Map(DefinitionGenerationContext context) {
            return injectionContextDefinitionBuilder.Build(context.Injector, context);
        }
    }
}
