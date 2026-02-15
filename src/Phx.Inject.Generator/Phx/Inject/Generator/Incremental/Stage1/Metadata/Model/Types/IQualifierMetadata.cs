// -----------------------------------------------------------------------------
// <copyright file="IQualifierMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;

/// <summary>
///     Marker interface for dependency injection qualifiers that distinguish multiple bindings
///     of the same type.
/// </summary>
/// <remarks>
///     Implementations: <see cref="NoQualifierMetadata"/> (no qualifier),
///     <see cref="LabelQualifierMetadata"/> (string-based), <see cref="CustomQualifierMetadata"/>
///     (user-defined attribute). All implementations must exclude Location from equality.
/// </remarks>
internal interface IQualifierMetadata : ISourceCodeElement, IEquatable<IQualifierMetadata> { }