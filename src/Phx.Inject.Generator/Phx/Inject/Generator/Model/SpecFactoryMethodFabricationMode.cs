// -----------------------------------------------------------------------------
//  <copyright file="SpecFactoryMethodFabricationMode.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model {
    // These values must match Phx.Inject.FabricationMode to parse correctly.
    internal enum SpecFactoryMethodFabricationMode {
        Scoped = 0,
        Recurrent = 1,
        Container = 2,
        ContainerScoped = 3
    }
}
