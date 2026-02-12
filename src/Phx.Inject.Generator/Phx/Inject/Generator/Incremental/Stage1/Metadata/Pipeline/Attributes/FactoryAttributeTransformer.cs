// -----------------------------------------------------------------------------
// <copyright file="FactoryAttributeTransformer.cs" company="Star Cruise Studios LLC">
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
///     Transforms Factory attribute data into metadata.
/// </summary>
/// <remarks>
///     <para><b>Purpose - Factory Method Configuration:</b></para>
///     <para>
///     [Factory] marks methods that should generate factory implementations for creating objects.
///     Factory methods define the signature of what to create (return type) and what dependencies
///     are needed (parameters). The transformer extracts configuration controlling how the factory
///     creates instances.
///     </para>
///     
///     <para><b>Attribute Argument - FabricationMode:</b></para>
///     <para>
///     FabricationMode enum controls the instantiation strategy:
///     </para>
///     <list type="bullet">
///         <item>
///             <term>Default (0):</term>
///             <description>
///             Use generator's default behavior based on type characteristics (sealed class = direct
///             instantiation, abstract/interface = error requiring explicit mode).
///             WHY: Sensible defaults reduce annotation burden for common cases.
///             </description>
///         </item>
///         <item>
///             <term>Constructor:</term>
///             <description>
///             Generate code calling the type's constructor. Used for concrete classes where we
///             control instantiation. WHY: Most efficient path, direct instantiation without
///             indirection or reflection.
///             </description>
///         </item>
///         <item>
///             <term>StaticMethod:</term>
///             <description>
///             Generate code calling a static factory method on the type. Used when type has complex
///             initialization logic encapsulated in a factory method.
///             WHY: Respects encapsulation when constructor is private/internal or has validation logic.
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Enum Extraction - TypedConstant Type Filtering:</b></para>
///     <para>
///     FabricationMode extraction uses a predicate to find the correct constructor argument:
///     `argument.Type!.GetFullyQualifiedName() == FabricationModeClassName`. This type-based
///     filtering is necessary because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Constructor arguments don't include parameter names, only types and values
///             </description>
///         </item>
///         <item>
///             <description>
///             Attribute may have multiple constructor overloads with different parameter orders
///             </description>
///         </item>
///         <item>
///             <description>
///             Enum type check is more reliable than position-based extraction across versions
///             </description>
///         </item>
///     </list>
///     <para>
///     The predicate checks the TypedConstant.Type (the parameter's type) against the expected
///     fully qualified enum name. This handles cases where the enum value was passed as an integer
///     literal but should be interpreted as the enum type.
///     </para>
///     
///     <para><b>Default Value Handling:</b></para>
///     <para>
///     GetConstructorArgument falls back to `default(FabricationMode)` if not found. This is safe
///     because FabricationMode.Default (0) is explicitly designed as the fallback behavior.
///     Validators later decide if Default is acceptable for the target type.
///     </para>
///     
///     <para><b>Why No Validation of FabricationMode Value:</b></para>
///     <para>
///     Transformer doesn't validate if the mode is appropriate for the target type (e.g., checking
///     if Constructor mode is used on an abstract class). This is deferred to validators because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Validation requires analyzing target symbol's type characteristics (abstract, sealed, etc.)
///             </description>
///         </item>
///         <item>
///             <description>
///             Validators have complete context about the specification method and its return type
///             </description>
///         </item>
///         <item>
///             <description>
///             Separates "what was requested" (transformer) from "is request valid" (validator)
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Performance - Minimal Allocation:</b></para>
///     <para>
///     Factory metadata contains only a single enum field plus base metadata. Extremely lightweight
///     allocation that incremental caching can efficiently compare. Enum comparison is pointer-sized
///     integer comparison - negligible cost in equality checks.
///     </para>
/// </remarks>
internal class FactoryAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<FactoryAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static FactoryAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    private const string FabricationModeClassName = $"{NamespaceName}.{nameof(FabricationMode)}";

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, FactoryAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<FactoryAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            FactoryAttributeMetadata.AttributeClassName
        );

        var fabricationMode =
            attributeData.GetNamedArgument<FabricationMode?>(nameof(FactoryAttribute.FabricationMode))
            ?? attributeData.GetConstructorArgument<FabricationMode>(argument =>
                argument.Type!.GetFullyQualifiedName() == FabricationModeClassName,
                default);

        return new FactoryAttributeMetadata(fabricationMode, attributeMetadata).ToOkResult();
    }
}
