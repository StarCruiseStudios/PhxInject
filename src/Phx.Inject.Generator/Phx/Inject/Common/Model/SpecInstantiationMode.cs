// -----------------------------------------------------------------------------
//  <copyright file="SpecInstantiationMode.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Common.Model;

internal enum SpecInstantiationMode {
    Static = 0,
    Instantiated = 1,
    Dependency = 2
}
