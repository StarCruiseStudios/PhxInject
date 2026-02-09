// -----------------------------------------------------------------------------
// <copyright file="StringBuilderUtil.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Text;

namespace Phx.Inject.Common.Util;

internal static class StringBuilderUtil {
    public static string BuildString(Action<StringBuilder> build) {
        var builder = new StringBuilder();
        build(builder);
        return builder.ToString();
    }
}