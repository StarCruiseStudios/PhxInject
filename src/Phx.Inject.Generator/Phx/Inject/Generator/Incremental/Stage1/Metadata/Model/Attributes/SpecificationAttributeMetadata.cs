// -----------------------------------------------------------------------------
// <copyright file="SpecificationAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Util;
using static Phx.Inject.Generator.Incremental.PhxInject;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

/// <summary>
///     Metadata representing an analyzed [Specification] attribute that marks a class or
///     interface as a DI binding specification.
/// </summary>
/// <param name="AttributeMetadata">
///     The common attribute metadata (class name, target, locations) shared by all attributes.
/// </param>
/// <remarks>
///     <para><b>Role in DI Framework:</b></para>
///     <para>
///     Represents a user-declared specification class that defines the dependency graph
///     configuration. Specifications are the core organizational unit of the DI framework,
///     containing the factory methods, builder methods, link declarations, and factory
///     references that collectively define how dependencies are constructed and wired together.
///     They are the "recipe book" that injectors consult when resolving dependencies.
///     </para>
///     
///     <para><b>What User Declarations Represent:</b></para>
///     <para>
///     When users write "[Specification] static class MySpec { ... }", this metadata captures
///     that the class contains DI binding definitions. Specifications can be static classes
///     (for stateless bindings) or interfaces (for modular, composable binding definitions).
///     The specification becomes a dependency graph module that can be referenced by multiple
///     injectors or composed with other specifications.
///     </para>
///     
///     <para><b>Why These Properties Were Chosen:</b></para>
///     <para>
///     The [Specification] attribute itself has no parameters - it's purely a marker attribute.
///     Therefore, only the base AttributeMetadata is needed. The actual specification content
///     (factories, builders, etc.) is extracted by analyzing the class members, not the attribute.
///     This separation keeps the attribute metadata simple while deferring complex structural
///     analysis to SpecificationModel in later pipeline stages.
///     </para>
///     
///     <para><b>Code Generation Needs:</b></para>
///     <para>
///     Code generation doesn't directly generate code from SpecificationAttributeMetadata itself.
///     Instead, this metadata identifies which classes should be analyzed for their contained
///     factory/builder methods. The specification serves as a namespace/container that groups
///     related bindings, affecting how the generator organizes and references binding methods
///     in the generated injector code.
///     </para>
///     
///     <para><b>Immutability Requirements:</b></para>
///     <para>
///     Contains only the immutable AttributeMetadata record, making this a stable cache key.
///     Changes to specification contents (adding/removing factories) affect the individual
///     factory/builder metadata records rather than this marker attribute, enabling fine-grained
///     incremental compilation: only the changed members invalidate their caches, not the entire
///     specification.
///     </para>
///     
///     <para><b>Relationship to Other Models:</b></para>
///     <list type="bullet">
///         <item>
///             <description>
///             Referenced by InjectorAttributeMetadata.Specifications to link injectors to their specs
///             </description>
///         </item>
///         <item>
///             <description>
///             Used by SpecificationModel which aggregates all factories, builders, and links within
///             </description>
///         </item>
///         <item>
///             <description>
///             Contains FactoryAttributeMetadata, BuilderAttributeMetadata, and LinkAttributeMetadata
///             as child elements analyzed during later stages
///             </description>
///         </item>
///         <item>
///             <description>
///             Specifications can reference other specifications for modular composition
///             </description>
///         </item>
///     </list>
/// </remarks>
internal record SpecificationAttributeMetadata(
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public const string AttributeClassName = $"{NamespaceName}.{nameof(SpecificationAttribute)}";
    
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}