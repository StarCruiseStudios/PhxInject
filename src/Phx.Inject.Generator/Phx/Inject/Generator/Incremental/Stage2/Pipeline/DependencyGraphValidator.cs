// -----------------------------------------------------------------------------
// <copyright file="DependencyGraphValidator.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Stage2.Model;

namespace Phx.Inject.Generator.Incremental.Stage2.Pipeline;

/// <summary>
/// Validates that all required dependencies are available in the dependency graph.
/// </summary>
internal static class DependencyGraphValidator {
    
    /// <summary>
    /// Validates that all dependencies required by providers are available in the provider map.
    /// </summary>
    public static DependencyValidationResult Validate(QualifiedTypeProviderMap providerMap) {
        var missingDependencies = new List<MissingDependency>();
        
        foreach (var provider in providerMap.GetAllProviders()) {
            foreach (var dependency in provider.Dependencies) {
                if (!providerMap.HasProvider(dependency)) {
                    missingDependencies.Add(new MissingDependency(dependency, provider));
                }
            }
        }
        
        if (missingDependencies.Count > 0) {
            return DependencyValidationResult.Invalid(missingDependencies);
        }
        
        return DependencyValidationResult.Valid();
    }
    
    /// <summary>
    /// Validates that specific required types are available in the provider map.
    /// </summary>
    public static DependencyValidationResult ValidateRequiredTypes(
        QualifiedTypeProviderMap providerMap,
        IEnumerable<QualifiedTypeMetadata> requiredTypes) {
        
        var missingDependencies = new List<MissingDependency>();
        
        foreach (var requiredType in requiredTypes) {
            if (!providerMap.HasProvider(requiredType)) {
                // Create a synthetic provider to represent the requirement
                missingDependencies.Add(new MissingDependency(requiredType, null!));
            }
        }
        
        if (missingDependencies.Count > 0) {
            return DependencyValidationResult.Invalid(missingDependencies);
        }
        
        return DependencyValidationResult.Valid();
    }
}
