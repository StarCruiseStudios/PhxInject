// -----------------------------------------------------------------------------
// <copyright file="QualifiedTypeProviderMapTests.cs" company="Star Cruise Studios LLC">
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

public class QualifiedTypeProviderMapTests {
    [Test]
    public void AddProvider_ShouldAddProviderToMap() {
        // Arrange
        var map = new QualifiedTypeProviderMap();
        var qualifiedType = CreateQualifiedType("TestType");
        var provider = CreateFactoryProvider(qualifiedType);
        
        // Act
        map.AddProvider(provider);
        
        // Assert
        Assert.That(map.HasProvider(qualifiedType), Is.True);
        var providers = map.GetProviders(qualifiedType);
        Assert.That(providers, Has.Count.EqualTo(1));
        Assert.That(providers[0], Is.EqualTo(provider));
    }
    
    [Test]
    public void AddMultipleProviders_ShouldAllBeRetrievable() {
        // Arrange
        var map = new QualifiedTypeProviderMap();
        var qualifiedType = CreateQualifiedType("TestType");
        var provider1 = CreateFactoryProvider(qualifiedType);
        var provider2 = CreateFactoryProvider(qualifiedType);
        
        // Act
        map.AddProvider(provider1);
        map.AddProvider(provider2);
        
        // Assert
        var providers = map.GetProviders(qualifiedType);
        Assert.That(providers, Has.Count.EqualTo(2));
        Assert.That(providers, Contains.Item(provider1));
        Assert.That(providers, Contains.Item(provider2));
    }
    
    [Test]
    public void GetProviders_ForNonExistentType_ShouldReturnEmpty() {
        // Arrange
        var map = new QualifiedTypeProviderMap();
        var qualifiedType = CreateQualifiedType("NonExistent");
        
        // Act
        var providers = map.GetProviders(qualifiedType);
        
        // Assert
        Assert.That(providers, Is.Empty);
        Assert.That(map.HasProvider(qualifiedType), Is.False);
    }
    
    [Test]
    public void GetProvidedTypes_ShouldReturnAllTypes() {
        // Arrange
        var map = new QualifiedTypeProviderMap();
        var type1 = CreateQualifiedType("Type1");
        var type2 = CreateQualifiedType("Type2");
        map.AddProvider(CreateFactoryProvider(type1));
        map.AddProvider(CreateFactoryProvider(type2));
        
        // Act
        var providedTypes = map.GetProvidedTypes().ToList();
        
        // Assert
        Assert.That(providedTypes, Has.Count.EqualTo(2));
        Assert.That(providedTypes, Contains.Item(type1));
        Assert.That(providedTypes, Contains.Item(type2));
    }
    
    [Test]
    public void GetAllProviders_ShouldReturnAllProviders() {
        // Arrange
        var map = new QualifiedTypeProviderMap();
        var type1 = CreateQualifiedType("Type1");
        var type2 = CreateQualifiedType("Type2");
        var provider1 = CreateFactoryProvider(type1);
        var provider2 = CreateFactoryProvider(type2);
        map.AddProvider(provider1);
        map.AddProvider(provider2);
        
        // Act
        var allProviders = map.GetAllProviders().ToList();
        
        // Assert
        Assert.That(allProviders, Has.Count.EqualTo(2));
        Assert.That(allProviders, Contains.Item(provider1));
        Assert.That(allProviders, Contains.Item(provider2));
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
    
    private static IProvider CreateFactoryProvider(QualifiedTypeMetadata providedType) {
        var factoryMetadata = new SpecFactoryMethodMetadata(
            "TestFactory",
            providedType,
            Array.Empty<QualifiedTypeMetadata>(),
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
