// -----------------------------------------------------------------------------
// <copyright file="InjectorModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Model.Injector;

/// <summary>
///     Code generation model for the concrete injector implementation class.
/// </summary>
/// <param name="InjectorType">
///     The generated implementation class type (e.g., "RequestInjector_RequestSpec").
/// </param>
/// <param name="InjectorInterfaceType">
///     The user-defined interface type. The generated class implements this.
/// </param>
/// <param name="Specifications">
///     All specification types referenced by this injector, including parent-accessed specs.
/// </param>
/// <param name="ConstructedSpecifications">
///     Specifications instantiated by this injector (subset of Specifications).
/// </param>
/// <param name="Dependency">
///     Optional parent injector dependency interface (e.g., "IApplicationDependencies").
/// </param>
/// <param name="Providers">
///     Provider method models to generate.
/// </param>
/// <param name="Builders">
///     Builder method models to generate.
/// </param>
/// <param name="ChildFactories">
///     Child factory method models to generate.
/// </param>
/// <param name="Location">The source location where this injector is defined.</param>
/// <remarks>
///     Transforms Stage 1's <see cref="Stage1.Metadata.Model.Injector.InjectorInterfaceMetadata"/>
///     into an actionable code generation model. Each InjectorModel produces one concrete class that
///     implements the user's injector interface, delegating to specification containers for dependency
///     resolution. Supports hierarchical injectors with parent dependencies and child factory methods.
/// </remarks>
/// <param name="InjectorType"> 
///     The generated implementation class type (e.g., "RequestInjector_RequestSpec"). Synthesized
///     from interface name + specification types by InjectorMapper.
/// </param>
/// <param name="InjectorInterfaceType"> 
///     The user-defined interface type (e.g., "IRequestInjector"). The generated class implements this.
/// </param>
/// <param name="Specifications"> 
///     All specification types referenced by this injector, including parent-accessed specs. Used for
///     imports and dependency analysis. Superset of ConstructedSpecifications.
/// </param>
/// <param name="ConstructedSpecifications"> 
///     Specifications instantiated by this injector (subset of Specifications). Each becomes a private
///     field in the generated class. Parent-provided specs are excluded.
/// </param>
/// <param name="Dependency"> 
///     Optional parent injector dependency interface (e.g., "IApplicationDependencies"). If present,
///     generates a constructor parameter and field for accessing parent-provided dependencies.
/// </param>
/// <param name="Providers"> 
///     Provider method models to generate. Each produces a method that constructs and returns an object
///     by delegating to a specification container's factory method.
/// </param>
/// <param name="Builders"> 
///     Builder method models to generate. Each produces a method that initializes an existing object
///     by delegating to a specification container's builder method.
/// </param>
/// <param name="ChildFactories"> 
///     Child factory method models to generate. Each produces a method that instantiates a child
///     injector, passing this injector as parent and forwarding context parameters.
/// </param>
/// <param name="Location"> The source location where this injector is defined for diagnostics. </param>
internal record InjectorModel(
    TypeMetadata InjectorType,
    TypeMetadata InjectorInterfaceType,
    IEnumerable<TypeMetadata> Specifications,
    IEnumerable<TypeMetadata> ConstructedSpecifications,
    TypeMetadata? Dependency,
    IEnumerable<InjectorProviderModel> Providers,
    IEnumerable<InjectorBuilderModel> Builders,
    IEnumerable<InjectorChildFactoryModel> ChildFactories,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
