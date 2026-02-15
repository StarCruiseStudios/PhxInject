// -----------------------------------------------------------------------------
// <copyright file="SpecificationAttributeTransformer.cs" company="Star Cruise Studios LLC">
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
///     Transforms Specification attribute data into metadata.
/// </summary>
/// <remarks>
///     <para>Purpose - Specification Container Marking:</para>
///     <para>
///     [Specification] marks interfaces or abstract classes as containers for factory method
///     declarations. This attribute has no arguments - it's a marker attribute that signals
///     "this type contains specification methods that the generator should process."
///     The transformer simply detects its presence.
///     </para>
///     
///     <para>User Code Pattern - Specification Interface:</para>
///     <code>
///     [Specification]
///     public interface IUserServices {
///         [Factory] IUserService CreateUserService();
///         [Builder] IUserBuilder CreateBuilder();
///     }
///     </code>
///     <para>
///     When detected, generator creates an implementation class containing all factory/builder logic
///     defined by the specification methods within.
///     </para>
///     
///     <para>Why No Arguments - Simple Detection:</para>
///     <para>
///     Specification attribute has no configuration because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             The specification's purpose is entirely determined by its methods (Factory, Builder, etc.)
///             </description>
///         </item>
///         <item>
///             <description>
///             All configuration is method-level (fabrication modes, scopes, qualifiers)
///             </description>
///         </item>
///         <item>
///             <description>
///             Reduces cognitive load - attribute presence is the only signal needed
///             </description>
///         </item>
///     </list>
///     
///     <para>ExpectSingleAttribute - Preventing Duplicate Declarations:</para>
///     <para>
///     Uses ExpectSingleAttribute rather than allowing multiple instances because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Having [Specification] multiple times on same type is meaningless (no configuration to vary)
///             </description>
///         </item>
///         <item>
///             <description>
///             Likely indicates user error or confusion about attribute purpose
///             </description>
///         </item>
///         <item>
///             <description>
///             Clear diagnostic prevents confusion about generator behavior
///             </description>
///         </item>
///     </list>
///     
///     <para>Common Errors Prevented:</para>
///     <list type="bullet">
///         <item>
///             <term>Multiple [Specification] on same type:</term>
///             <description>
///             ExpectSingleAttribute rejects this, preventing confusion about whether multiple
///             implementations would be generated.
///             </description>
///         </item>
///         <item>
///             <term>Forgetting [Specification] on container:</term>
///             <description>
///             Generator ignores types without this marker, preventing accidental processing of
///             unrelated interfaces.
///             </description>
///         </item>
///     </list>
/// </remarks>
internal sealed class SpecificationAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<SpecificationAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static SpecificationAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, SpecificationAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<SpecificationAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            SpecificationAttributeMetadata.AttributeClassName
        );
        
        return new SpecificationAttributeMetadata(attributeMetadata).ToOkResult();
    }
}
