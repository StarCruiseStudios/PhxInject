// -----------------------------------------------------------------------------
// <copyright file="AttributeMetadataTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Pairs attribute data with its corresponding metadata.
/// </summary>
/// <remarks>
///     <para><b>Dual-View Pattern - Raw Data + Location Context:</b></para>
///     <para>
///     Encapsulates both Roslyn's AttributeData (the raw attribute information) and our
///     AttributeMetadata (standardized location/context information). This allows transformers
///     to extract argument values from Data while using Metadata for diagnostic reporting.
///     </para>
///     <para>
///     Separating these concerns prevents location calculation logic from being duplicated
///     across every transformer implementation. AttributeMetadata.Create handles the complex
///     logic of determining precise source locations from ApplicationSyntaxReference.
///     </para>
/// </remarks>
internal record AttributeMetadataPair(
    AttributeData Data,
    AttributeMetadata Metadata
) {
    /// <summary>
    ///     Creates an attribute metadata pair from a symbol and attribute data.
    /// </summary>
    /// <param name="targetSymbol">The symbol that has the attribute.</param>
    /// <param name="attributeData">The attribute data from Roslyn.</param>
    /// <returns>A new attribute metadata pair.</returns>
    public static AttributeMetadataPair From(ISymbol targetSymbol, AttributeData attributeData) {
        return new AttributeMetadataPair(attributeData, AttributeMetadata.Create(targetSymbol, attributeData));
    }
}

/// <summary>
///     Provides methods to transform and query attribute data.
/// </summary>
/// <remarks>
///     <para><b>Foundation Service - Base Attribute Query Layer:</b></para>
///     <para>
///     IAttributeMetadataTransformer is the foundational service that all specific attribute
///     transformers depend on. It encapsulates Roslyn attribute querying patterns and provides
///     a consistent API for attribute detection and extraction.
///     </para>
///     
///     <para><b>Why Separate Interface - Testability and Mocking:</b></para>
///     <para>
///     By abstracting attribute queries behind an interface, specific transformers (like
///     InjectorAttributeTransformer) can be unit tested by mocking attribute presence/data
///     without requiring full Roslyn compilation objects. Critical for fast, isolated tests
///     of transformation logic.
///     </para>
/// </remarks>
internal interface IAttributeMetadataTransformer {
    /// <summary>
    ///     Checks if the target symbol has the specified attribute.
    /// </summary>
    /// <param name="targetSymbol">The symbol to check.</param>
    /// <param name="attributeClassName">The fully qualified attribute class name.</param>
    /// <returns>True if the attribute is present; otherwise, false.</returns>
    /// <remarks>
    ///     <para>
    ///     Uses string-based fully qualified name matching rather than symbol equality to handle
    ///     attributes defined in different compilations or referenced assemblies. More robust
    ///     than comparing INamedTypeSymbol references which may differ across compilation contexts.
    ///     </para>
    /// </remarks>
    bool HasAttribute(ISymbol targetSymbol, string attributeClassName);
    
    /// <summary>
    ///     Gets all attributes of the specified type from the target symbol.
    /// </summary>
    /// <param name="targetSymbol">The symbol to query.</param>
    /// <param name="attributeClassName">The fully qualified attribute class name.</param>
    /// <returns>A list of attribute metadata pairs.</returns>
    /// <remarks>
    ///     <para>
    ///     Used by IAttributeListTransformer implementations for repeatable attributes.
    ///     Returns empty list if no matches found. Preserves source declaration order.
    ///     </para>
    /// </remarks>
    EquatableList<AttributeMetadataPair> GetAttributes(
        ISymbol targetSymbol,
        string attributeClassName);
    
    /// <summary>
    ///     Gets a single attribute of the specified type, or null if not present.
    /// </summary>
    /// <param name="targetSymbol">The symbol to query.</param>
    /// <param name="attributeClassName">The fully qualified attribute class name.</param>
    /// <returns>The attribute metadata pair, or null if not found.</returns>
    /// <remarks>
    ///     <para>
    ///     For attributes with [AttributeUsage(AllowMultiple = false)]. Uses SingleOrDefault,
    ///     which throws if multiple attributes found - catches attribute misuse early.
    ///     Null return indicates attribute absence, distinguishing from transformation failure.
    ///     </para>
    /// </remarks>
    AttributeMetadataPair? SingleAttributeOrNull(
        ISymbol targetSymbol,
        string attributeClassName);
    
