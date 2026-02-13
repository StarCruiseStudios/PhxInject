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
///     <para>Role in DI Framework:</para>
///     <para>
///     Provides a unified abstraction over all framework-specific attributes ([Injector], [Factory],
///     [Builder], [Dependency], etc.) that have been analyzed during Stage 1 metadata extraction.
///     This enables polymorphic handling of different attribute types in the processing pipeline.
///     </para>
///     
///     <para>What User Declarations Represent:</para>
///     <para>
///     Each implementation corresponds to a specific attribute users apply to configure the DI framework:
///     configuration markers (Injector, Specification), binding definitions (Factory, Builder),
///     explicit dependencies (Dependency), or qualifier disambiguation (Label, Qualifier).
///     </para>
///     
///     <para>Design Pattern:</para>
///     <para>
///     Uses a tagged union pattern where the AttributeMetadata property provides common base data
///     (attribute class name, target name, locations) while derived types add attribute-specific
///     properties (e.g., FabricationMode for Factory, Label string for Label attribute).
///     </para>
///     
///     <para>Immutability Requirements:</para>
///     <para>
///     All implementations must be immutable records to serve as stable cache keys in incremental
///     compilation. The base AttributeMetadata follows the same Location-exclusion pattern as
///     TypeMetadata, ensuring that whitespace-only changes don't invalidate cached metadata.
///     </para>
///     
///     <para>Relationship to Other Models:</para>
///     <list type="bullet">
///         <item>
///             <description>
///             Extends ISourceCodeElement to participate in the unified metadata hierarchy
///             </description>
///         </item>
///         <item>
///             <description>
///             Contains TypeMetadata for type-parameterized attributes (Dependency, Qualifier)
///             </description>
///         </item>
///         <item>
///             <description>
///             Used by higher-level models (InjectorModel, FactoryModel) that aggregate attributes
///             </description>
///         </item>
///     </list>
/// </remarks>
internal interface IAttributeElement : ISourceCodeElement {
    /// <summary>
    ///     Gets the common attribute metadata shared by all framework attributes.
    /// </summary>
    /// <remarks>
    ///     Provides access to the generic attribute information (class name, target, locations)
    ///     without needing to know the specific attribute type. This enables shared processing
    ///     logic for diagnostics, validation, and metadata aggregation across all attribute types.
    /// </remarks>
    AttributeMetadata AttributeMetadata { get; }
}