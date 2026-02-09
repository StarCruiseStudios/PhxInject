// -----------------------------------------------------------------------------
// <copyright file="DependencyGraphValidatorTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Stage2.Model;
using Phx.Inject.Generator.Incremental.Stage2.Pipeline;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Tests.Stage2;

public class DependencyGraphValidatorTests {
    [Test]
    public void Validate_WithNoDependencies_ShouldReturnValid() {
        // Arrange
        var map = new QualifiedTypeProviderMap();
        var type = CreateQualifiedType("TestType");
        var provider = CreateFactoryProvider(type, Array.Empty<QualifiedTypeMetadata>());
        map.AddProvider(provider);
        
        // Act
        var result = DependencyGraphValidator.Validate(map);
        
        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.MissingDependencies, Is.Empty);
    }
    
    [Test]
    public void Validate_WithSatisfiedDependencies_ShouldReturnValid() {
        // Arrange
        var map = new QualifiedTypeProviderMap();
        var dependencyType = CreateQualifiedType("DependencyType");
        var providedType = CreateQualifiedType("ProvidedType");
        
        // Add provider for dependency
        var dependencyProvider = CreateFactoryProvider(dependencyType, Array.Empty<QualifiedTypeMetadata>());
        map.AddProvider(dependencyProvider);
        
        // Add provider that depends on the dependency
        var mainProvider = CreateFactoryProvider(providedType, new[] { dependencyType });
        map.AddProvider(mainProvider);
        
        // Act
        var result = DependencyGraphValidator.Validate(map);
        
        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.MissingDependencies, Is.Empty);
    }
    
    [Test]
    public void Validate_WithMissingDependency_ShouldReturnInvalid() {
        // Arrange
        var map = new QualifiedTypeProviderMap();
        var missingType = CreateQualifiedType("MissingType");
        var providedType = CreateQualifiedType("ProvidedType");
        
        // Add provider that depends on a missing type
        var provider = CreateFactoryProvider(providedType, new[] { missingType });
        map.AddProvider(provider);
        
        // Act
        var result = DependencyGraphValidator.Validate(map);
        
        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.MissingDependencies, Has.Count.EqualTo(1));
        Assert.That(result.MissingDependencies[0].RequiredType, Is.EqualTo(missingType));
        Assert.That(result.MissingDependencies[0].RequiredBy, Is.EqualTo(provider));
    }
    
    [Test]
    public void ValidateRequiredTypes_WithAllTypesAvailable_ShouldReturnValid() {
        // Arrange
        var map = new QualifiedTypeProviderMap();
        var type1 = CreateQualifiedType("Type1");
        var type2 = CreateQualifiedType("Type2");
        map.AddProvider(CreateFactoryProvider(type1, Array.Empty<QualifiedTypeMetadata>()));
        map.AddProvider(CreateFactoryProvider(type2, Array.Empty<QualifiedTypeMetadata>()));
        
        // Act
        var result = DependencyGraphValidator.ValidateRequiredTypes(map, new[] { type1, type2 });
        
        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.MissingDependencies, Is.Empty);
    }
    
    [Test]
    public void ValidateRequiredTypes_WithMissingRequiredType_ShouldReturnInvalid() {
        // Arrange
        var map = new QualifiedTypeProviderMap();
        var availableType = CreateQualifiedType("AvailableType");
        var missingType = CreateQualifiedType("MissingType");
        map.AddProvider(CreateFactoryProvider(availableType, Array.Empty<QualifiedTypeMetadata>()));
        
        // Act
        var result = DependencyGraphValidator.ValidateRequiredTypes(map, new[] { availableType, missingType });
        
        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.MissingDependencies, Has.Count.EqualTo(1));
        Assert.That(result.MissingDependencies[0].RequiredType, Is.EqualTo(missingType));
    }
    
    private static QualifiedTypeMetadata CreateQualifiedType(string typeName) {
        var typeMetadata = new TypeMetadata(
            "TestNamespace",
            typeName,
            Array.Empty<TypeMetadata>(),
            Location.None.GeneratorIgnored()
        );
        return new QualifiedTypeMetadata(typeMetadata, NoQualifierMetadata.Instance);
    }
    
    private static IProvider CreateFactoryProvider(
        QualifiedTypeMetadata providedType,
        IEnumerable<QualifiedTypeMetadata> dependencies) {
        
        var factoryMetadata = new SpecFactoryMethodMetadata(
            "TestFactory",
            providedType,
            dependencies,
            new FactoryAttributeMetadata(
                FabricationMode.Recurrent,
                new AttributeMetadata(
                    "TestAttribute",
                    "TestTarget",
                    Location.None.GeneratorIgnored(),
                    Location.None.GeneratorIgnored()
                )
            ),
            null,
            Location.None.GeneratorIgnored()
        );
        return new SpecFactoryMethodProvider(factoryMetadata);
    }
}
