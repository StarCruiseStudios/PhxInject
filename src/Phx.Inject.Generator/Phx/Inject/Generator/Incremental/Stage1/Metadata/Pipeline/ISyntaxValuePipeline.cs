// -----------------------------------------------------------------------------
// <copyright file="ISyntaxValuePipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline;

/// <summary>
///     Defines a pipeline that selects a single syntax value from source code.
/// </summary>
/// <typeparam name="T">The type of source code element to select.</typeparam>
internal interface ISyntaxValuePipeline<T> where T : ISourceCodeElement, IEquatable<T> {
    /// <summary>
    ///     Selects a single incremental value from the syntax provider.
    /// </summary>
    /// <param name="syntaxProvider">The syntax value provider to select from.</param>
    /// <returns>An incremental value provider containing the result.</returns>
    IncrementalValueProvider<IResult<T>> Select(SyntaxValueProvider syntaxProvider);
}