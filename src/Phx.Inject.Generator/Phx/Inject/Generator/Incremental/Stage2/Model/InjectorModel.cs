// -----------------------------------------------------------------------------
// <copyright file="InjectorModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;

namespace Phx.Inject.Generator.Incremental.Stage2.Model;

/// <summary>
/// The Stage 2 model for an injector, containing the complete dependency graph
/// and provider mappings needed for code generation.
/// </summary>
internal record InjectorModel(
    InjectorInterfaceMetadata InjectorMetadata,
    QualifiedTypeProviderMap ProviderMap,
    DependencyValidationResult ValidationResult,
    IReadOnlyList<QualifiedTypeMetadata> RequiredTypes
) {
    /// <summary>
    /// Gets all providers for a given qualified type.
    /// </summary>
    public IReadOnlyList<IProvider> GetProvidersFor(QualifiedTypeMetadata qualifiedType) {
        return ProviderMap.GetProviders(qualifiedType);
    }
    
    /// <summary>
    /// Checks if a qualified type can be provided.
    /// </summary>
    public bool CanProvide(QualifiedTypeMetadata qualifiedType) {
        return ProviderMap.HasProvider(qualifiedType);
    }
}
