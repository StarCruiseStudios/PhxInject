// -----------------------------------------------------------------------------
// <copyright file="DependencyAttributeMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata representing an analyzed [Dependency] attribute that explicitly declares
///     a dependency relationship in the DI framework.
/// </summary>
/// <param name="DependencyType">
///     The type metadata of the dependency being declared. Represents the actual type
///     that must be provided to satisfy this dependency relationship.
/// </param>
/// <param name="AttributeMetadata">
///     The common attribute metadata (class name, target, locations) shared by all attributes.
/// </param>
/// <remarks>
///     <para>Role in DI Framework:</para>
///     <para>
///     Represents an explicit dependency declaration where the user manually specifies that
///     a type depends on another type. This attribute overrides or supplements automatic
///     dependency inference from constructor parameters or method signatures. It's primarily
///     used to declare dependencies that cannot be inferred automatically (e.g., optional
///     dependencies, or dependencies with complex resolution logic).
///     </para>
///     
///     <para>What User Declarations Represent:</para>
///     <para>
///     When users write "[Dependency(typeof(ILogger))] class MyClass", this metadata captures
///     that MyClass explicitly requires ILogger, even if that dependency isn't visible in
///     the constructor signature. This is useful for generated code scenarios, partial classes,
///     or when dependencies are satisfied through non-constructor means.
///     </para>
///     
///     <para>Why These Properties Were Chosen:</para>
///     <list type="bullet">
///         <item>
///             <description>
///             DependencyType: Code generation needs to know the exact type to resolve when
///             satisfying this dependency. Unlike implicit dependencies extracted from parameters,
///             this is an explicit type declaration that may reference types not directly used
///             in the class's public API. TypeMetadata provides the fully-qualified identity
///             needed for unambiguous resolution in the dependency graph.
///             </description>
///         </item>
///         <item>
///             <description>
///             AttributeMetadata: Provides diagnostic locations for reporting errors when the
///             declared dependency cannot be satisfied or creates circular dependencies. The
///             target name identifies which type has the unsatisfiable dependency.
///             </description>
///         </item>
///     </list>
///     
///     <para>Immutability Requirements:</para>
///     <para>
///     DependencyType is an immutable TypeMetadata record, and AttributeMetadata is immutable.
///     This makes DependencyAttributeMetadata a stable cache key. Changes to the dependency
///     type correctly invalidate the cache since they alter the dependency graph structure,
///     affecting which factories/builders can be used and what resolution order is required.
///     </para>
///     
///     <para>Relationship to Other Models:</para>
///     <list type="bullet">
///         <item>
///             <description>
///             Used by dependency graph analysis to construct the complete dependency tree
///             </description>
///         </item>
///         <item>
///             <description>
///             Complements implicit dependencies extracted from Factory/Builder method parameters
///             </description>
///         </item>
///         <item>
///             <description>
///             TypeMetadata links to the type system model for resolution and validation
///             </description>
///         </item>
///         <item>
///             <description>
///             May be combined with QualifierAttributeMetadata when qualified dependencies are needed
///             </description>
///         </item>
///     </list>
/// </remarks>
internal record DependencyAttributeMetadata(
    TypeMetadata DependencyType,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public const string AttributeClassName = $"{PhxInject.NamespaceName}.{nameof(DependencyAttribute)}";
    
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}