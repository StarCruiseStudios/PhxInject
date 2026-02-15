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
///     Contract for pipeline segments that extract a single metadata element from compilation.
/// </summary>
/// <typeparam name="T">
///     The metadata element type produced by this pipeline. Must be immutable (ISourceCodeElement)
///     and structurally comparable (IEquatable&lt;T&gt;) for incremental compilation caching.
/// </typeparam>
/// <remarks>
///     Singleton pattern for exactly one metadata instance per compilation (e.g., assembly settings).
///     Returns <c>IResult&lt;T&gt;</c> for error handling without exceptions. Produces
///     <c>IncrementalValueProvider</c> (single value). For multiple instances use
///     <c>ISyntaxValuesPipeline</c> (plural). Implements two-phase: predicate filters syntax,
///     transform extracts semantics with full model access.
/// </remarks>
internal interface ISyntaxValuePipeline<T> where T : ISourceCodeElement, IEquatable<T> {
    /// <summary>
    ///     Registers a predicate-and-transform pair with Roslyn's incremental pipeline.
    /// </summary>
    /// <param name="syntaxProvider">
    ///     Roslyn's syntax provider that manages predicate filtering and transformation.
    /// </param>
    /// <returns>
    ///     An incremental value provider that lazily produces one Result&lt;T&gt; when the
    ///     pipeline executes. The Result contains either the extracted metadata or diagnostics.
    /// </returns>
    /// <remarks>
    ///     Calls <c>syntaxProvider.CreateSyntaxProvider</c> with predicate (fast syntax filter) and
    ///     transform (semantic extraction). Applies <c>.Collect()</c> or <c>.Select()</c> to
    ///     aggregate into single value. Roslyn caches both phases; unchanged results skip downstream.
    /// </remarks>
    IncrementalValueProvider<IResult<T>> Select(SyntaxValueProvider syntaxProvider);
}