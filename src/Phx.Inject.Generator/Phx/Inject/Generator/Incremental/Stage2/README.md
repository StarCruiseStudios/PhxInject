# Stage 2: Dependency Graph Model

Stage 2 of the code generation pipeline builds a comprehensive model of the dependency graph, mapping qualified types to the providers (factories and builders) that can provide them, and validates that all required dependencies are available.

## Overview

Stage 2 transforms the raw metadata from Stage 1 into a structured model that:
1. Maps qualified types to their providers
2. Validates the dependency graph for completeness
3. Prepares data structures needed for Stage 3 code generation

## Architecture

### Model Classes (`Stage2/Model/`)

#### Core Interfaces
- **`IProvider`**: Base interface for all providers (factories and builders)
  - `ProvidedType`: The qualified type this provider produces
  - `Dependencies`: The types this provider requires

#### Factory Providers
- **`FactoryProvider`**: Abstract base for factory-type providers
  - **`SpecFactoryMethodProvider`**: Factory method from specification
  - **`SpecFactoryPropertyProvider`**: Factory property from specification
  - **`SpecFactoryReferenceProvider`**: Factory reference (Func<>) from specification
  - **`AutoFactoryProvider`**: Auto-generated factory from annotated class
  - **`LinkProvider`**: Link-based type mapping (implicit factory)

#### Builder Providers
- **`BuilderProvider`**: Abstract base for builder-type providers
  - **`SpecBuilderMethodProvider`**: Builder method from specification
  - **`SpecBuilderReferenceProvider`**: Builder reference (Action<>) from specification
  - **`AutoBuilderProvider`**: Auto-generated builder from annotated method

#### Provider Map
- **`QualifiedTypeProviderMap`**: Maps qualified types to providers
  - `AddProvider(IProvider)`: Adds a provider to the map
  - `GetProviders(QualifiedTypeMetadata)`: Gets all providers for a type
  - `HasProvider(QualifiedTypeMetadata)`: Checks if a type has providers
  - `GetProvidedTypes()`: Returns all types that have providers
  - `GetAllProviders()`: Returns all providers in the map

#### Validation
- **`DependencyValidationResult`**: Result of dependency graph validation
  - `IsValid`: Whether all dependencies are available
  - `MissingDependencies`: List of missing dependencies
  
- **`MissingDependency`**: Represents a missing dependency
  - `RequiredType`: The type that is missing
  - `RequiredBy`: The provider that requires it (null if it's a direct requirement)
  - Factory methods:
    - `FromProvider(type, provider)`: For dependencies required by a provider
    - `FromRequirement(type)`: For directly required types

#### Injector Model
- **`InjectorModel`**: Root Stage 2 model
  - `InjectorMetadata`: Stage 1 injector interface metadata
  - `ProviderMap`: Map of qualified types to providers
  - `ValidationResult`: Result of dependency graph validation
  - `RequiredTypes`: Types that must be provided by the injector

### Pipeline Transformers (`Stage2/Pipeline/`)

#### QualifiedTypeMapBuilder
Builds provider maps from Stage 1 metadata:
- `BuildFromSpecification(SpecClassMetadata)`: Builds map from specification class
- `BuildFromSpecification(SpecInterfaceMetadata)`: Builds map from specification interface
- `BuildFromInjectorDependency(InjectorDependencyInterfaceMetadata)`: Builds map from injector dependencies
- `AddAutoFactory(map, AutoFactoryMetadata)`: Adds auto factory to map
- `AddAutoBuilder(map, AutoBuilderMetadata)`: Adds auto builder to map
- `AddLink(map, LinkAttributeMetadata)`: Adds link to map
- `Merge(maps...)`: Merges multiple maps

#### DependencyGraphValidator
Validates dependency graph completeness:
- `Validate(QualifiedTypeProviderMap)`: Validates all provider dependencies are available
- `ValidateRequiredTypes(map, requiredTypes)`: Validates specific required types are available

#### Stage2ModelTransformer
Transforms Stage 1 metadata into Stage 2 models:
- `Transform(injector, specs, dependencies, autoFactories, autoBuilders)`: Creates InjectorModel
  - Builds provider map from all sources
  - Extracts required types from injector
  - Validates dependency graph

## Usage Example

```csharp
// Build provider map from Stage 1 metadata
var providerMap = QualifiedTypeMapBuilder.BuildFromSpecification(specClassMetadata);

// Validate dependencies
var validationResult = DependencyGraphValidator.Validate(providerMap);
if (!validationResult.IsValid) {
    foreach (var missing in validationResult.MissingDependencies) {
        Console.WriteLine($"Missing: {missing.RequiredType}");
    }
}

// Transform to InjectorModel
var injectorModel = Stage2ModelTransformer.Transform(
    injectorMetadata,
    specClasses,
    specInterfaces,
    injectorDependencies,
    autoFactories,
    autoBuilders
);

// Use the model
foreach (var providedType in injectorModel.ProviderMap.GetProvidedTypes()) {
    var providers = injectorModel.GetProvidersFor(providedType);
    Console.WriteLine($"Type {providedType} has {providers.Count} provider(s)");
}
```

## Features

### Qualified Type Support
- Full support for type qualifiers (labels and custom qualifiers)
- Distinguishes between `int` and `[Label("X")] int`
- Handles complex qualifier scenarios

### Provider Types
Supports all provider types from the framework:
- Factory methods and properties
- Factory references (Func<>)
- Builder methods
- Builder references (Action<>)
- Auto factories (from annotated classes)
- Auto builders (from annotated methods)
- Links (implicit type conversions)

### Dependency Validation
- Validates that all provider dependencies are satisfied
- Validates that all injector requirements can be fulfilled
- Provides detailed diagnostic information for missing dependencies
- Distinguishes between provider dependencies and direct requirements

### Extensibility
- Clean separation between model and pipeline
- Easy to add new provider types
- Pluggable validation logic

## Testing

The Stage 2 implementation includes comprehensive unit tests:
- **QualifiedTypeProviderMapTests** (5 tests)
  - Adding providers
  - Retrieving providers
  - Handling multiple providers for same type
  - Handling non-existent types
  
- **DependencyGraphValidatorTests** (5 tests)
  - Validating with no dependencies
  - Validating satisfied dependencies
  - Detecting missing dependencies
  - Validating required types
  - Detecting missing required types

All tests use NUnit and follow the existing test patterns in the project.

## Integration

Stage 2 is currently integrated into `IncrementalSourceGenerator` for specification classes:
- Demonstrates building provider maps
- Shows provider information in generated code comments
- Validates the Stage 2 model works correctly with real metadata

Future work will involve:
- Full integration for injector generation
- Using Stage 2 model for actual code generation (Stage 3)
- Reporting validation errors as Roslyn diagnostics

## Files

**Model/** (7 files):
- `IProvider.cs`
- `FactoryProvider.cs`
- `BuilderProvider.cs`
- `LinkProvider.cs`
- `QualifiedTypeProviderMap.cs`
- `DependencyValidationResult.cs`
- `InjectorModel.cs`

**Pipeline/** (3 files):
- `QualifiedTypeMapBuilder.cs`
- `DependencyGraphValidator.cs`
- `Stage2ModelTransformer.cs`

**Total**: 10 files, ~600 lines of code, 10 unit tests
