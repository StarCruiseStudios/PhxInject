// -----------------------------------------------------------------------------
// <copyright file="InjectorActivatorMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;

/// <summary>
///     Metadata representing an analyzed injector activator method.
/// </summary>
/// <param name="ActivatorMethodName"> The name of the activator method. </param>
/// <param name="ActivatedType"> The qualified type that is activated. </param>
/// <param name="Location"> The source location of the activator definition. </param>
internal record InjectorActivatorMetadata(
    string ActivatorMethodName,
    QualifiedTypeMetadata ActivatedType,
    GeneratorIgnored<LocationInfo?> Location
): ISourceCodeElement { }