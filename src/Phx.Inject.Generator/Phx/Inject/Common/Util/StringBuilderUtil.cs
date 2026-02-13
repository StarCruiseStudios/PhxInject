// -----------------------------------------------------------------------------
// <copyright file="StringBuilderUtil.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using System.Text;

#endregion

namespace Phx.Inject.Common.Util;

/// <summary>
///     Utility methods for working with <see cref="StringBuilder"/>.
/// </summary>
internal static class StringBuilderUtil {
    /// <summary>
    ///     Builds a string using a <see cref="StringBuilder"/> and an action.
    /// </summary>
    /// <param name="build"> The action that builds the string. </param>
    /// <returns> The resulting string. </returns>
    public static string BuildString(Action<StringBuilder> build) {
        var builder = new StringBuilder();
        build(builder);
        return builder.ToString();
    }
}