// -----------------------------------------------------------------------------
// <copyright file="QualifiedTypeProviderMap.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;

namespace Phx.Inject.Generator.Incremental.Stage2.Model;

/// <summary>
/// Maps qualified types to the providers (factories and builders) that can provide them.
/// </summary>
internal class QualifiedTypeProviderMap {
    private readonly Dictionary<QualifiedTypeMetadata, List<IProvider>> _providersByType = new();
    
    /// <summary>
    /// Adds a provider to the map.
    /// </summary>
    public void AddProvider(IProvider provider) {
        if (!_providersByType.ContainsKey(provider.ProvidedType)) {
            _providersByType[provider.ProvidedType] = new List<IProvider>();
        }
        _providersByType[provider.ProvidedType].Add(provider);
    }
    
    /// <summary>
    /// Gets all providers for a given qualified type.
    /// </summary>
    public IReadOnlyList<IProvider> GetProviders(QualifiedTypeMetadata qualifiedType) {
        return _providersByType.TryGetValue(qualifiedType, out var providers) 
            ? providers 
            : Array.Empty<IProvider>();
    }
    
    /// <summary>
    /// Checks if there is at least one provider for the given qualified type.
    /// </summary>
    public bool HasProvider(QualifiedTypeMetadata qualifiedType) {
        return _providersByType.ContainsKey(qualifiedType) && _providersByType[qualifiedType].Count > 0;
    }
    
    /// <summary>
    /// Gets all qualified types that have at least one provider.
    /// </summary>
    public IEnumerable<QualifiedTypeMetadata> GetProvidedTypes() {
        return _providersByType.Keys;
    }
    
    /// <summary>
    /// Gets all providers in the map.
    /// </summary>
    public IEnumerable<IProvider> GetAllProviders() {
        return _providersByType.Values.SelectMany(providers => providers);
    }
}
