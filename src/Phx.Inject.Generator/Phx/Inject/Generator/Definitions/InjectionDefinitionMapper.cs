// -----------------------------------------------------------------------------
//  <copyright file="InjectionDefinitionMapper.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Definitions {
    internal class InjectionDefinitionMapper {
        private readonly CreateInjectionContextDefinition createInjectionContextDefinition;

        public InjectionDefinitionMapper(CreateInjectionContextDefinition createInjectionContextDefinition) {
            this.createInjectionContextDefinition = createInjectionContextDefinition;
        }

        public InjectionDefinitionMapper() : this(
            new InjectionContextDefinition.Builder(
                new InjectorDefinition.Builder().Build,
                new SpecContainerDefinition.Builder().Build,
                new ExternalDependencyImplementationDefinition.Builder().Build).Build) { }

        public InjectionContextDefinition Map(DefinitionGenerationContext context) {
            return createInjectionContextDefinition(context.Injector, context);
        }
    }
}
