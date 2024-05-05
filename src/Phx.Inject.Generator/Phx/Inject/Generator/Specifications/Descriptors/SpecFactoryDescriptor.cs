﻿// -----------------------------------------------------------------------------
//  <copyright file="SpecFactoryDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Specifications.Descriptors {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Descriptors;

    internal delegate SpecFactoryDescriptor? CreateSpecFactoryMethodDescriptor(
            IMethodSymbol factoryMethod,
            DescriptorGenerationContext context
    );

    internal delegate SpecFactoryDescriptor? CreateSpecFactoryPropertyDescriptor(
            IPropertySymbol factoryProperty,
            DescriptorGenerationContext context
    );

    internal delegate SpecFactoryDescriptor? CreateSpecFactoryReferencePropertyDescriptor(
            IPropertySymbol factoryReferenceProperty,
            DescriptorGenerationContext context
    );

    internal delegate SpecFactoryDescriptor? CreateSpecFactoryReferenceFieldDescriptor(
            IFieldSymbol factoryReferenceField,
            DescriptorGenerationContext context
    );

    internal record SpecFactoryDescriptor(
            QualifiedTypeModel ReturnType,
            string FactoryMemberName,
            SpecFactoryMemberType SpecFactoryMemberType,
            IEnumerable<QualifiedTypeModel> Parameters,
            SpecFactoryMethodFabricationMode FabricationMode,
            Location Location
    ) : IDescriptor {
        public class Builder {
            public SpecFactoryDescriptor? BuildFactory(
                    IMethodSymbol factoryMethod,
                    DescriptorGenerationContext context) {
                var factoryLocation = factoryMethod.Locations.First();

                if (!TryGetFactoryFabricationMode(factoryMethod, factoryLocation, context, out var fabricationMode)) {
                    // This is not a factory.
                    return null;
                }

                var methodParameterTypes = MetadataHelpers.GetMethodParametersQualifiedTypes(factoryMethod);

                var qualifier = MetadataHelpers.GetQualifier(factoryMethod);
                var returnTypeModel = TypeModel.FromTypeSymbol(factoryMethod.ReturnType);
                var returnType = new QualifiedTypeModel(
                        returnTypeModel,
                        qualifier);

                return new SpecFactoryDescriptor(
                        returnType,
                        factoryMethod.Name,
                        SpecFactoryMemberType.Method,
                        methodParameterTypes,
                        fabricationMode,
                        factoryLocation);
            }

            public SpecFactoryDescriptor? BuildFactory(
                    IPropertySymbol factoryProperty,
                    DescriptorGenerationContext context) {
                var factoryLocation = factoryProperty.Locations.First();

                if (!TryGetFactoryFabricationMode(factoryProperty, factoryLocation, context, out var fabricationMode)) {
                    // This is not a factory.
                    return null;
                }

                var methodParameterTypes = ImmutableList.Create<QualifiedTypeModel>();

                var qualifier = MetadataHelpers.GetQualifier(factoryProperty);
                var returnTypeModel = TypeModel.FromTypeSymbol(factoryProperty.Type);
                var returnType = new QualifiedTypeModel(
                        returnTypeModel,
                        qualifier);

                return new SpecFactoryDescriptor(
                        returnType,
                        factoryProperty.Name,
                        SpecFactoryMemberType.Property,
                        methodParameterTypes,
                        fabricationMode,
                        factoryLocation);
            }

            public SpecFactoryDescriptor? BuildFactoryReference(
                    IPropertySymbol factoryReferenceProperty,
                    DescriptorGenerationContext context) {
                var factoryReferenceLocation = factoryReferenceProperty.Locations.First();
                if (!TryGetFactoryReferenceFabricationMode(factoryReferenceProperty,
                        factoryReferenceLocation,
                        context,
                        out var fabricationMode)) {
                    // This is not a factory reference.
                    return null;
                }

                GetFactoryReferenceTypes(
                        factoryReferenceProperty,
                        factoryReferenceProperty.Type,
                        factoryReferenceLocation,
                        out var returnType,
                        out var parameterTypes);

                return new SpecFactoryDescriptor(
                        returnType,
                        factoryReferenceProperty.Name,
                        SpecFactoryMemberType.Reference,
                        parameterTypes,
                        fabricationMode,
                        factoryReferenceLocation);
            }

            public SpecFactoryDescriptor? BuildFactoryReference(
                    IFieldSymbol factoryReferenceField,
                    DescriptorGenerationContext context) {
                var factoryReferenceLocation = factoryReferenceField.Locations.First();
                if (!TryGetFactoryReferenceFabricationMode(factoryReferenceField,
                        factoryReferenceLocation,
                        context,
                        out var fabricationMode)) {
                    // This is not a factory reference.
                    return null;
                }

                GetFactoryReferenceTypes(
                        factoryReferenceField,
                        factoryReferenceField.Type,
                        factoryReferenceLocation,
                        out var returnType,
                        out var parameterTypes);

                return new SpecFactoryDescriptor(
                        returnType,
                        factoryReferenceField.Name,
                        SpecFactoryMemberType.Reference,
                        parameterTypes,
                        fabricationMode,
                        factoryReferenceLocation);
            }
        }

        private static bool TryGetFactoryFabricationMode(
                ISymbol factorySymbol,
                Location factoryLocation,
                DescriptorGenerationContext context,
                out SpecFactoryMethodFabricationMode fabricationMode
        ) {
            var factoryAttributes = factorySymbol.GetFactoryAttributes();
            var numFactoryAttributes = factoryAttributes.Count;
            if (numFactoryAttributes == 0) {
                // This is not a factory method.
                fabricationMode = SpecFactoryMethodFabricationMode.Recurrent;
                return false;
            }

            if (numFactoryAttributes > 1) {
                throw new InjectionException(
                        Diagnostics.InvalidSpecification,
                        "Method or Property can only have a single factory attribute.",
                        factoryLocation);
            }

            var factoryReferenceAttributes = factorySymbol.GetFactoryReferenceAttributes();
            if (factoryReferenceAttributes.Count > 0) {
                // Cannot be a factory and a factory reference.
                throw new InjectionException(
                        Diagnostics.InvalidSpecification,
                        "Method or Property cannot have both Factory and FactoryReference attributes.",
                        factoryLocation);
            }

            var factoryAttribute = factoryAttributes.Single();
            fabricationMode = MetadataHelpers.GetFactoryFabricationMode(
                    factoryAttribute,
                    factoryLocation);
            return true;
        }

        private static bool TryGetFactoryReferenceFabricationMode(
                ISymbol factoryReferenceSymbol,
                Location factoryReferenceLocation,
                DescriptorGenerationContext context,
                out SpecFactoryMethodFabricationMode fabricationMode
        ) {
            var factoryReferenceAttributes = factoryReferenceSymbol.GetFactoryReferenceAttributes();
            var numFactoryReferenceAttributes = factoryReferenceAttributes.Count;
            if (numFactoryReferenceAttributes == 0) {
                // This is not a factory reference.
                fabricationMode = SpecFactoryMethodFabricationMode.Recurrent;
                return false;
            }

            if (numFactoryReferenceAttributes > 1) {
                throw new InjectionException(
                        Diagnostics.InvalidSpecification,
                        "Property or Field can only have a single factory reference attribute.",
                        factoryReferenceLocation);
            }

            var factoryAttributes = factoryReferenceSymbol.GetFactoryAttributes();
            if (factoryAttributes.Count > 0) {
                // Cannot be a factory and a factory reference.
                throw new InjectionException(
                        Diagnostics.InvalidSpecification,
                        "Property or Field cannot have both Factory and FactoryReference attributes.",
                        factoryReferenceLocation);
            }

            var factoryReferenceAttribute = factoryReferenceAttributes.Single();
            fabricationMode = MetadataHelpers.GetFactoryFabricationMode(
                    factoryReferenceAttribute,
                    factoryReferenceLocation);
            return true;
        }

        private static void GetFactoryReferenceTypes(
                ISymbol factoryReferenceSymbol,
                ITypeSymbol factoryReferenceTypeSymbol,
                Location factoryReferenceLocation,
                out QualifiedTypeModel returnType,
                out IEnumerable<QualifiedTypeModel> parameterTypes
        ) {
            var referenceTypeSymbol = factoryReferenceTypeSymbol as INamedTypeSymbol;
            if (referenceTypeSymbol == null || referenceTypeSymbol.Name != "Func") {
                // Not the correct type to be a factory reference.
                throw new InjectionException(
                        Diagnostics.InvalidSpecification,
                        "Factory reference must be a field or property of type Func<>.",
                        factoryReferenceLocation);
            }

            var typeArguments = referenceTypeSymbol.TypeArguments;

            var qualifier = MetadataHelpers.GetQualifier(factoryReferenceSymbol);
            var returnTypeModel = TypeModel.FromTypeSymbol(typeArguments[typeArguments.Length - 1]);
            returnType = new QualifiedTypeModel(
                    returnTypeModel,
                    qualifier);

            parameterTypes = typeArguments.Length == 1
                    ? ImmutableList.Create<QualifiedTypeModel>()
                    : typeArguments.Take(typeArguments.Length - 1)
                            .Select(typeArgument => TypeModel.FromTypeSymbol(typeArgument))
                            .Select(typeModel => new QualifiedTypeModel(typeModel, QualifiedTypeModel.NoQualifier))
                            .ToImmutableList();
        }
    }
}
