// -----------------------------------------------------------------------------
// <copyright file="QualifiedTypeMapBuilder.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Auto;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Stage2.Model;

namespace Phx.Inject.Generator.Incremental.Stage2.Pipeline;

/// <summary>
/// Builds a map from qualified types to the providers (factories and builders) that can provide them.
/// </summary>
internal static class QualifiedTypeMapBuilder {
    
    /// <summary>
    /// Builds a provider map from specification class metadata.
    /// </summary>
    public static QualifiedTypeProviderMap BuildFromSpecification(SpecClassMetadata specMetadata) {
        var map = new QualifiedTypeProviderMap();
        
        // Add factory methods
        foreach (var factoryMethod in specMetadata.FactoryMethods) {
            map.AddProvider(new SpecFactoryMethodProvider(factoryMethod));
        }
        
        // Add factory properties
        foreach (var factoryProperty in specMetadata.FactoryProperties) {
            map.AddProvider(new SpecFactoryPropertyProvider(factoryProperty));
        }
        
        // Add factory references
        foreach (var factoryReference in specMetadata.FactoryReferences) {
            map.AddProvider(new SpecFactoryReferenceProvider(factoryReference));
        }
        
        // Add builder methods
        foreach (var builderMethod in specMetadata.BuilderMethods) {
            map.AddProvider(new SpecBuilderMethodProvider(builderMethod));
        }
        
        // Add builder references
        foreach (var builderReference in specMetadata.BuilderReferences) {
            map.AddProvider(new SpecBuilderReferenceProvider(builderReference));
        }
        
        return map;
    }
    
    /// <summary>
    /// Builds a provider map from specification interface metadata.
    /// </summary>
    public static QualifiedTypeProviderMap BuildFromSpecification(SpecInterfaceMetadata specMetadata) {
        var map = new QualifiedTypeProviderMap();
        
        // Add factory methods
        foreach (var factoryMethod in specMetadata.FactoryMethods) {
            map.AddProvider(new SpecFactoryMethodProvider(factoryMethod));
        }
        
        // Add factory properties
        foreach (var factoryProperty in specMetadata.FactoryProperties) {
            map.AddProvider(new SpecFactoryPropertyProvider(factoryProperty));
        }
        
        // Add factory references
        foreach (var factoryReference in specMetadata.FactoryReferences) {
            map.AddProvider(new SpecFactoryReferenceProvider(factoryReference));
        }
        
        // Add builder methods
        foreach (var builderMethod in specMetadata.BuilderMethods) {
            map.AddProvider(new SpecBuilderMethodProvider(builderMethod));
        }
        
        // Add builder references
        foreach (var builderReference in specMetadata.BuilderReferences) {
            map.AddProvider(new SpecBuilderReferenceProvider(builderReference));
        }
        
        return map;
    }
    
    /// <summary>
    /// Adds auto factories to the provider map.
    /// </summary>
    public static void AddAutoFactory(QualifiedTypeProviderMap map, AutoFactoryMetadata autoFactory) {
        map.AddProvider(new AutoFactoryProvider(autoFactory));
    }
    
    /// <summary>
    /// Adds auto builders to the provider map.
    /// </summary>
    public static void AddAutoBuilder(QualifiedTypeProviderMap map, AutoBuilderMetadata autoBuilder) {
        map.AddProvider(new AutoBuilderProvider(autoBuilder));
    }
    
    /// <summary>
    /// Merges multiple provider maps into a single map.
    /// </summary>
    public static QualifiedTypeProviderMap Merge(params QualifiedTypeProviderMap[] maps) {
        var result = new QualifiedTypeProviderMap();
        foreach (var map in maps) {
            foreach (var provider in map.GetAllProviders()) {
                result.AddProvider(provider);
            }
        }
        return result;
    }
}
