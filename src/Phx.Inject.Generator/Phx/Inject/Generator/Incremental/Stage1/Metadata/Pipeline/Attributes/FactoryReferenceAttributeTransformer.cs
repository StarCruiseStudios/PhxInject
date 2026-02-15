// -----------------------------------------------------------------------------
// <copyright file="FactoryReferenceAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Util;
using static Phx.Inject.Generator.Incremental.PhxInject;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms FactoryReference attribute data into metadata.
/// </summary>
/// <remarks>
///     Extracts optional <c>FabricationMode</c> for parameters receiving factory delegates instead
///     of resolved instances (enables lazy initialization, multiple creation, caller-controlled
///     timing). Tries named argument first, then constructor argument filtered by
///     <c>FabricationModeClassName</c> type for version resilience across signature changes.
/// </remarks>
///         </item>
///     </list>
///     
///     <para>FabricationMode Interpretation - Default vs Explicit:</para>
///     <list type="bullet">
///         <item>
///             <term>Default (0 or unspecified):</term>
///             <description>
///             Use the referenced factory's natural fabrication mode. If CreateUserService is scoped,
///             factory reference respects that scoping.
///             </description>
///         </item>
///         <item>
///             <term>Recurrent:</term>
///             <description>
///             Force new instance on every delegate invocation, even if factory is scoped.
///             Generated: () => CreateNewUserService()
///             </description>
///         </item>
///         <item>
///             <term>Scoped:</term>
///             <description>
///             Cache first instance, return same on subsequent delegate calls.
///             Generated: Lazy initialization with scope-local cache.
///             </description>
///         </item>
///         <item>
///             <term>Container/ContainerScoped:</term>
///             <description>
///             Container-level caching for child injector scenarios.
///             </description>
///         </item>
///     </list>
///     
///     <para>Validation Constraints - Enforced by Later Stages:</para>
///     <para>
///     Transformer doesn't validate reference semantics. Later validation ensures:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Parameter type is Func&lt;T&gt; or compatible delegate matching factory return type
///             </description>
///         </item>
///         <item>
///             <description>
///             Referenced factory actually exists and is accessible
///             </description>
///         </item>
///         <item>
///             <description>
///             FabricationMode is compatible with factory's declaration (can't force Constructor mode
///             on StaticMethod factory)
///             </description>
///         </item>
///     </list>
/// </remarks>
internal sealed class FactoryReferenceAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<FactoryReferenceAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static FactoryReferenceAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    private const string FabricationModeClassName = $"{NamespaceName}.{nameof(FabricationMode)}";

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, FactoryReferenceAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<FactoryReferenceAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            FactoryReferenceAttributeMetadata.AttributeClassName
        );

        var fabricationMode =
            attributeData.GetNamedArgument<FabricationMode?>(nameof(FactoryReferenceAttribute.FabricationMode))
            ?? attributeData.GetConstructorArgument<FabricationMode>(argument =>
                argument.Type!.GetFullyQualifiedName() == FabricationModeClassName,
                default);

        return new FactoryReferenceAttributeMetadata(fabricationMode, attributeMetadata).ToOkResult();
    }
}
