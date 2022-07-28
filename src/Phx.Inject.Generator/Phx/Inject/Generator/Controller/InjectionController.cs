// -----------------------------------------------------------------------------
//  <copyright file="InjectionController.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Controller {
    using Phx.Inject.Generator.Model.Definitions;

    internal class InjectionController {
        public InjectionController() {
            var injectorManager = new InjectorDefinition.Builder(
                    createInjectorProviderMethod: null!,
                    createInjectorBuilderMethod: null!,
                    createSpecContainerCollection: null!);

            var injectionContextManager = new InjectionContextDefinition.Builder(
                    injectorManager.CreateInjectorDefinition,
                    createSpecContainer: null!);
        }
    }
}
