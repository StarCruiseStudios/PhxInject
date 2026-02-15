// -----------------------------------------------------------------------------
// <copyright file="PartialAttributeTransformer.cs" company="Star Cruise Studios LLC">
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
///     Transforms Partial attribute data into metadata.
/// </summary>
/// <remarks>
///     Marker attribute enabling specification composition across multiple files/declarations.
///     Generator merges all <c>[Specification, Partial]</c> declarations of same type into single
///     implementation. Each partial declaration validated with <c>ExpectSingleAttribute</c> (prevents
///     duplicate <c>[Partial]</c> per declaration). All-or-nothing: all declarations must have
///     <c>[Partial]</c> or none.
/// </remarks>
///         </item>
///         <item>
///             <term>Duplicate [Specification] without [Partial]:</term>
///             <description>
///             Multiple [Specification] on same type without [Partial] is treated as error
///             (likely user copy-paste mistake).
///             </description>
///         </item>
///         <item>
///             <term>Method signature conflicts:</term>
///             <description>
///             Two partial declarations with methods having same name but different signatures
///             would cause C# compilation error. Validator catches this early with clear diagnostic.
///             </description>
///         </item>
///     </list>
/// </remarks>
internal sealed class PartialAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<PartialAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static PartialAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, PartialAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<PartialAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            PartialAttributeMetadata.AttributeClassName
        );
        
        return new PartialAttributeMetadata(attributeMetadata).ToOkResult();
    }
}
