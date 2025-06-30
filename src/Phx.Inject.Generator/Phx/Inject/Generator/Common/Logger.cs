// -----------------------------------------------------------------------------
//  <copyright file="Logger.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Common {
    using Microsoft.CodeAnalysis;

    internal static class Logger {
        public static void Info(string message, Location? location = null) {
            Diagnostics.Log(message, null);
        }
    }
}
