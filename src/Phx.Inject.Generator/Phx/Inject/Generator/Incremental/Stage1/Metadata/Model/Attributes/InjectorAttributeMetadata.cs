// -----------------------------------------------------------------------------
// <copyright file="InjectorAttributeMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata representing an analyzed [Injector] attribute that marks an interface as
///     a DI container access point.
/// </summary>
/// <param name="GeneratedClassName">
///     Optional custom name for the generated injector implementation class.
///     If null, defaults to "Generated{InterfaceName}" (e.g., "GeneratedIMyInjector").
/// </param>
/// <param name="Specifications">
///     Ordered list of specification types that provide the dependency graph definitions.
///     Each specification contributes factories, builders, and links to the injector's
///     available dependencies. Must use EquatableList for proper structural equality.
/// </param>
/// <param name="AttributeMetadata">
///     The common attribute metadata (class name, target, locations) shared by all attributes.
/// </param>
/// <remarks>
///     <para><b>Role in DI Framework:</b></para>
///     <para>
///     Represents the user's declaration of a DI container interface. The [Injector] attribute
///     marks an interface as the runtime access point for retrieving dependencies. The framework
///     generates an implementation class that orchestrates dependency construction based on the
///     specifications provided.
///     </para>
///     
///     <para><b>What User Declarations Represent:</b></para>
///     <para>
///     When users write "[Injector(typeof(MySpecification))] interface IMyInjector", this metadata
///     captures that declaration. The interface methods become provider methods (parameterless)
///     or activators (accepting the object to initialize). Specifications define what dependencies
///     are available to satisfy those provider methods.
///     </para>
///     
///     <para><b>Why These Properties Were Chosen:</b></para>
///     <list type="bullet">
///         <item>
///             <description>
///             GeneratedClassName: Allows users to control the generated class name for better
///             integration with existing codebases or naming conventions. Null handling enables
///             a sensible default without requiring explicit naming in simple cases.
///             </description>
///         </item>
///         <item>
///             <description>
///             Specifications: Code generation needs to know which dependency graph definitions
///             to include when generating the injector implementation. The ordered list preserves
///             specification precedence for conflict resolution (later specs override earlier ones).
///             EquatableList ensures proper equality for incremental caching.
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Immutability Requirements:</b></para>
///     <para>
///     This record serves as a cache key in incremental compilation. All properties must be
///     immutable value types or records. The Specifications property uses EquatableList to
///     provide structural equality rather than reference equality. Location data is excluded
///     from equality via GeneratorIgnored to prevent cache invalidation on whitespace changes.
///     </para>
///     
///     <para><b>Relationship to Other Models:</b></para>
///     <list type="bullet">
///         <item>
///             <description>
///             Links to SpecificationMetadata via the Specifications TypeMetadata references
///             </description>
///         </item>
///         <item>
///             <description>
///             Used by InjectorModel in later stages to generate the actual injector implementation
///             </description>
///         </item>
///         <item>
///             <description>
///             Consumed by validation logic to ensure specifications exist and are properly formed
///             </description>
///         </item>
///     </list>
/// </remarks>
internal record InjectorAttributeMetadata(
    string? GeneratedClassName,
    EquatableList<TypeMetadata> Specifications,
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public const string AttributeClassName =
        $"{PhxInject.NamespaceName}.{nameof(InjectorAttribute)}";
    
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}