// -----------------------------------------------------------------------------
// <copyright file="InjectorChildFactoryModel.cs" company="Star Cruise Studios LLC">
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
///     Model representing a child injector factory method.
/// </summary>
/// <param name="ChildInjectorType"> The type of child injector created by the factory. </param>
/// <param name="ChildFactoryMethodName"> The name of the factory method. </param>
/// <param name="Parameters"> The parameters required by the factory method. </param>
/// <param name="Location"> The source location where this factory is defined. </param>
internal record InjectorChildFactoryModel(
    TypeMetadata ChildInjectorType,
    string ChildFactoryMethodName,
    IEnumerable<TypeMetadata> Parameters,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
