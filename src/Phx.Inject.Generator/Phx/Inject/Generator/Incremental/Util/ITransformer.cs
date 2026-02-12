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

internal interface ITransformer<TIn, TOut> where TOut : IEquatable<TOut>? {
    bool CanTransform(TIn input);
    IResult<TOut> Transform(TIn input);
}