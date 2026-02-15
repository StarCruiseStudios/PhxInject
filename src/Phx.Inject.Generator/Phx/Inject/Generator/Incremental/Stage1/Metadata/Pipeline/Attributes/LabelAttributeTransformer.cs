// -----------------------------------------------------------------------------
// <copyright file="LabelAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms Label attribute data into metadata.
/// </summary>
/// <remarks>
///     <para>Purpose - Dependency Discrimination:</para>
///     <para>
///     [Label("name")] attributes provide string-based discrimination for dependencies of the same type.
///     When multiple dependencies share a type (e.g., multiple ILogger instances), labels distinguish
///     between them. The transformer extracts the label string from the attribute's constructor argument.
///     </para>
///     
///     <para>User Code Pattern - Labeled Dependencies:</para>
///     <code>
///     [Specification]
///     public interface ILoggers {
///         [Factory, Label("Application")]
///         ILogger CreateAppLogger();
///         
///         [Factory, Label("Performance")]
///         ILogger CreatePerfLogger();
///         
///         void ProcessData(
///             [Label("Application")] ILogger appLogger,
///             [Label("Performance")] ILogger perfLogger);
///     }
///     </code>
///     <para>
///     Generator matches parameters with labeled factory methods by comparing label strings.
///     Without labels, multiple ILogger parameters would be ambiguous.
///     </para>
///     
///     <para>String Extraction - GetConstructorArgument Pattern:</para>
///     <para>
///     The label string is extracted using:
///     `attributeData.GetConstructorArgument&lt;string&gt;(argument => argument.Kind != TypedConstantKind.Array)`
///     </para>
///     <para>
///     This pattern handles several edge cases:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Filters out array types (TypedConstantKind.Array) to ensure single string extraction
///             </description>
///         </item>
///         <item>
///             <description>
///             Works across different constructor signatures if attribute is extended
///             </description>
///         </item>
///         <item>
///             <description>
///             Returns null if no matching argument found (enforced by GetConstructorArgument's null handling)
///             </description>
///         </item>
///     </list>
///     <para>
///     The null-forgiving operator (!) is safe because LabelAttribute constructor requires a non-null
///     string parameter. If the attribute exists on the symbol, the argument must exist.
///     </para>
///     
///     <para>Why Labels vs Qualifiers:</para>
///     <para>
///     Labels use strings while QualifierAttribute uses types. Labels are preferred when:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Discrimination is conceptual rather than type-based ("Application" vs "Performance")
///             </description>
///         </item>
///         <item>
///             <description>
///             Creating marker types for every variation would clutter the codebase
///             </description>
///         </item>
///         <item>
///             <description>
///             String comparison is simpler than type comparison for dependency matching
///             </description>
///         </item>
///     </list>
///     
///     <para>Validation Constraints - Enforced by Later Stages:</para>
///     <para>
///     Transformer doesn't validate label contents. Later validation checks:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Label uniqueness within a specification (no duplicate labels for same type)
///             </description>
///         </item>
///         <item>
///             <description>
///             Label usage consistency (parameter labels must match factory labels)
///             </description>
///         </item>
///         <item>
///             <description>
///             String isn't empty/whitespace (meaningful discriminator)
///             </description>
///         </item>
///     </list>
///     
///     <para>Common Errors Prevented:</para>
///     <list type="bullet">
///         <item>
///             <term>Typo in label string:</term>
///             <description>
///             Validator catches mismatches between factory labels and parameter labels,
///             preventing silent failures where wrong dependency is injected.
///             </description>
///         </item>
///         <item>
///             <term>Missing label on parameter:</term>
///             <description>
///             When multiple labeled factories exist for a type, unlabeled parameters are flagged
///             as ambiguous.
///             </description>
///         </item>
///         <item>
///             <term>Duplicate labels:</term>
///             <description>
///             Two factories with same type and label create ambiguity, caught by validator.
///             </description>
///         </item>
///     </list>
/// </remarks>
internal sealed class LabelAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<LabelAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static LabelAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, LabelAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<LabelAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            LabelAttributeMetadata.AttributeClassName
        );

        var label = attributeData.GetConstructorArgument<string>(
            argument => argument.Kind != TypedConstantKind.Array
        )!;

        return new LabelAttributeMetadata(label, attributeMetadata).ToOkResult();
    }
}
