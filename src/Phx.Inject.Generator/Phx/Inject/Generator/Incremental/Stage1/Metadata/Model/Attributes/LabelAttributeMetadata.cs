// -----------------------------------------------------------------------------
// <copyright file="LabelAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

/// <summary>
///     Metadata representing an analyzed [Label] attribute that provides a string-based
///     qualifier for disambiguating dependencies of the same type.
/// </summary>
/// <param name="Label">
///     The string identifier that distinguishes this binding from others of the same type
///     (e.g., "primary", "backup", "production"). Case-sensitive for exact matching.
/// </param>
/// <param name="AttributeMetadata">
///     The common attribute metadata (class name, target, locations) shared by all attributes.
/// </param>
/// <remarks>
///     <para><b>Role in DI Framework:</b></para>
///     <para>
///     Provides a built-in string-based qualification mechanism for disambiguating multiple
///     bindings of the same type. Labels are the most common form of qualifier, offering a
///     lightweight alternative to creating custom qualifier attribute types. When applied to
///     a factory/builder, it marks what that binding provides. When applied to a parameter,
///     it specifies which labeled binding to inject.
///     </para>
///     
///     <para><b>What User Declarations Represent:</b></para>
///     <para>
///     When users write "[Factory] [Label("primary")] int GetPrimaryPort() => 8080" and
///     "[Factory] IService Create([Label("primary")] int port)", the label creates a qualified
///     binding relationship. The first factory provides a "primary"-labeled int, and the
///     parameter requests that same "primary"-labeled int. This allows multiple int factories
///     to coexist without ambiguity.
///     </para>
///     
///     <para><b>Why These Properties Were Chosen:</b></para>
///     <list type="bullet">
///         <item>
///             <description>
///             Label (string): Code generation needs the exact label string to generate binding
///             keys and perform dependency resolution. Strings provide human-readable identifiers
///             in diagnostics ("Cannot resolve dependency: [Label("backup")] ILogger") and support
///             dynamic scenarios without requiring new attribute types. Case-sensitivity ensures
///             predictable matching behavior.
///             </description>
///         </item>
///         <item>
///             <description>
///             AttributeMetadata: Provides locations for reporting errors when labels are mismatched
///             (e.g., "[Label("primary")] provided but [Label("backup")] requested) or when
///             multiple unlabeled bindings conflict with labeled ones.
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Immutability Requirements:</b></para>
///     <para>
///     String is immutable, and AttributeMetadata is immutable, making this a stable cache key.
///     Changes to the label string correctly invalidate the cache since they alter the binding
///     identity in the dependency graph. Label changes affect which dependencies can satisfy
///     which injection sites, fundamentally changing the resolution logic.
///     </para>
///     
///     <para><b>Relationship to Other Models:</b></para>
///     <list type="bullet">
///         <item>
///             <description>
///             Converted to LabelQualifierMetadata (implementing IQualifierMetadata) for use in
///             QualifiedTypeMetadata binding keys
///             </description>
///         </item>
///         <item>
///             <description>
///             Alternative to QualifierAttributeMetadata (custom qualifier types) - simpler but
///             less type-safe
///             </description>
///         </item>
///         <item>
///             <description>
///             Applied to the same targets as qualifiers: factory methods, builder methods, and
///             dependency parameters
///             </description>
///         </item>
///         <item>
///             <description>
///             Used by dependency resolution to match providers with injection sites based on
///             label equality
///             </description>
///         </item>
///     </list>
/// </remarks>
internal record LabelAttributeMetadata(
    string Label,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    /// <summary>
    ///     The fully-qualified name of the Label attribute class for pattern matching.
    /// </summary>
    /// <remarks>
    ///     Used during Stage 1 filtering to identify which symbols have label qualifiers.
    ///     Constant to enable compile-time optimizations.
    /// </remarks>
    public const string AttributeClassName = $"{PhxInject.NamespaceName}.{nameof(LabelAttribute)}";
    
    /// <summary>
    ///     Gets the source location of the attribute for diagnostic reporting.
    /// </summary>
    /// <remarks>
    ///     Delegates to the underlying AttributeMetadata. Wrapped in GeneratorIgnored
    ///     to exclude from equality comparisons, maintaining cache stability.
    /// </remarks>
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}