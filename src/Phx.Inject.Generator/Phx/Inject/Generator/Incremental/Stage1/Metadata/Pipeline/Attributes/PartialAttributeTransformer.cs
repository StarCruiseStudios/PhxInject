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
///     <para><b>Purpose - Partial Specification Composition:</b></para>
///     <para>
///     [Partial] marks specifications that should be composed together into a single generated implementation.
///     Multiple partial specifications can declare different factory methods, and the generator merges them
///     into one implementation class. This enables splitting large specifications across multiple files
///     or organizing specifications by concern.
///     </para>
///     
///     <para><b>User Code Pattern - Split Specifications:</b></para>
///     <code>
///     // File: IUserServices.Factories.cs
///     [Specification, Partial]
///     public partial interface IUserServices {
///         [Factory] IUserService CreateUserService();
///         [Factory] IUserRepository CreateUserRepository();
///     }
///     
///     // File: IUserServices.Builders.cs
///     [Specification, Partial]
///     public partial interface IUserServices {
///         [Builder] IUserBuilder CreateUserBuilder();
///     }
///     </code>
///     <para>
///     Generator creates a single IUserServicesImpl class containing all methods from both partial
///     declarations. Without [Partial], multiple [Specification] on same type would error.
///     </para>
///     
///     <para><b>Why Partial Needs Special Handling - Type Symbol Merging:</b></para>
///     <para>
///     Partial specifications require special handling because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Roslyn provides separate ITypeSymbol instances for each partial declaration
///             </description>
///         </item>
///         <item>
///             <description>
///             Generator must merge methods from all partial declarations into single implementation
///             </description>
///         </item>
///         <item>
///             <description>
///             Attribute presence signals "merge these" vs "these are duplicates (error)"
///             </description>
///         </item>
///         <item>
///             <description>
///             Generated implementation class must be partial to allow user-written partial class extensions
///             </description>
///         </item>
///     </list>
///     
///     <para><b>No Arguments - Marker Attribute Pattern:</b></para>
///     <para>
///     Like SpecificationAttribute, PartialAttribute has no arguments because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Partial behavior is binary: either merge declarations or don't
///             </description>
///         </item>
///         <item>
///             <description>
///             All configuration is at method level (no partial-specific settings)
///             </description>
///         </item>
///         <item>
///             <description>
///             Mirrors C#'s "partial" keyword semantics (no configuration, just declaration)
///             </description>
///         </item>
///     </list>
///     
///     <para><b>ExpectSingleAttribute Per Declaration - Not Per Type:</b></para>
///     <para>
///     ExpectSingleAttribute validates each partial declaration has at most one [Partial] attribute:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Each partial file can have [Specification, Partial] once
///             </description>
///         </item>
///         <item>
///             <description>
///             Having [Partial] multiple times on same declaration is meaningless (no configuration)
///             </description>
///         </item>
///         <item>
///             <description>
///             Different partial declarations of same type each have [Partial] independently
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Validation Constraints - Enforced by Later Stages:</b></para>
///     <para>
///     Transformer doesn't validate partial semantics. Later validation ensures:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             All partial declarations of a type have [Partial] (can't mix partial and non-partial)
///             </description>
///         </item>
///         <item>
///             <description>
///             Partial specifications don't have conflicting method signatures across declarations
///             </description>
///         </item>
///         <item>
///             <description>
///             Generated implementation combines all methods without name collisions
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Common Errors Prevented:</b></para>
///     <list type="bullet">
///         <item>
///             <term>Mixing partial and non-partial:</term>
///             <description>
///             Having [Specification] on one file and [Specification, Partial] on another for same type
///             causes ambiguity. Validator requires all-or-nothing [Partial] usage.
///             </description>
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
internal class PartialAttributeTransformer(
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
