// -----------------------------------------------------------------------------
// <copyright file="SpecContainerModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Model.SpecContainer;

/// <summary>
///     Model representing a specification container for code generation.
/// </summary>
/// <remarks>
///     <para><b>WHY: Specification Container Pattern</b></para>
///     <para>
///     Consolidates all factory and builder methods from a specification (SpecClassMetadata or
///     SpecInterfaceMetadata) into a single generated container class. This aggregation serves three purposes:
///     1) Centralizes dependency resolution - one container manages all related factories
///     2) Enables method reuse - factories can invoke other factories in the same container
///     3) Simplifies Injector generation - Injector delegates to containers instead of duplicating factory logic
///     </para>
///     
///     <para><b>Relationship to Stage 1 Metadata:</b></para>
///     <para>
///     Derived from Stage 1's SpecClassMetadata or SpecInterfaceMetadata which contain the user-authored
///     specification definitions. Stage 2 transforms that metadata into this code generation model:
///     - SpecClassMetadata.FactoryMethods/FactoryProperties/FactoryReferences → FactoryMethodDefs
///     - SpecClassMetadata.BuilderMethods/BuilderReferences → BuilderMethodDefs
///     - SpecClassMetadata.SpecAttributeMetadata determines SpecInstantiationMode
///     </para>
///     
///     <para><b>Generated Code Structure:</b></para>
///     <para>
///     Produces a class like: `class MySpec_Container { public T CreateFoo() { ... } }`
///     The container class wraps the user's specification instance (for non-static specs) or provides
///     static methods (for static specs), routing each factory/builder call to the appropriate
///     specification member while resolving dependencies from the dependency graph.
///     </para>
/// </remarks>
/// <param name="SpecContainerType"> The specification container implementation type. </param>
/// <param name="SpecificationType"> The specification type contained. </param>
/// <param name="SpecInstantiationMode"> The instantiation mode for the specification. </param>
/// <param name="FactoryMethodDefs"> The factory method definitions in this container. </param>
/// <param name="BuilderMethodDefs"> The builder method definitions in this container. </param>
/// <param name="Location"> The source location where this container is defined. </param>
internal record SpecContainerModel(
    TypeMetadata SpecContainerType,
    TypeMetadata SpecificationType,
    SpecInstantiationMode SpecInstantiationMode,
    IEnumerable<SpecContainerFactoryModel> FactoryMethodDefs,
    IEnumerable<SpecContainerBuilderModel> BuilderMethodDefs,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;

/// <summary>
///     Specifies how a specification is instantiated.
/// </summary>
/// <remarks>
///     <para>
///     Controls the lifetime and ownership of the specification instance:
///     - Static: Specification has only static members, no instance needed
///     - Instantiated: Injector creates new specification instance per request
///     - Dependency: Specification instance provided externally (e.g., from parent injector)
///     </para>
///     <para>
///     This determines whether the generated container wraps a specification field/property
///     or simply forwards calls to static methods.
///     </para>
/// </remarks>
internal enum SpecInstantiationMode {
    Static = 0,
    Instantiated = 1,
    Dependency = 2
}
