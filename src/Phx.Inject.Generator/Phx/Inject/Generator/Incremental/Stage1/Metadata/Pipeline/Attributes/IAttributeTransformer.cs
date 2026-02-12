// -----------------------------------------------------------------------------
// <copyright file="IAttributeTransformer.cs" company="Star Cruise Studios LLC">
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
///     Transforms attribute data from a symbol into attribute metadata.
/// </summary>
/// <typeparam name="TAttributeMetadata">The type of attribute metadata to produce.</typeparam>
/// <remarks>
///     <para>Architecture - Bridging Roslyn to Domain Model:</para>
///     <para>
///     Attribute transformers are the critical bridge between Roslyn's semantic model and our
///     generator's domain model. They extract attribute arguments from Roslyn's AttributeData
///     representation and convert them into strongly-typed, equatable metadata objects suitable
///     for incremental generator caching.
///     </para>
///     
///     <para>Roslyn Attribute Data Traversal Pattern:</para>
///     <para>
///     Roslyn represents attributes through the AttributeData type, which exposes:
///     </para>
///     <list type="bullet">
///         <item>
///             <term>AttributeClass (INamedTypeSymbol):</term>
///             <description>
///             The resolved type symbol of the attribute class. Used for attribute detection by
///             comparing fully qualified names. Critical for distinguishing our attributes from
///             similarly-named third-party attributes.
///             </description>
///         </item>
///         <item>
///             <term>ConstructorArguments (ImmutableArray&lt;TypedConstant&gt;):</term>
///             <description>
///             Positional arguments passed to the attribute constructor. TypedConstant wraps the
///             value with type information, handling primitives, enums, types (as ITypeSymbol),
///             and arrays. Transformers must unwrap these using predicates since parameter names
///             are not available - only position and type.
///             </description>
///         </item>
///         <item>
///             <term>NamedArguments (ImmutableArray&lt;KeyValuePair&lt;string, TypedConstant&gt;&gt;):</term>
///             <description>
///             Named property initializers (e.g., [Injector(GeneratedClassName = "Foo")]).
///             Accessible by string key. Preferred over constructor arguments when available
///             since they're self-documenting and order-independent.
///             </description>
///         </item>
///         <item>
///             <term>ApplicationSyntaxReference:</term>
///             <description>
///             Reference back to the syntax node where the attribute was applied. Used to
///             extract precise location information for diagnostics. May be null for
///             attributes from metadata (referenced assemblies).
///             </description>
///         </item>
///     </list>
///     
///     <para>Why IResult Return Type - Error Handling Strategy:</para>
///     <para>
///     Transform returns IResult&lt;TAttributeMetadata&gt; rather than directly returning metadata
///     to support validation and diagnostic generation during transformation. Reasons:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Attribute arguments may be malformed (e.g., invalid enum value, null where not allowed)
///             </description>
///         </item>
///         <item>
///             <description>
///             Type arguments may reference inaccessible or non-existent types (broken references)
///             </description>
///         </item>
///         <item>
///             <description>
///             Semantic validation (e.g., DependencyAttribute requires public interface) may fail
///             </description>
///         </item>
///         <item>
///             <description>
///             Enables precise diagnostic location pointing to the attribute application site
///             </description>
///         </item>
///     </list>
///     <para>
///     Returning IResult allows transformers to capture these failures with diagnostic context,
///     which the pipeline can surface as compiler errors at the exact attribute location.
///     </para>
///     
///     <para>Integration with Validators:</para>
///     <para>
///     Some transformers (e.g., DependencyAttributeTransformer) inject ICodeElementValidator
///     instances to validate extracted type arguments. This follows single-responsibility:
///     transformers extract data, validators verify structural constraints. Validators generate
///     their own diagnostics which the transformer wraps in Error results.
///     </para>
///     
///     <para>Equatability Contract - Incremental Generation Requirement:</para>
///     <para>
///     TAttributeMetadata must implement IEquatable&lt;TAttributeMetadata&gt;. Roslyn's incremental
///     generator pipeline uses equality comparison to detect when cached results are still valid.
///     If metadata hasn't changed between compilations, downstream transformation steps are skipped,
///     dramatically improving IDE responsiveness during typing.
///     </para>
///     
///     <para>Performance Considerations:</para>
///     <list type="bullet">
///         <item>
///             <description>
///             Only transform when HasAttribute returns true (use TransformOrNull pattern)
///             </description>
///         </item>
///         <item>
///             <description>
///             Avoid materializing type symbols for unneeded arguments (lazy evaluation)
///             </description>
///         </item>
///         <item>
///             <description>
///             Cache singleton transformer instances - transformers are stateless and thread-safe
///             </description>
///         </item>
///         <item>
///             <description>
///             Minimize allocations in hot path by reusing extension methods (GetConstructorArgument, etc.)
///             </description>
///         </item>
///     </list>
/// </remarks>
internal interface IAttributeTransformer<out TAttributeMetadata> : IAttributeChecker where TAttributeMetadata : IAttributeElement, IEquatable<TAttributeMetadata> {
    /// <summary>
    ///     Transforms the attribute on the target symbol into metadata.
    /// </summary>
    /// <param name="targetSymbol">The symbol with the attribute.</param>
    /// <returns>A result containing the attribute metadata.</returns>
    /// <remarks>
    ///     <para>
    ///     Expects that HasAttribute(targetSymbol) returns true. If the attribute is not present,
    ///     behavior is undefined (typically throws InvalidOperationException). Callers should use
    ///     TransformOrNull extension method for conditional transformation.
    ///     </para>
    /// </remarks>
    IResult<TAttributeMetadata> Transform(ISymbol targetSymbol);
}

/// <summary>
///     Extension methods for attribute transformers.
/// </summary>
internal static class IAttributeTransformerExtensions {
    /// <summary>
    ///     Transforms the attribute if present, or returns null.
    /// </summary>
    /// <typeparam name="TAttributeMetadata">The type of attribute metadata.</typeparam>
    /// <param name="transformer">The attribute transformer.</param>
    /// <param name="targetSymbol">The symbol to check and transform.</param>
    /// <returns>The transform result if the attribute exists; otherwise, null.</returns>
    /// <remarks>
    ///     <para>Check-Then-Transform Pattern:</para>
    ///     <para>
    ///     Implements safe conditional transformation by checking attribute existence before
    ///     transformation. This is the recommended pattern for optional attributes where absence
    ///     is semantically meaningful (e.g., Factory attribute may or may not be present on a
    ///     specification method).
    ///     </para>
    ///     <para>
    ///     Avoids the overhead of try-catch or defensive null checking in Transform implementations.
    ///     HasAttribute is explicitly designed for this filtering role and is optimized for fast
    ///     negative checks.
    ///     </para>
    /// </remarks>
    public static IResult<TAttributeMetadata>? TransformOrNull<TAttributeMetadata>(this IAttributeTransformer<TAttributeMetadata> transformer, ISymbol targetSymbol) where TAttributeMetadata : IAttributeElement, IEquatable<TAttributeMetadata> {
        return transformer.HasAttribute(targetSymbol)
            ? transformer.Transform(targetSymbol)
            : null;
    }
}