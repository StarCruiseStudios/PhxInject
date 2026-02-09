// -----------------------------------------------------------------------------
// <copyright file="Stage2ModelTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Auto;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Stage2.Model;

namespace Phx.Inject.Generator.Incremental.Stage2.Pipeline;

/// <summary>
/// Transforms Stage 1 metadata into Stage 2 models with dependency graph information.
/// </summary>
internal static class Stage2ModelTransformer {
    
    /// <summary>
    /// Transforms an injector and its associated specifications into a Stage 2 InjectorModel.
    /// </summary>
    public static InjectorModel Transform(
        InjectorInterfaceMetadata injectorMetadata,
        IEnumerable<SpecClassMetadata> specClasses,
        IEnumerable<SpecInterfaceMetadata> specInterfaces,
        IEnumerable<AutoFactoryMetadata> autoFactories,
        IEnumerable<AutoBuilderMetadata> autoBuilders) {
        
        // Build the provider map from all sources
        var providerMap = BuildProviderMap(specClasses, specInterfaces, autoFactories, autoBuilders);
        
        // Extract required types from the injector
        var requiredTypes = ExtractRequiredTypes(injectorMetadata);
        
        // Validate the dependency graph
        var validationResult = ValidateDependencies(providerMap, requiredTypes);
        
        return new InjectorModel(
            injectorMetadata,
            providerMap,
            validationResult,
            requiredTypes
        );
    }
    
    private static QualifiedTypeProviderMap BuildProviderMap(
        IEnumerable<SpecClassMetadata> specClasses,
        IEnumerable<SpecInterfaceMetadata> specInterfaces,
        IEnumerable<AutoFactoryMetadata> autoFactories,
        IEnumerable<AutoBuilderMetadata> autoBuilders) {
        
        var map = new QualifiedTypeProviderMap();
        
        // Add all providers from specification classes
        foreach (var specClass in specClasses) {
            var specMap = QualifiedTypeMapBuilder.BuildFromSpecification(specClass);
            map = QualifiedTypeMapBuilder.Merge(map, specMap);
        }
        
        // Add all providers from specification interfaces
        foreach (var specInterface in specInterfaces) {
            var specMap = QualifiedTypeMapBuilder.BuildFromSpecification(specInterface);
            map = QualifiedTypeMapBuilder.Merge(map, specMap);
        }
        
        // Add auto factories
        foreach (var autoFactory in autoFactories) {
            QualifiedTypeMapBuilder.AddAutoFactory(map, autoFactory);
        }
        
        // Add auto builders
        foreach (var autoBuilder in autoBuilders) {
            QualifiedTypeMapBuilder.AddAutoBuilder(map, autoBuilder);
        }
        
        return map;
    }
    
    private static IReadOnlyList<QualifiedTypeMetadata> ExtractRequiredTypes(
        InjectorInterfaceMetadata injectorMetadata) {
        
        var requiredTypes = new List<QualifiedTypeMetadata>();
        
        // Add types required by providers
        foreach (var provider in injectorMetadata.Providers) {
            requiredTypes.Add(provider.ProvidedType);
        }
        
        // Add types required by activators (builders)
        foreach (var activator in injectorMetadata.Activators) {
            requiredTypes.Add(activator.ActivatedType);
        }
        
        return requiredTypes;
    }
    
    private static DependencyValidationResult ValidateDependencies(
        QualifiedTypeProviderMap providerMap,
        IReadOnlyList<QualifiedTypeMetadata> requiredTypes) {
        
        // First validate that all providers have their dependencies available
        var providerValidation = DependencyGraphValidator.Validate(providerMap);
        
        // Then validate that all required types are available
        var requiredTypesValidation = DependencyGraphValidator.ValidateRequiredTypes(
            providerMap, 
            requiredTypes);
        
        // Combine validation results
        if (!providerValidation.IsValid || !requiredTypesValidation.IsValid) {
            var allMissing = providerValidation.MissingDependencies
                .Concat(requiredTypesValidation.MissingDependencies)
                .ToList();
            return DependencyValidationResult.Invalid(allMissing);
        }
        
        return DependencyValidationResult.Valid();
    }
}
