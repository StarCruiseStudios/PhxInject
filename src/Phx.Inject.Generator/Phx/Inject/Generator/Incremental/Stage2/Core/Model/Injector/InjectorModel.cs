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
///     Model representing a generated injector implementation.
/// </summary>
/// <param name="InjectorType"> The generated injector implementation type. </param>
/// <param name="InjectorInterfaceType"> The injector interface type. </param>
/// <param name="Specifications"> All specification types used by this injector. </param>
/// <param name="ConstructedSpecifications"> Specifications that are instantiated by the injector. </param>
/// <param name="Dependency"> The optional dependency injector type. </param>
/// <param name="Providers"> The provider methods to include in this injector. </param>
/// <param name="Builders"> The builder methods to include in this injector. </param>
/// <param name="ChildFactories"> The child injector factory methods. </param>
/// <param name="Location"> The source location where this injector is defined. </param>
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
