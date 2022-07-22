// -----------------------------------------------------------------------------
//  <copyright file="FabricationMode.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject {
    /// <summary>
    ///     Enumerates the modes of fabrication that can be used by a factory
    ///     method.
    /// </summary>
    public enum FabricationMode {
        /// <summary>
        ///     Indicates that the factory method should only construct a single
        ///     instance within a given scope. Returning that first instance on
        ///     all invocations after the first.
        /// </summary>
        Scoped,

        /// <summary>
        ///     Indicates that the factory method should construct a new
        ///     instance each time it is invoked.
        /// </summary>
        Recurrent
    }
}
