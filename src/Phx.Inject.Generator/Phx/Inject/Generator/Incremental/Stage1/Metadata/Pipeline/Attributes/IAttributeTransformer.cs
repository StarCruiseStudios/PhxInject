// -----------------------------------------------------------------------------
// <copyright file="IAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

internal interface IAttributeTransformer<out TAttributeMetadata> where TAttributeMetadata : IAttributeElement, IEquatable<TAttributeMetadata> {
    IResult<TAttributeMetadata> Transform(ISymbol targetSymbol);
}