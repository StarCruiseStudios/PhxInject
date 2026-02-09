// -----------------------------------------------------------------------------
// <copyright file="DependencyValidationResult.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;

namespace Phx.Inject.Generator.Incremental.Stage2.Model;

/// <summary>
/// Result of validating that all required dependencies are available.
/// </summary>
internal record DependencyValidationResult(
    bool IsValid,
    IReadOnlyList<MissingDependency> MissingDependencies
) {
    public static DependencyValidationResult Valid() {
        return new DependencyValidationResult(true, Array.Empty<MissingDependency>());
    }
    
    public static DependencyValidationResult Invalid(IReadOnlyList<MissingDependency> missingDependencies) {
        if (missingDependencies == null || missingDependencies.Count == 0) {
            throw new ArgumentException("Invalid result must have at least one missing dependency", nameof(missingDependencies));
        }
        return new DependencyValidationResult(false, missingDependencies);
    }
}

/// <summary>
/// Represents a dependency that is required but not available in the dependency graph.
/// </summary>
internal record MissingDependency(
    QualifiedTypeMetadata RequiredType,
    IProvider? RequiredBy
) {
    /// <summary>
    /// Creates a MissingDependency for a type required by a specific provider.
    /// </summary>
    public static MissingDependency FromProvider(QualifiedTypeMetadata requiredType, IProvider requiredBy) {
        return new MissingDependency(requiredType, requiredBy);
    }
    
    /// <summary>
    /// Creates a MissingDependency for a type that is directly required (not by a specific provider).
    /// </summary>
    public static MissingDependency FromRequirement(QualifiedTypeMetadata requiredType) {
        return new MissingDependency(requiredType, null);
    }
}
