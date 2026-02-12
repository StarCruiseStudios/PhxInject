// -----------------------------------------------------------------------------
// <copyright file="FactoryAttributeMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata representing an analyzed [Factory] attribute that marks a method or property
///     as a dependency provider in the DI framework.
/// </summary>
/// <param name="FabricationMode">
///     Specifies the lifetime behavior of instances created by this factory.
///     Recurrent: Creates a new instance on each invocation.
///     Scoped: Returns the same instance within a given scope (lazy singleton per scope).
/// </param>
/// <param name="AttributeMetadata">
///     The common attribute metadata (class name, target, locations) shared by all attributes.
/// </param>
/// <remarks>
///     <para>Role in DI Framework:</para>
///     <para>
///     Represents a user-declared factory method that creates and returns new instances of
///     dependencies. Factories are the primary mechanism for defining how objects are constructed
///     in the dependency graph. They differ from Builders in that they create new objects rather
///     than initializing existing ones.
///     </para>
///     
///     <para>What User Declarations Represent:</para>
///     <para>
///     When users write "[Factory] static MyClass Create(IDependency dep) => new MyClass(dep)",
///     this metadata captures that declaration. The method signature defines both what is provided
///     (return type) and what dependencies it requires (parameters). The FabricationMode determines
///     whether each call creates a fresh instance or reuses a scoped singleton.
///     </para>
///     
///     <para>Why These Properties Were Chosen:</para>
///     <list type="bullet">
///         <item>
///             <description>
///             FabricationMode: Critical for code generation to determine whether to generate
///             caching logic (Scoped) or direct invocation (Recurrent). Affects the generated
///             injector's field declarations and method bodies. This is the semantic difference
///             between transient and singleton-like dependencies.
///             </description>
///         </item>
///         <item>
///             <description>
///             AttributeMetadata: Provides diagnostic locations for error reporting when factory
///             signatures are invalid or when circular dependencies are detected. The target name
///             helps identify which factory failed in complex dependency graphs.
///             </description>
///         </item>
///     </list>
///     
///     <para>Immutability Requirements:</para>
///     <para>
///     FabricationMode is an enum (inherently immutable), and AttributeMetadata is itself an
///     immutable record. This makes FactoryAttributeMetadata a stable cache key for incremental
///     compilation. Changes to the fabrication mode correctly invalidate the cache since they
///     affect generated code structure (fields for scoped vs no fields for recurrent).
///     </para>
///     
///     <para>Relationship to Other Models:</para>
///     <list type="bullet">
///         <item>
///             <description>
///             Used by FactoryModel which combines this metadata with method signature analysis
///             </description>
///         </item>
///         <item>
///             <description>
///             Distinguished from BuilderAttributeMetadata by the creation vs initialization semantics
///             </description>
///         </item>
///         <item>
///             <description>
///             FabricationMode informs InjectorModel's field generation for scoped dependency caching
///             </description>
///         </item>
///         <item>
///             <description>
///             Related to SpecificationAttributeMetadata as factories are defined within specifications
///             </description>
///         </item>
///     </list>
/// </remarks>
internal record FactoryAttributeMetadata(
    FabricationMode FabricationMode,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public const string AttributeClassName = $"{PhxInject.NamespaceName}.{nameof(FactoryAttribute)}";
    
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}