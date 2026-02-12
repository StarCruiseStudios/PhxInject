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
///     <para><b>Singleton vs Collection Pattern:</b></para>
///     <para>
///     Use ISyntaxValuePipeline (singular) when exactly one instance of the metadata type should
///     exist per compilation, such as:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Assembly-level settings (PhxInjectSettingsMetadata) - configuration is global
///             </description>
///         </item>
///         <item>
///             <description>
///             Compilation-wide aggregations or summaries
///             </description>
///         </item>
///         <item>
///             <description>
///             Any metadata where multiple declarations would be an error
///             </description>
///         </item>
///     </list>
///     
///     <para><b>vs. ISyntaxValuesPipeline (Plural):</b></para>
///     <para>
///     Most DI declarations are plural - multiple injectors, specs, factories, etc. Use
///     ISyntaxValuesPipeline for those cases. This singular version produces IncrementalValueProvider
///     (single value) while plural produces IncrementalValuesProvider (stream of values).
///     </para>
///     
///     <para><b>Result Wrapping:</b></para>
///     <para>
///     Returns IResult&lt;T&gt; rather than T directly to enable error handling without exceptions.
///     If metadata extraction fails (malformed syntax, validation errors), the Result contains
///     diagnostics instead of a value. This allows the pipeline to continue processing other
///     segments even when one fails.
///     </para>
///     
///     <para><b>Incremental Compilation Contract:</b></para>
///     <para>
///     The IncrementalValueProvider returned is lazy - Select registers a computation but doesn't
///     execute it. Roslyn's incremental engine decides when to execute based on cache validity.
///     T must be immutable and implement structural equality to serve as an effective cache key.
///     </para>
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
    ///     <para><b>Two-Phase Processing:</b></para>
    ///     <para>
    ///     Implementation typically calls syntaxProvider.CreateSyntaxProvider with:
    ///     </para>
    ///     <list type="number">
    ///         <item>
    ///             <term>Predicate:</term>
    ///             <description>
    ///             Fast syntax-only filter (does this node look like it might be relevant?)
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>Transform:</term>
    ///             <description>
    ///             Semantic extraction (given this relevant node, extract full metadata)
    ///             </description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///     After creating the provider, typically applies .Collect() or .Select() to aggregate
    ///     into a single value (e.g., take first match, or error if multiple found).
    ///     </para>
    ///     
    ///     <para><b>Caching Behavior:</b></para>
    ///     <para>
    ///     Roslyn caches both predicate and transform results. If a syntax node is unchanged and
    ///     the semantic model is unchanged, neither phase re-executes. If syntax changes but
    ///     semantic result is equal (by T.Equals), downstream stages don't re-execute.
    ///     </para>
    /// </remarks>
    IncrementalValueProvider<IResult<T>> Select(SyntaxValueProvider syntaxProvider);
}