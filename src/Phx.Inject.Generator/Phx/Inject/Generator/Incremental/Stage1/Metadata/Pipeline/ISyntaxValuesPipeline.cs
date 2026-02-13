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
///     <para>Collection Pattern for Multiple Declarations:</para>
///     <para>
///     Use ISyntaxValuesPipeline (plural) when multiple instances of the metadata type can coexist
///     in a compilation, such as:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Multiple injectors - each @Injector interface becomes one InjectorInterfaceMetadata
///             </description>
///         </item>
///         <item>
///             <description>
///             Multiple specifications - each @Specification class/interface is independent
///             </description>
///         </item>
///         <item>
///             <description>
///             Multiple factories, builders, auto-generation targets, etc.
///             </description>
///         </item>
///     </list>
///     
///     <para>vs. ISyntaxValuePipeline (Singular):</para>
///     <para>
///     For singleton metadata like assembly settings, use ISyntaxValuePipeline which produces
///     IncrementalValueProvider (single value). This plural version produces IncrementalValuesProvider
///     (stream of values), one per matching declaration.
///     </para>
///     
///     <para>Result Wrapping Per Element:</para>
///     <para>
///     Each element in the stream is wrapped in IResult&lt;T&gt;, allowing individual declarations
///     to fail independently. If extracting metadata for InjectorA succeeds but InjectorB has errors,
///     the stream contains Success(InjectorA) and Failure(diagnostics_for_B). Processing continues
///     for remaining elements.
///     </para>
///     
///     <para>Parallel Processing:</para>
///     <para>
///     Roslyn processes each element in the stream independently and potentially in parallel.
///     Predicate and transform must be thread-safe. The order of elements in the stream is undefined
///     (do not rely on source file order or declaration order).
///     </para>
///     
///     <para>Incremental Update Granularity:</para>
///     <para>
///     When a single declaration changes, only that element's transform re-executes. Other elements
///     remain cached. This fine-grained caching is why ISyntaxValuesPipeline is preferred over
///     ISyntaxValuePipeline.Collect() for collections - the latter would invalidate the entire
///     collection on any single change.
///     </para>
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
    ///     <para>Two-Phase Processing Per Element:</para>
    ///     <para>
    ///     Implementation typically calls syntaxProvider.CreateSyntaxProvider with:
    ///     </para>
    ///     <list type="number">
    ///         <item>
    ///             <term>Predicate:</term>
    ///             <description>
    ///             Fast syntax-only filter executed on every syntax node in parallel.
    ///             Returns true for nodes that might be relevant (e.g., classes with certain modifiers).
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>Transform:</term>
    ///             <description>
    ///             Semantic extraction executed only for nodes passing predicate.
    ///             Has full semantic model access for type resolution, attribute checking, etc.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     
    ///     <para>Caching and Invalidation:</para>
    ///     <para>
    ///     Each element in the stream is cached independently. Editing fileA.cs only invalidates
    ///     cache entries for declarations in fileA.cs. Declarations in fileB.cs remain cached.
    ///     This granular caching is essential for IDE responsiveness in large codebases.
    ///     </para>
    ///     
    ///     <para>Empty Stream Handling:</para>
    ///     <para>
    ///     If no declarations match the predicate, the stream is empty (not null, not a single
    ///     Result.Failure). Downstream consumers must handle empty streams appropriately.
    ///     </para>
    /// </remarks>
    IncrementalValuesProvider<IResult<T>> Select(SyntaxValueProvider syntaxProvider);
}