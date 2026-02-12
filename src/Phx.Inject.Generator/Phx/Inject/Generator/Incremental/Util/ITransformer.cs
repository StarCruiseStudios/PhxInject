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
///     Defines a transformer that converts from one type to another, producing a result.
/// </summary>
/// <typeparam name="TIn"> The input type. </typeparam>
/// <typeparam name="TOut"> The output type. </typeparam>
internal interface ITransformer<TIn, TOut> where TOut : IEquatable<TOut>? {
    /// <summary>
    ///     Determines whether this transformer can transform the given input.
    /// </summary>
    /// <param name="input"> The input to check. </param>
    /// <returns> True if this transformer can handle the input. </returns>
    bool CanTransform(TIn input);
    
    /// <summary>
    ///     Transforms the input to an output result.
    /// </summary>
    /// <param name="input"> The input to transform. </param>
    /// <returns> A result containing the transformed output or diagnostics. </returns>
    IResult<TOut> Transform(TIn input);
}