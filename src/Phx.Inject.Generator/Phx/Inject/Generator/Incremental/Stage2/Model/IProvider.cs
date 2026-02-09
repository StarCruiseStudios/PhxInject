// -----------------------------------------------------------------------------
// <copyright file="IProvider.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;

namespace Phx.Inject.Generator.Incremental.Stage2.Model;

/// <summary>
/// Represents a provider (factory or builder) that can provide a qualified type.
/// </summary>
internal interface IProvider {
    /// <summary>
    /// The qualified type that this provider produces.
    /// </summary>
    QualifiedTypeMetadata ProvidedType { get; }
    
    /// <summary>
    /// The dependencies (parameters) that this provider requires.
    /// </summary>
    IReadOnlyList<QualifiedTypeMetadata> Dependencies { get; }
}
