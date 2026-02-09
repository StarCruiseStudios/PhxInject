// -----------------------------------------------------------------------------
// <copyright file="QualifiedTypeMapBuilder.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Auto;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
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
        return BuildFromSpecificationCommon(
            specMetadata.FactoryMethods,
            specMetadata.FactoryProperties,
            specMetadata.FactoryReferences,
            specMetadata.BuilderMethods,
            specMetadata.BuilderReferences,
            specMetadata.Links
        );
    }
    
    /// <summary>
    /// Builds a provider map from specification interface metadata.
    /// </summary>
    public static QualifiedTypeProviderMap BuildFromSpecification(SpecInterfaceMetadata specMetadata) {
        return BuildFromSpecificationCommon(
            specMetadata.FactoryMethods,
            specMetadata.FactoryProperties,
            specMetadata.FactoryReferences,
            specMetadata.BuilderMethods,
            specMetadata.BuilderReferences,
            specMetadata.Links
        );
    }
    
    /// <summary>
    /// Common logic for building provider maps from specification metadata.
    /// </summary>
    private static QualifiedTypeProviderMap BuildFromSpecificationCommon(
        IEnumerable<SpecFactoryMethodMetadata> factoryMethods,
        IEnumerable<SpecFactoryPropertyMetadata> factoryProperties,
        IEnumerable<SpecFactoryReferenceMetadata> factoryReferences,
        IEnumerable<SpecBuilderMethodMetadata> builderMethods,
        IEnumerable<SpecBuilderReferenceMetadata> builderReferences,
        IEnumerable<LinkAttributeMetadata> links) {
        
        var map = new QualifiedTypeProviderMap();
        
        // Add factory methods
        foreach (var factoryMethod in factoryMethods) {
            map.AddProvider(new SpecFactoryMethodProvider(factoryMethod));
        }
        
        // Add factory properties
        foreach (var factoryProperty in factoryProperties) {
            map.AddProvider(new SpecFactoryPropertyProvider(factoryProperty));
        }
        
        // Add factory references
        foreach (var factoryReference in factoryReferences) {
            map.AddProvider(new SpecFactoryReferenceProvider(factoryReference));
        }
        
        // Add builder methods
        foreach (var builderMethod in builderMethods) {
            map.AddProvider(new SpecBuilderMethodProvider(builderMethod));
        }
        
        // Add builder references
        foreach (var builderReference in builderReferences) {
            map.AddProvider(new SpecBuilderReferenceProvider(builderReference));
        }
        
        // Add links
        foreach (var link in links) {
            AddLink(map, link);
        }
        
        return map;
    }
    
    /// <summary>
    /// Builds a provider map from injector dependency interface metadata.
    /// </summary>
    public static QualifiedTypeProviderMap BuildFromInjectorDependency(InjectorDependencyInterfaceMetadata injectorDependency) {
        var map = new QualifiedTypeProviderMap();
        
        // Add factory methods
        foreach (var factoryMethod in injectorDependency.FactoryMethods) {
            map.AddProvider(new SpecFactoryMethodProvider(factoryMethod));
        }
        
        // Add factory properties
        foreach (var factoryProperty in injectorDependency.FactoryProperties) {
            map.AddProvider(new SpecFactoryPropertyProvider(factoryProperty));
        }
        
        return map;
    }
    
    /// <summary>
    /// Adds a link to the provider map.
    /// </summary>
    public static void AddLink(QualifiedTypeProviderMap map, LinkAttributeMetadata link) {
        // Create qualified types for input and output based on the link metadata
        var inputQualifier = link.InputQualifier != null
            ? (IQualifierMetadata)new CustomQualifierMetadata(
                new QualifierAttributeMetadata(link.InputQualifier, link.AttributeMetadata))
            : link.InputLabel != null
                ? new LabelQualifierMetadata(
                    new LabelAttributeMetadata(link.InputLabel, link.AttributeMetadata))
                : NoQualifierMetadata.Instance;
        
        var outputQualifier = link.OutputQualifier != null
            ? (IQualifierMetadata)new CustomQualifierMetadata(
                new QualifierAttributeMetadata(link.OutputQualifier, link.AttributeMetadata))
            : link.OutputLabel != null
                ? new LabelQualifierMetadata(
                    new LabelAttributeMetadata(link.OutputLabel, link.AttributeMetadata))
                : NoQualifierMetadata.Instance;
        
        var inputType = new QualifiedTypeMetadata(link.Input, inputQualifier);
        var outputType = new QualifiedTypeMetadata(link.Output, outputQualifier);
        
        map.AddProvider(new LinkProvider(link, inputType, outputType));
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
