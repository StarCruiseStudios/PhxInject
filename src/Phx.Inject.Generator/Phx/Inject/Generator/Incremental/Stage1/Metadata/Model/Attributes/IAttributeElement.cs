// -----------------------------------------------------------------------------
// <copyright file="IAttributeElement.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

/// <summary>
///     Common interface for all attribute metadata types in the Stage 1 metadata model.
/// </summary>
/// <remarks>
///     Provides a unified abstraction over all framework-specific attributes—<c>[Injector]</c>,
///     <c>[Factory]</c>, <c>[Builder]</c>, <c>[Dependency]</c>, etc.—enabling polymorphic
///     handling in the processing pipeline. All implementations are immutable records serving
///     as stable cache keys.
/// </remarks>
internal interface IAttributeElement : ISourceCodeElement {
    /// <summary>
    ///     Gets the common attribute metadata shared by all framework attributes.
    /// </summary>
    /// <remarks>
    ///     Provides access to generic attribute information (class name, target, locations)
    ///     without needing to know the specific attribute type.
    /// </remarks>
    AttributeMetadata AttributeMetadata { get; }
}