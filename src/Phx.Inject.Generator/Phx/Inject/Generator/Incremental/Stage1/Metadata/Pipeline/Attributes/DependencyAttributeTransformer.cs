// -----------------------------------------------------------------------------
// <copyright file="DependencyAttributeTransformer.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms Dependency attribute data into metadata.
/// </summary>
/// <remarks>
///     <para>Purpose - External Dependency Declaration:</para>
///     <para>
///     [Dependency] marks injector constructor parameters that represent external dependencies
///     provided by parent injectors or application code. Unlike specifications (which this injector
///     provides), dependencies are required inputs that this injector consumes. The transformer
///     extracts and validates the dependency type.
///     </para>
///     
///     <para>Attribute Argument - Dependency Type:</para>
///     <para>
///     Single required argument: the interface type of the dependency. Extracted via
///     GetConstructorArgument with predicate filtering for non-array TypedConstant. WHY interface:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Dependencies are contracts, not implementations - interfaces define what we need, not how it's provided
///             </description>
///         </item>
///         <item>
///             <description>
///             Allows parent injectors to provide different implementations for different contexts (test vs prod)
///             </description>
///         </item>
///         <item>
///             <description>
///             Interfaces can be implemented by multiple providers, enabling composition of injector hierarchies
///             </description>
///         </item>
///     </list>
///     
///     <para>Validator Integration - WHY Validation During Transform:</para>
///     <para>
///     Unlike most transformers that defer all validation, DependencyAttributeTransformer validates
///     the extracted type symbol immediately via injected ICodeElementValidator. This is a deliberate
///     architectural exception:
///     </para>
///     <list type="bullet">
///         <item>
///             <term>Early feedback:</term>
///             <description>
///             Dependency type validation is simple (must be public/internal interface) and catches
///             90% of errors. Validating early prevents downstream transformers from seeing invalid
///             dependency types and reporting confusing secondary errors.
///             </description>
///         </item>
///         <item>
///             <term>Precise diagnostics:</term>
///             <description>
///             Error location points directly to [Dependency(typeof(IFoo))] attribute argument,
///             not to downstream usage sites. User sees error where they made the mistake.
///             </description>
///         </item>
///         <item>
///             <term>Fail-fast for broken references:</term>
///             <description>
///             If dependency type references a non-existent assembly or private type, we report it
///             immediately rather than attempting to generate code that won't compile.
///             </description>
///         </item>
///     </list>
///     
///     <para>Validation Rules - InterfaceElementValidator:</para>
///     <para>
///     The injected validator is configured as:
///     `InterfaceElementValidator.PublicInterface`
///     </para>
///     <para>This enforces:</para>
///     <list type="bullet">
///         <item>
///             <term>TypeKind == Interface:</term>
///             <description>
///             Dependency must be an interface type, not class/struct/enum. WHY: Interfaces are
///             implementation-agnostic contracts. Requiring concrete classes as dependencies couples
///             injector to specific implementations, defeating the purpose of dependency injection.
///             </description>
///         </item>
///         <item>
///             <term>Public or Internal accessibility:</term>
///             <description>
///             Generated injector code must be able to reference the dependency type. Private nested
///             interfaces cannot be used as constructor parameters across assembly boundaries.
///             Internal is allowed since generator outputs code in same assembly.
///             WHY: Prevents compile errors in generated code where dependency type is inaccessible.
///             </description>
///         </item>
///     </list>
///     
///     <para>Error Handling - Result.Error with Diagnostic:</para>
///     <para>
///     When validation fails, returns `Result.Error&lt;DependencyAttributeMetadata&gt;` with
///     DiagnosticInfo. The diagnostic captures:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>DiagnosticType.UnexpectedError (indicates generator precondition violation)</description>
///         </item>
///         <item>
///             <description>User-facing message explaining the constraint</description>
///         </item>
///         <item>
///             <description>LocationInfo pointing to the attribute application (targetSymbol.GetLocationOrDefault())</description>
///         </item>
///     </list>
///     <para>
///     This diagnostic flows through the pipeline and eventually becomes a compiler error in the
///     IDE, showing a squiggly underline under the [Dependency] attribute with the error message.
///     </para>
///     
///     <para>Type Model Conversion:</para>
///     <para>
///     Validated ITypeSymbol is converted to TypeModel via ToTypeModel() before storing in metadata.
///     This conversion is critical for incremental generator caching - TypeModel has structural
///     equality based on fully qualified name, while ITypeSymbol uses reference equality tied to
///     Compilation lifetime.
///     </para>
///     
///     <para>Performance - Validation Cost Trade-off:</para>
///     <para>
///     InterfaceElementValidator.IsValidSymbol checks TypeKind and Accessibility properties, both
///     cheap reads from symbol metadata. Cost is negligible compared to downstream transformations
///     that would fail anyway if dependency type is invalid. The early-fail saves work rather than
///     adding cost.
///     </para>
/// </remarks>
internal sealed class DependencyAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer,
    ICodeElementValidator dependencyTypeValidator
) : IAttributeTransformer<DependencyAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static DependencyAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance,
        DependencyAttributeMetadata.ElementValidator
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, DependencyAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<DependencyAttributeMetadata> Transform(ISymbol targetSymbol) {
            var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
                targetSymbol,
                DependencyAttributeMetadata.AttributeClassName
            );

            var dependencyTypeSymbol = attributeData
                .GetConstructorArgument<ITypeSymbol>(argument => argument.Kind != TypedConstantKind.Array);

            if (!dependencyTypeValidator.IsValidSymbol(dependencyTypeSymbol)) {
                return Result.Error<DependencyAttributeMetadata>(new DiagnosticInfo(
                    DiagnosticType.UnexpectedError,
                    "The specified dependency type is invalid.",
                    LocationInfo.CreateFrom(targetSymbol.GetLocationOrDefault())
                ));
            }
            
            return new DependencyAttributeMetadata(dependencyTypeSymbol.ToTypeModel(), attributeMetadata).ToOkResult();
    }
}
