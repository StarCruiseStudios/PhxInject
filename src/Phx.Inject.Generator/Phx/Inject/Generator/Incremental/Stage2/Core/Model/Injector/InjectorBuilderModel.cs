// -----------------------------------------------------------------------------
// <copyright file="InjectorBuilderModel.cs" company="Star Cruise Studios LLC">
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
///     Model representing a builder method in an injector.
/// </summary>
/// <param name="BuiltType"> The type constructed by the builder. </param>
/// <param name="BuilderMethodName"> The name of the builder method. </param>
/// <param name="Location"> The source location where this builder is defined. </param>
internal record InjectorBuilderModel(
    QualifiedTypeMetadata BuiltType,
    string BuilderMethodName,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
