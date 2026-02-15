// -----------------------------------------------------------------------------
// <copyright file="ITransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Diagnostics;

#endregion

namespace Phx.Inject.Generator.Incremental.Util;

/// <summary>
///     Strategy pattern interface for converting between representation types in the generator pipeline.
/// </summary>
/// <typeparam name="TIn">
///     The input representation (typically Roslyn syntax nodes or symbols).
/// </typeparam>
/// <typeparam name="TOut">
///     The output representation (typically immutable metadata records).
///     Must be equatable for incremental generator caching.
/// </typeparam>
/// <remarks>
///     ## Architectural Role
///
///     Implements the Transformer pattern to decouple syntax analysis from metadata extraction.
///     Each transformer is responsible for one specific AST-to-metadata conversion, enabling
///     clear separation of concerns and testability.
///     
///     ## Two-Phase Protocol
///
///     1. CanTransform: Fast predicate check (typically O(1)) to filter candidates
///        without allocating result objects. Used by pipeline dispatchers to route work
///        to appropriate transformers.
///     2. Transform: Performs actual conversion and validation, producing a
///        <see cref="IResult{T}"/> that captures both success values and diagnostic errors.
///     
///     ## Error Handling Philosophy
///
///     Transformers NEVER throw exceptions for invalid user code. All validation errors are
///     captured as diagnostics in the returned <c>IResult</c>. This ensures the compiler can
///     report all errors in a file, not just the first one encountered.
///     
///     ## Immutability Requirement
///
///     <c>TOut</c> must be immutable and implement value equality. This is essential for Roslyn's
///     incremental caching to detect when results haven't changed and skip downstream processing.
///     
///     ## Thread Safety
///
///     Transformer instances are typically singleton and must be thread-safe. Roslyn may invoke
///     transformers concurrently for different syntax trees. Maintain no mutable state.
///     
///     ## Performance Considerations
///
///     <c>CanTransform</c> is invoked for every syntax node matching the pipeline's selector.
///     It must be extremely fast (sub-microsecond) to avoid degrading IDE responsiveness.
///     Prefer cheap checks like attribute presence or node kind over expensive semantic analysis.
/// </remarks>
internal interface ITransformer<TIn, TOut> where TOut : IEquatable<TOut>? {
    /// <summary>
    ///     Fast predicate to determine if this transformer handles the given input.
    /// </summary>
    /// <param name="input">The candidate input to check.</param>
    /// <returns>
    ///     <c>true</c> if this transformer can process the input; <c>false</c> to skip to next transformer.
    /// </returns>
    /// <remarks>
    ///     ## Performance Critical
    ///
    ///     Invoked frequently during pipeline execution. Must complete in sub-microsecond time.
    ///     Avoid allocations, expensive semantic analysis, or I/O.
    ///     
    ///     ## Typical Implementations
    ///
    ///     - Check for specific attribute presence via syntax scanning
    ///     - Test syntax node kind (e.g., is this a class declaration?)
    ///     - Verify symbol properties (e.g., is this interface public?)
    ///
    ///     Do NOT perform full transformation logic here. Leave detailed validation for
    ///     <see cref="Transform"/> which is only called when this returns true.
    /// </remarks>
    bool CanTransform(TIn input);
    
    /// <summary>
    ///     Transforms the input into an output metadata record, capturing any validation errors.
    /// </summary>
    /// <param name="input">The input to transform.</param>
    /// <returns>
    ///     A result containing either:
    ///
    ///     - Success: The transformed output value
    ///     - Error: A collection of diagnostic messages for user code issues
    /// </returns>
    /// <remarks>
    ///     ## Error Handling Contract
    ///
    ///     NEVER throw exceptions for malformed user code. Capture all validation failures
    ///     as diagnostics in the result. This allows the compiler to report all errors in a
    ///     file rather than failing fast on the first issue.
    ///     
    ///     ## Exception Cases
    ///
    ///     Only throw for internal generator bugs (e.g., unexpected Roslyn API behavior)
    ///     or for contract violations that indicate generator implementation errors.
    ///     
    ///     ## Immutability
    ///
    ///     The returned <c>TOut</c> value must be completely immutable. It may be cached
    ///     by Roslyn's incremental system and reused across compilation passes.
    ///     
    ///     ## Idempotence
    ///
    ///     Given the same input, must always produce the same output (modulo non-semantic
    ///     details wrapped in <see cref="GeneratorIgnored{T}"/>). Roslyn's cache validity
    ///     depends on this property.
    /// </remarks>
    IResult<TOut> Transform(TIn input);
}