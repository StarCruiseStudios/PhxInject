// -----------------------------------------------------------------------------
// <copyright file="ChildInjectorAttributeTransformer.cs" company="Star Cruise Studios LLC">
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
///     Transforms ChildInjector attribute data into metadata.
/// </summary>
/// <remarks>
///     Marker attribute identifying child injector specifications that inherit parent dependencies.
///     Generated child implementations accept parent injector in constructor and walk dependency
///     chain (child → parent → grandparent). Parent-child relationship determined by factory method
///     return types. Mutually exclusive with <c>[Injector]</c> (root injectors have no parent).
/// </remarks>
///         <item>
///             <term>Missing parent factory:</term>
///             <description>
///             Child injector without any parent factory method is orphaned. Validator ensures
///             every child has exactly one parent factory.
///             </description>
///         </item>
///         <item>
///             <term>Multiple parents:</term>
///             <description>
///             Child injector returned by multiple factories is ambiguous (which parent owns it?).
///             Validator requires single parent factory.
///             </description>
///         </item>
///         <item>
///             <term>Circular hierarchy:</term>
///             <description>
///             Parent creates child, child creates original parent causes infinite recursion.
///             Validator detects cycles in injector hierarchy.
///             </description>
///         </item>
///         <item>
///             <term>Using both [Injector] and [ChildInjector]:</term>
///             <description>
///             Mutually exclusive attributes. Specification is either root or child, not both.
///             </description>
///         </item>
///     </list>
/// </remarks>
internal sealed class ChildInjectorAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<ChildInjectorAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static ChildInjectorAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, ChildInjectorAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<ChildInjectorAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            ChildInjectorAttributeMetadata.AttributeClassName
        );
        
        return new ChildInjectorAttributeMetadata(attributeMetadata).ToOkResult();
    }
}
