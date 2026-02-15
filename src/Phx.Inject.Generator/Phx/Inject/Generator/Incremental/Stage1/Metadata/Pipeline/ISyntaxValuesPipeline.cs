// -----------------------------------------------------------------------------
// <copyright file="ISyntaxValuesPipeline.cs" company="Star Cruise Studios LLC">
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
///     Contract for pipeline segments that extract multiple metadata elements from compilation.
/// </summary>
/// <typeparam name="T">
///     The metadata element type produced by this pipeline. Must be immutable (ISourceCodeElement)
///     and structurally comparable (IEquatable&lt;T&gt;) for incremental compilation caching.
/// </typeparam>
/// <remarks>
///     Collection pattern for multiple instances per compilation (e.g., multiple injectors/specs).
///     Returns <c>IResult&lt;T&gt;</c> per element (independent failure). Produces
///     <c>IncrementalValuesProvider</c> (stream). For singleton metadata use
///     <c>ISyntaxValuePipeline</c>. Parallel processing per element, thread-safe required. Granular
///     caching: only changed declarations re-execute. Order undefined (don't rely on source order).
/// </remarks>
internal interface ISyntaxValuesPipeline<T> where T : ISourceCodeElement, IEquatable<T> {
    /// <summary>
    ///     Registers a predicate-and-transform pair with Roslyn's incremental pipeline.
    /// </summary>
    /// <param name="syntaxProvider">
    ///     Roslyn's syntax provider that manages parallel predicate filtering and transformation.
    /// </param>
    /// <returns>
    ///     An incremental values provider that lazily produces a stream of Result&lt;T&gt; values,
    ///     one per matching declaration. Each Result contains either extracted metadata or diagnostics.
    /// </returns>
    /// <remarks>
    ///     Calls <c>syntaxProvider.CreateSyntaxProvider</c> with predicate (fast syntax filter on all
    ///     nodes in parallel) and transform (semantic extraction on passing nodes only). Each element
    ///     cached independently (edit fileA doesn't invalidate fileB). Empty stream if no matches.
    /// </remarks>
    IncrementalValuesProvider<IResult<T>> Select(SyntaxValueProvider syntaxProvider);
}