    /// <summary>
    ///     Gets a single attribute of the specified type, throwing if not present.
    /// </summary>
    /// <param name="targetSymbol">The symbol to query.</param>
    /// <param name="attributeClassName">The fully qualified attribute class name.</param>
    /// <returns>The attribute metadata pair.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the attribute is not found.</exception>
    /// <remarks>
    ///     <para><b>Fail-Fast Pattern for Required Attributes:</b></para>
    ///     <para>
    ///     Used when transformer is called only after HasAttribute confirmed presence.
    ///     The exception indicates programmer error (calling Transform without checking), not
    ///     user error. Exception message includes diagnostic context for debugging generator issues.
    ///     </para>
    ///     <para>
    ///     Throws rather than returning error Result because this is infrastructure failure, not
    ///     semantic validation failure. Semantic errors return Result.Error with diagnostics.
    ///     </para>
    /// </remarks>
    AttributeMetadataPair ExpectSingleAttribute(
        ISymbol targetSymbol,
        string attributeClassName);
}

/// <summary>
///     Transforms attribute data into attribute metadata.
/// </summary>
/// <remarks>
///     <para><b>Singleton Pattern - Stateless Service:</b></para>
///     <para>
///     Implemented as singleton since it has no mutable state and all operations are thread-safe.
///     Specific attribute transformers inject this singleton, avoiding redundant allocations.
///     </para>
///     
///     <para><b>Roslyn Symbol Walking - GetAttributes() Behavior:</b></para>
///     <para>
///     Leverages ISymbol.GetAttributes(), which returns all attributes applied directly to the
///     symbol (not inherited). For type symbols, includes attributes on the type declaration only,
///     not on members. For methods/properties, includes attributes on that member only.
///     </para>
///     <para>
///     GetAttributes() returns AttributeData with resolved type information - attribute class must
///     be accessible and valid at call time. Malformed attributes (e.g., references to deleted types)
///     may return AttributeData with null AttributeClass, which we handle via extension methods
///     that throw descriptive exceptions.
///     </para>
///     
///     <para><b>Performance - Attribute Filtering Strategy:</b></para>
///     <para>
///     Uses fully qualified name string comparison (GetFullyQualifiedName()) rather than symbol
///     equality checks. Trade-off:
///     </para>
///     <list type="bullet">
///         <item>
///             <term>Cost:</term>
///             <description>
///             String allocation and comparison for each attribute (typically 1-5 per symbol)
///             </description>
///         </item>
///         <item>
///             <term>Benefit:</term>
///             <description>
///             Works reliably across compilation boundaries and with retargeted assemblies where
///             symbol references may differ but qualified names remain stable
///             </description>
///         </item>
///     </list>
///     <para>
///     The string comparison approach is empirically faster than resolving and comparing symbol
///     references when the symbol count is small, which is the common case.
///     </para>
/// </remarks>
internal class AttributeMetadataTransformer : IAttributeMetadataTransformer {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static readonly AttributeMetadataTransformer Instance = new();
    
    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol, string attributeClassName) {
        return targetSymbol.GetAttributes().Any(a => a.GetFullyQualifiedName() == attributeClassName);
    }

    /// <inheritdoc />
    public EquatableList<AttributeMetadataPair> GetAttributes(
        ISymbol targetSymbol,
        string attributeClassName
    ) {
        return targetSymbol.GetAttributes()
            .Where(attributeData => attributeData.GetFullyQualifiedName() == attributeClassName)
            .Select(attributeData => AttributeMetadataPair.From(targetSymbol, attributeData))
            .ToEquatableList();
    }

    /// <inheritdoc />
    public AttributeMetadataPair? SingleAttributeOrNull(
        ISymbol targetSymbol,
        string attributeClassName
    ) {
        var attributeData = targetSymbol.GetAttributes()
            .SingleOrDefault(attributeData => attributeData.GetNamedTypeSymbol().GetFullyQualifiedBaseName() == attributeClassName);
        return attributeData != null ? AttributeMetadataPair.From(targetSymbol, attributeData) : null;
    }
    
    /// <inheritdoc />
    public AttributeMetadataPair ExpectSingleAttribute(
        ISymbol targetSymbol,
        string attributeClassName
    ) {
        var attributeData = targetSymbol.GetAttributes()
            .SingleOrDefault(attributeData => attributeData.GetNamedTypeSymbol().GetFullyQualifiedBaseName() == attributeClassName);
        return attributeData != null 
            ? AttributeMetadataPair.From(targetSymbol, attributeData) 
            : throw new InvalidOperationException(
                $"Expected single {attributeClassName} attribute on {targetSymbol.Name}. " +
                $"Found {targetSymbol.GetAttributes().Count(d => d.GetNamedTypeSymbol().GetFullyQualifiedBaseName() == attributeClassName)}");
    }
    
    

}