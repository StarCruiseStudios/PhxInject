// -----------------------------------------------------------------------------
//  <copyright file="Logger.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Common {
    using System;

    internal static class Logger {
        public static void Info(string message) {
            Console.Out.WriteLine(message);
        }

        public static void Error(string message) {
            Console.Error.WriteLine(message);
        }

        public static void Error(string message, Exception ex) {
            Console.Error.WriteLine(message);
            Console.Error.WriteLine(ex);
        }
    }
}
