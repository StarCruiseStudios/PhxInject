// -----------------------------------------------------------------------------
//  <copyright file="RenderSettings.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator {
    public record RenderSettings(
            int TabSize = 4,
            string GeneratedFileExtension = "generated.cs",
            bool NullableEnabled = true,
            bool ShouldWriteFiles = false,
            string OutputPath = "");
}
