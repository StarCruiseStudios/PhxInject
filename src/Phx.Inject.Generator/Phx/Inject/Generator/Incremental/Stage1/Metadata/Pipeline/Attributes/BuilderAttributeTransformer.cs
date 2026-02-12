// -----------------------------------------------------------------------------
// <copyright file="BuilderAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms Builder attribute data into metadata.
/// </summary>
/// <remarks>
///     <para><b>Purpose - Builder Pattern Support:</b></para>
///     <para>
///     [Builder] marks methods that should generate builder pattern implementations. Unlike factories
///     which directly return constructed objects, builders return an intermediate builder object that
///     accumulates construction parameters before building the final object. The transformer extracts
///     no configuration arguments - [Builder] is purely a marker attribute.
///     </para>
///     
///     <para><b>Why Marker Attribute (No Arguments):</b></para>
///     <para>
///     Builder attribute has no constructor parameters or named arguments to extract. Its presence
///     alone signals the generation strategy. This design is intentional:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Builder behavior is inferred from method signature (return type defines builder interface)
///             </description>
///         </item>
///         <item>
///             <description>
///             No configuration is needed - builder pattern structure is standardized
///             </description>
///         </item>
///         <item>
///             <description>
///             Reduces cognitive load - no mode flags or options to learn
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Transform Implementation - Minimal Extraction:</b></para>
///     <para>
///     Transform method only extracts the base AttributeMetadata (location info for diagnostics).
///     No argument parsing needed. The BuilderAttributeMetadata constructor receives only the
///     base metadata, making this the simplest possible transformer implementation.
///     </para>
///     
///     <para><b>Comparison to Factory - Design Philosophy:</b></para>
///     <para>
///     Factory transformer extracts FabricationMode because factories can use different instantiation
///     strategies (constructor vs static method). Builders don't need this because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Builder pattern always produces a builder object first, then the builder builds the final type
///             </description>
///         </item>
///         <item>
///             <description>
///             The builder itself determines fabrication strategy through its Build() method
///             </description>
///         </item>
///         <item>
///             <description>
///             Builder configuration is inferred from builder interface shape, not attribute arguments
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Performance - Zero Allocation Beyond Base:</b></para>
///     <para>
///     Since no argument extraction occurs, this transformer has minimal CPU overhead. Only cost
///     is allocating BuilderAttributeMetadata record, which is unavoidable for incremental caching.
///     Fastest possible transformer implementation.
///     </para>
///     
///     <para><b>Extension Point - Future Configuration:</b></para>
///     <para>
///     If builder patterns later need configuration (e.g., whether to generate fluent builder vs
///     traditional builder), arguments would be added here. Current marker design allows backward-
///     compatible addition of optional constructor parameters defaulting to current behavior.
///     </para>
/// </remarks>
internal class BuilderAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<BuilderAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static BuilderAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, BuilderAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<BuilderAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            BuilderAttributeMetadata.AttributeClassName
        );
        
        return new BuilderAttributeMetadata(attributeMetadata).ToOkResult();
    }
}
