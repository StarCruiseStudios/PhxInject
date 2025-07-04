// -----------------------------------------------------------------------------
//  <copyright file="SpecFactoryDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record SpecFactoryDesc(
    QualifiedTypeModel ReturnType,
    string FactoryMemberName,
    SpecFactoryMemberType SpecFactoryMemberType,
    IEnumerable<QualifiedTypeModel> Parameters,
    IEnumerable<SpecFactoryRequiredPropertyDesc> RequiredProperties,
    SpecFactoryMethodFabricationMode FabricationMode,
    bool isPartial,
    Location Location
) : IDescriptor {
    private static bool TryGetConstructorFactoryFabricationMode(
        ITypeSymbol constructorFactoryType,
        Location constructorFactoryLocation,
        ExtractorContext context,
        out SpecFactoryMethodFabricationMode fabricationMode
    ) {
        var factoryAttribute = constructorFactoryType.TryGetFactoryAttribute().GetOrThrow(context.GenerationContext);
        if (factoryAttribute == null) {
            // This is an implicit constructor factory.
            fabricationMode = SpecFactoryMethodFabricationMode.Recurrent;
            return false;
        }

        fabricationMode = MetadataHelpers.GetFactoryFabricationMode(
            factoryAttribute,
            constructorFactoryLocation,
            context.GenerationContext);
        return true;
    }

    private static bool TryGetFactoryFabricationMode(
        ISymbol factorySymbol,
        Location factoryLocation,
        ExtractorContext context,
        out SpecFactoryMethodFabricationMode fabricationMode
    ) {
        var factoryAttribute = factorySymbol.TryGetFactoryAttribute().GetOrThrow(context.GenerationContext);
        if (factoryAttribute == null) {
            // This is not a factory method.
            fabricationMode = SpecFactoryMethodFabricationMode.Recurrent;
            return false;
        }

        var factoryReferenceAttribute =
            factorySymbol.TryGetFactoryReferenceAttribute().GetOrThrow(context.GenerationContext);
        if (factoryReferenceAttribute != null) {
            // Cannot be a factory and a factory reference.
            throw new InjectionException(
                "Method or Property cannot have both Factory and FactoryReference attributes.",
                Diagnostics.InvalidSpecification,
                factoryLocation,
                context.GenerationContext);
        }

        fabricationMode = MetadataHelpers.GetFactoryFabricationMode(
            factoryAttribute,
            factoryLocation,
            context.GenerationContext);
        return true;
    }

    private static bool TryGetFactoryReferenceFabricationMode(
        ISymbol factoryReferenceSymbol,
        Location factoryReferenceLocation,
        ExtractorContext context,
        out SpecFactoryMethodFabricationMode fabricationMode
    ) {
        var factoryReferenceAttribute =
            factoryReferenceSymbol.TryGetFactoryReferenceAttribute().GetOrThrow(context.GenerationContext);

        if (factoryReferenceAttribute == null) {
            // This is not a factory reference.
            fabricationMode = SpecFactoryMethodFabricationMode.Recurrent;
            return false;
        }

        var factoryAttribute = factoryReferenceSymbol.TryGetFactoryAttribute().GetOrThrow(context.GenerationContext);
        if (factoryAttribute != null) {
            // Cannot be a factory and a factory reference.
            throw new InjectionException("Property or Field cannot have both Factory and FactoryReference attributes.",
                Diagnostics.InvalidSpecification,
                factoryReferenceLocation,
                context.GenerationContext);
        }

        fabricationMode = MetadataHelpers.GetFactoryFabricationMode(
            factoryReferenceAttribute,
            factoryReferenceLocation,
            context.GenerationContext);
        return true;
    }

    private static void GetFactoryReferenceTypes(
        ISymbol factoryReferenceSymbol,
        ITypeSymbol factoryReferenceTypeSymbol,
        Location factoryReferenceLocation,
        ExtractorContext context,
        out QualifiedTypeModel returnType,
        out IEnumerable<QualifiedTypeModel> parameterTypes
    ) {
        var referenceTypeSymbol = factoryReferenceTypeSymbol as INamedTypeSymbol;
        if (referenceTypeSymbol == null || referenceTypeSymbol.Name != "Func") {
            // Not the correct type to be a factory reference.
            throw new InjectionException("Factory reference must be a field or property of type Func<>.",
                Diagnostics.InvalidSpecification,
                factoryReferenceLocation,
                context.GenerationContext);
        }

        var typeArguments = referenceTypeSymbol.TypeArguments;

        var qualifier = MetadataHelpers.TryGetQualifier(factoryReferenceSymbol, context.GenerationContext)
            .GetOrThrow(context.GenerationContext);
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

    private static IResult<bool> GetIsPartial(ISymbol factorySymbol) {
        var partialAttributes = factorySymbol.TryGetPartialAttribute();
        return partialAttributes.Map(it => Result.Ok(it != null));
    }

    public interface IExtractor {
        SpecFactoryDesc ExtractAutoConstructorFactory(
            QualifiedTypeModel factoryType,
            ExtractorContext context
        );
        SpecFactoryDesc? ExtractFactory(
            IMethodSymbol factoryMethod,
            ExtractorContext context
        );
        SpecFactoryDesc? ExtractFactory(
            IPropertySymbol factoryProperty,
            ExtractorContext context
        );
        SpecFactoryDesc? ExtractFactoryReference(
            IPropertySymbol factoryReferenceProperty,
            ExtractorContext context
        );
        SpecFactoryDesc? ExtractFactoryReference(
            IFieldSymbol factoryReferenceField,
            ExtractorContext context
        );
    }

    public class Extractor : IExtractor {
        public SpecFactoryDesc ExtractAutoConstructorFactory(
            QualifiedTypeModel factoryType,
            ExtractorContext context
        ) {
            var factorySymbol = factoryType.TypeModel.typeSymbol;
            var factoryLocation = factorySymbol.Locations.First();
            TryGetConstructorFactoryFabricationMode(factorySymbol, factoryLocation, context, out var fabricationMode);

            var constructorParameterTypes =
                MetadataHelpers.TryGetConstructorParameterQualifiedTypes(factorySymbol, context.GenerationContext);
            var requiredProperties = MetadataHelpers
                .GetRequiredPropertyQualifiedTypes(factorySymbol, context.GenerationContext)
                .Select(property => new SpecFactoryRequiredPropertyDesc(property.Value, property.Key, factoryLocation));
            var qualifier = MetadataHelpers.TryGetQualifier(factorySymbol, context.GenerationContext)
                .GetOrThrow(context.GenerationContext);
            var returnType = factoryType with {
                Qualifier = qualifier
            };

            return new SpecFactoryDesc(
                returnType,
                factoryType.TypeModel.GetVariableName(),
                SpecFactoryMemberType.Constructor,
                constructorParameterTypes,
                requiredProperties,
                fabricationMode,
                false, // Constructor factories cannot be partial
                factoryLocation);
        }

        public SpecFactoryDesc? ExtractFactory(
            IMethodSymbol factoryMethod,
            ExtractorContext context) {
            var factoryLocation = factoryMethod.Locations.First();

            if (!TryGetFactoryFabricationMode(factoryMethod, factoryLocation, context, out var fabricationMode)) {
                // This is not a factory.
                return null;
            }

            var methodParameterTypes =
                MetadataHelpers.TryGetMethodParametersQualifiedTypes(factoryMethod, context.GenerationContext);

            var qualifier = MetadataHelpers.TryGetQualifier(factoryMethod, context.GenerationContext)
                .GetOrThrow(context.GenerationContext);
            var returnTypeModel = TypeModel.FromTypeSymbol(factoryMethod.ReturnType);
            var returnType = new QualifiedTypeModel(
                returnTypeModel,
                qualifier);

            var isPartial = GetIsPartial(factoryMethod).GetOrThrow(context.GenerationContext);
            TypeHelpers.ValidatePartialType(returnType, isPartial, factoryLocation, context.GenerationContext);

            return new SpecFactoryDesc(
                returnType,
                factoryMethod.Name,
                SpecFactoryMemberType.Method,
                methodParameterTypes,
                ImmutableList<SpecFactoryRequiredPropertyDesc>.Empty,
                fabricationMode,
                isPartial,
                factoryLocation);
        }

        public SpecFactoryDesc? ExtractFactory(
            IPropertySymbol factoryProperty,
            ExtractorContext context) {
            var factoryLocation = factoryProperty.Locations.First();

            if (!TryGetFactoryFabricationMode(factoryProperty, factoryLocation, context, out var fabricationMode)) {
                // This is not a factory.
                return null;
            }

            var methodParameterTypes = ImmutableList.Create<QualifiedTypeModel>();

            var qualifier = MetadataHelpers.TryGetQualifier(factoryProperty, context.GenerationContext)
                .GetOrThrow(context.GenerationContext);
            var returnTypeModel = TypeModel.FromTypeSymbol(factoryProperty.Type);
            var returnType = new QualifiedTypeModel(
                returnTypeModel,
                qualifier);

            var isPartial = GetIsPartial(factoryProperty).GetOrThrow(context.GenerationContext);
            TypeHelpers.ValidatePartialType(returnType, isPartial, factoryLocation, context.GenerationContext);

            return new SpecFactoryDesc(
                returnType,
                factoryProperty.Name,
                SpecFactoryMemberType.Property,
                methodParameterTypes,
                ImmutableList<SpecFactoryRequiredPropertyDesc>.Empty,
                fabricationMode,
                isPartial,
                factoryLocation);
        }

        public SpecFactoryDesc? ExtractFactoryReference(
            IPropertySymbol factoryReferenceProperty,
            ExtractorContext context) {
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
                context,
                out var returnType,
                out var parameterTypes);
            var isPartial = GetIsPartial(factoryReferenceProperty).GetOrThrow(context.GenerationContext);
            TypeHelpers.ValidatePartialType(returnType, isPartial, factoryReferenceLocation, context.GenerationContext);

            return new SpecFactoryDesc(
                returnType,
                factoryReferenceProperty.Name,
                SpecFactoryMemberType.Reference,
                parameterTypes,
                ImmutableList<SpecFactoryRequiredPropertyDesc>.Empty,
                fabricationMode,
                isPartial,
                factoryReferenceLocation);
        }

        public SpecFactoryDesc? ExtractFactoryReference(
            IFieldSymbol factoryReferenceField,
            ExtractorContext context) {
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
                context,
                out var returnType,
                out var parameterTypes);
            var isPartial = GetIsPartial(factoryReferenceField).GetOrThrow(context.GenerationContext);
            TypeHelpers.ValidatePartialType(returnType, isPartial, factoryReferenceLocation, context.GenerationContext);

            return new SpecFactoryDesc(
                returnType,
                factoryReferenceField.Name,
                SpecFactoryMemberType.Reference,
                parameterTypes,
                ImmutableList<SpecFactoryRequiredPropertyDesc>.Empty,
                fabricationMode,
                isPartial,
                factoryReferenceLocation);
        }
    }
}
