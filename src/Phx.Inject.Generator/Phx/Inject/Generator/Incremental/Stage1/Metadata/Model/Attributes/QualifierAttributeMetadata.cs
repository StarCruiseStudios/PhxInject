// -----------------------------------------------------------------------------
// <copyright file="QualifierAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

/// <summary>
///     Metadata representing an analyzed [Qualifier] attribute that marks a custom attribute
///     type as a dependency qualifier in the DI framework.
/// </summary>
/// <param name="QualifierType">
///     The type metadata of the custom qualifier attribute being declared
///     (e.g., "MyApp.Attributes.ProductionAttribute"). This type itself becomes a qualifier
///     that can be applied to factories and parameters for type-safe disambiguation.
/// </param>
/// <param name="AttributeMetadata">
///     The common attribute metadata (class name, target, locations) shared by all attributes.
/// </param>
/// <remarks>
///     <para><b>Role in DI Framework:</b></para>
///     <para>
///     Enables users to create domain-specific qualifier attributes by marking an attribute
///     class with [Qualifier]. This promotes the attribute from a generic annotation to a
///     recognized DI qualifier that can be used to disambiguate bindings. Unlike [Label] which
///     uses strings, custom qualifiers provide compile-time type safety and can carry additional
///     metadata through their own attribute parameters.
///     </para>
///     
///     <para><b>What User Declarations Represent:</b></para>
///     <para>
///     When users write "[Qualifier] class ProductionAttribute : Attribute { }", they're declaring
///     a new qualifier type. Subsequently, "[Factory] [Production] ILogger Create()" and
///     "void Method([Production] ILogger logger)" create a typed binding relationship. This is
///     more maintainable than string labels for domain concepts (Production vs "production")
///     and supports IDE refactoring and find-all-references.
///     </para>
///     
///     <para><b>Why These Properties Were Chosen:</b></para>
///     <list type="bullet">
///         <item>
///             <description>
///             QualifierType (TypeMetadata): Code generation needs the fully-qualified type name
///             to identify which custom qualifier attribute to look for on factories and parameters.
///             TypeMetadata provides the complete type identity including namespace, enabling
///             unambiguous resolution even when multiple projects define identically-named
///             qualifier attributes. The type becomes part of the binding key in the dependency graph.
///             </description>
///         </item>
///         <item>
///             <description>
///             AttributeMetadata: Provides locations for reporting errors when the custom qualifier
///             is used incorrectly (e.g., applied to invalid targets) or when qualifier definitions
///             conflict. The target name helps identify which attribute type is problematic.
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Immutability Requirements:</b></para>
///     <para>
///     QualifierType is an immutable TypeMetadata record, and AttributeMetadata is immutable,
///     making this a stable cache key. Changes to the qualifier type identity correctly
///     invalidate the cache since the type name is part of binding keys. However, adding/removing
///     the [Qualifier] attribute to an attribute type is a breaking change that invalidates all
///     bindings using that qualifier.
///     </para>
///     
///     <para><b>Relationship to Other Models:</b></para>
///     <list type="bullet">
///         <item>
///             <description>
///             Converted to CustomQualifierMetadata (implementing IQualifierMetadata) for use in
///             QualifiedTypeMetadata binding keys
///             </description>
///         </item>
///         <item>
///             <description>
///             Alternative to LabelAttributeMetadata (string-based qualifiers) - more verbose but
///             type-safe and refactorable
///             </description>
///         </item>
///         <item>
///             <description>
///             QualifierType links to the type system model for validation and qualified type
///             construction
///             </description>
///         </item>
///         <item>
///             <description>
///             Used by dependency resolution to match providers with injection sites based on
///             qualifier type equality (not instance equality)
///             </description>
///         </item>
///     </list>
/// </remarks>
internal record QualifierAttributeMetadata(
    TypeMetadata QualifierType,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public const string AttributeClassName = $"{PhxInject.NamespaceName}.{nameof(QualifierAttribute)}";
    
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}