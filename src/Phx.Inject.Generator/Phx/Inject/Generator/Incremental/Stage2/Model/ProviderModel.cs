// -----------------------------------------------------------------------------
// <copyright file="ProviderModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;

namespace Phx.Inject.Generator.Incremental.Stage2.Model;

/// <summary>
/// Domain model representing a provider method on the injector interface.
/// Providers are parameterless methods that return a type from the dependency graph.
/// </summary>
internal record ProviderModel(
    string ProviderMethodName,
    QualifiedTypeMetadata ProvidedType
);
