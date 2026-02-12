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

internal interface ISyntaxValuesPipeline<T> where T : ISourceCodeElement, IEquatable<T> {
    IncrementalValuesProvider<IResult<T>> Select(SyntaxValueProvider syntaxProvider);
}