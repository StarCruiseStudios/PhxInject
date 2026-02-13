// -----------------------------------------------------------------------------
// <copyright file="AttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

/// <summary>
///     Common metadata shared by all analyzed framework attributes during Stage 1 processing.
/// </summary>
/// <param name="AttributeClassName">
///     The fully-qualified name of the attribute class (e.g., "Phx.Inject.InjectorAttribute").
///     Used to identify which framework attribute was applied during diagnostic reporting.
/// </param>
/// <param name="TargetName">
///     The fully-qualified name of the symbol this attribute is applied to
///     (e.g., "MyNamespace.MyClass.MyMethod"). Captured for diagnostic messages.
/// </param>
/// <param name="TargetLocation">
///     Source location of the attributed symbol, wrapped in GeneratorIgnored.
///     Excluded from equality to prevent whitespace-only changes from invalidating cache.
/// </param>
/// <param name="Location">
///     Source location of the attribute application itself, wrapped in GeneratorIgnored.
///     Used for diagnostic reporting but excluded from equality/hashing for cache stability.
/// </param>
/// <remarks>
///     <para>Role in DI Framework:</para>
///     <para>
///     Serves as the base metadata container for all framework attribute types. Contains
///     location and identity information needed for diagnostics while maintaining the
///     immutability contract required for incremental compilation caching.
///     </para>
///     
///     <para>Why These Properties:</para>
///     <list type="bullet">
///         <item>
///             <description>
///             AttributeClassName: Enables runtime identification of attribute type for diagnostics
///             without maintaining references to Roslyn symbol objects
///             </description>
///         </item>
///         <item>
///             <description>
///             TargetName: Provides human-readable symbol identification in error messages
///             (e.g., "Factory method MyClass.Create has invalid signature")
///             </description>
///         </item>
///         <item>
///             <description>
///             TargetLocation + Location: Both locations support precise IDE integration for
///             squiggles and quick-fixes, distinguishing between attribute and target errors
///             </description>
///         </item>
///     </list>
///     
///     <para>Immutability for Incremental Caching:</para>
///     <para>
///     Both location properties are wrapped in GeneratorIgnored to exclude them from structural
///     equality comparisons. This ensures that moving code around or reformatting doesn't
///     invalidate the incremental cache when semantic content is unchanged. The sealed keyword
///     prevents inheritance that might violate immutability invariants.
///     </para>
///     
///     <para>Relationship to Other Models:</para>
///     <para>
///     AttributeMetadata is embedded in all specific attribute metadata types (InjectorAttributeMetadata,
///     FactoryAttributeMetadata, etc.) as a composition property, providing a consistent base layer.
///     This is preferred over inheritance to avoid shared mutable state and maintain clear ownership.
///     </para>
/// </remarks>
sealed internal record AttributeMetadata(
    string AttributeClassName,
    string TargetName,
    GeneratorIgnored<LocationInfo?> TargetLocation,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement {
    /// <summary>
    ///     Factory method to construct AttributeMetadata from Roslyn's symbol and attribute models.
    /// </summary>
    /// <param name="targetSymbol">
    ///     The symbol (class, method, parameter, etc.) that the attribute is applied to.
    /// </param>
    /// <param name="attributeData">
    ///     Roslyn's representation of the attribute application, including constructor arguments.
    /// </param>
    /// <returns>
    ///     A new immutable AttributeMetadata instance capturing the essential attribute information.
    /// </returns>
    /// <remarks>
    ///     This is the standard entry point for converting from Roslyn's mutable symbol model
    ///     to our immutable metadata representation. Extracts fully-qualified names and locations
    ///     while wrapping location data in GeneratorIgnored for cache stability.
    /// </remarks>
    public static AttributeMetadata Create(ISymbol targetSymbol, AttributeData attributeData) {
        return new AttributeMetadata(
            attributeData.GetNamedTypeSymbol().GetFullyQualifiedBaseName(),
            targetSymbol.ToString(),
            targetSymbol.Locations.FirstOrDefault().GeneratorIgnored(),
            attributeData.GetAttributeLocation(targetSymbol).GeneratorIgnored());
    }
}