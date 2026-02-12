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
///     <para>Purpose - Deferred Fabrication Control:</para>
///     <para>
///     [FactoryReference] marks parameters that should receive a factory delegate rather than a resolved
///     instance. This enables lazy initialization, multiple instance creation, or caller-controlled
///     fabrication timing. The transformer extracts optional FabricationMode that controls how the
///     referenced factory creates instances.
///     </para>
///     
///     <para>User Code Pattern - Factory Delegation:</para>
///     <code>
///     [Specification]
///     public interface IServices {
///         [Factory] IUserService CreateUserService();
///         
///         // Processor receives factory delegate, not instance
///         [Factory]
///         IProcessor CreateProcessor(
///             [FactoryReference] Func&lt;IUserService&gt; userServiceFactory);
///     }
///     
///     // Generated implementation:
///     public IProcessor CreateProcessor() {
///         return new ProcessorImpl(() => CreateUserService());
///     }
///     </code>
///     <para>
///     Processor can call userServiceFactory() multiple times to create new instances, or defer
///     creation until needed (lazy initialization).
///     </para>
///     
///     <para>User Code Pattern - FabricationMode Override:</para>
///     <code>
///     [Specification]
///     public interface IServices {
///         // Default: creates new instance each call
///         [Factory] IUserService CreateUserService();
///         
///         // Force recurrent mode even if CreateUserService is scoped
///         [Factory]
///         IProcessor CreateProcessor(
///             [FactoryReference(FabricationMode.Recurrent)]
///             Func&lt;IUserService&gt; userServiceFactory);
///     }
///     </code>
///     <para>
///     FabricationMode argument overrides the factory's default behavior, giving caller explicit
///     control over instantiation strategy.
///     </para>
///     
///     <para>FabricationMode Extraction - Dual Source Pattern:</para>
///     <para>
///     FabricationMode can be specified via constructor argument or named argument:
///     </para>
///     <list type="number">
///         <item>
///             <term>Named Argument (Preferred):</term>
///             <description>
///             `GetNamedArgument&lt;FabricationMode?&gt;(nameof(FactoryReferenceAttribute.FabricationMode))`
///             extracts explicitly named property. Returns null if not specified.
///             </description>
///         </item>
///         <item>
///             <term>Constructor Argument (Fallback):</term>
///             <description>
///             `GetConstructorArgument&lt;FabricationMode&gt;(argument => argument.Type!.GetFullyQualifiedName() == FabricationModeClassName, default)`
///             finds enum by type filtering. Returns default(FabricationMode) if not found.
///             </description>
///         </item>
///     </list>
///     <para>
///     Null-coalescing operator (??) tries named first, then constructor, providing flexibility
///     for different attribute usage patterns.
///     </para>
///     
///     <para>Type-Based Enum Extraction - Why Predicate Filtering:</para>
///     <para>
///     The predicate `argument.Type!.GetFullyQualifiedName() == FabricationModeClassName` filters
///     constructor arguments by type rather than position because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Attribute may have multiple constructor overloads with parameters in different orders
///             </description>
///         </item>
///         <item>
///             <description>
///             Position-based extraction breaks if new constructor overloads are added
///             </description>
///         </item>
///         <item>
///             <description>
///             Type-based extraction is version-resilient (works across attribute signature changes)
///             </description>
///         </item>
///         <item>
///             <description>
///             FabricationMode is the only enum parameter, making type-based extraction unambiguous
///             </description>
///         </item>
///     </list>
///     
///     <para>Why FactoryReference Needs Special Handling - Delegate vs Instance:</para>
///     <para>
///     FactoryReference requires special code generation because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Normal parameters receive resolved instances (IUserService userService)
///             </description>
///         </item>
///         <item>
///             <description>
///             [FactoryReference] parameters receive factory delegates (Func&lt;IUserService&gt; factory)
///             </description>
///         </item>
///         <item>
///             <description>
///             Generated code must wrap factory method in delegate: () => CreateUserService()
///             </description>
///         </item>
///         <item>
///             <description>
///             FabricationMode affects closure implementation (scoped requires cache, recurrent doesn't)
///             </description>
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
///         <item>
///             <description>
///             [FactoryReference] isn't combined with [BuilderReference] on same parameter
///             </description>
///         </item>
///     </list>
///     
///     <para>Common Errors Prevented:</para>
///     <list type="bullet">
///         <item>
///             <term>Wrong delegate signature:</term>
///             <description>
///             Using Func&lt;IUserService&gt; to reference factory returning IAdminService causes type
///             mismatch. Validator ensures delegate type matches factory return type.
///             </description>
///         </item>
///         <item>
///             <term>Missing factory:</term>
///             <description>
///             [FactoryReference] on parameter without corresponding factory method is unresolvable.
///             Validator requires factory existence.
///             </description>
///         </item>
///         <item>
///             <term>Incompatible FabricationMode:</term>
///             <description>
///             Requesting Constructor mode when factory uses StaticMethod is nonsensical.
///             Validator checks mode compatibility.
///             </description>
///         </item>
///         <item>
///             <term>Circular factory dependencies:</term>
///             <description>
///             Factory A references Factory B, Factory B references Factory A. Validator detects
///             dependency cycles that would cause infinite recursion.
///             </description>
///         </item>
///     </list>
/// </remarks>
internal class FactoryReferenceAttributeTransformer(
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
