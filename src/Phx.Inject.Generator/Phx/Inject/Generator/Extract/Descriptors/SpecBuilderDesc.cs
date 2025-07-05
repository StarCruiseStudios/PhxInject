// -----------------------------------------------------------------------------
//  <copyright file="SpecBuilderDescriptor.cs" company="Star Cruise Studios LLC">
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

internal record SpecBuilderDesc(
    QualifiedTypeModel BuiltType,
    string BuilderMemberName,
    SpecBuilderMemberType SpecBuilderMemberType,
    IEnumerable<QualifiedTypeModel> Parameters,
    Location Location
) : IDescriptor {
    public interface IExtractor {
        SpecBuilderDesc? ExtractBuilder(IMethodSymbol builderMethod, ExtractorContext context);
        SpecBuilderDesc ExtractAutoBuilder(QualifiedTypeModel builderType, ExtractorContext context);
        SpecBuilderDesc? ExtractBuilderReference(IPropertySymbol builderProperty, ExtractorContext context);
        SpecBuilderDesc? ExtractBuilderReference(IFieldSymbol builderField, ExtractorContext context);
    }

    public class Extractor : IExtractor {
        public SpecBuilderDesc? ExtractBuilder(IMethodSymbol builderMethod, ExtractorContext context) {
            var builderLocation = builderMethod.Locations.First();

            if (!ValidateBuilder(builderMethod, builderLocation, context)) {
                // This is not a builder method.
                return null;
            }

            var methodParameterTypes =
                MetadataHelpers.TryGetMethodParametersQualifiedTypes(builderMethod, context);
            if (methodParameterTypes.Count == 0) {
                throw Diagnostics.InvalidSpecification.AsException(
                    "Builder method must have at least one parameter.",
                    builderLocation,
                    context);
            }

            var qualifier = MetadataHelpers.TryGetQualifier(builderMethod)
                .GetOrThrow(context);
            // Use the qualifier from the method, not the parameter.
            var builtType = methodParameterTypes[0] with {
                Qualifier = qualifier
            };
            IReadOnlyList<QualifiedTypeModel> builderArguments = methodParameterTypes.Count > 1
                ? methodParameterTypes.Skip(1).ToImmutableList()
                : ImmutableList.Create<QualifiedTypeModel>();

            return new SpecBuilderDesc(
                builtType,
                builderMethod.Name,
                SpecBuilderMemberType.Method,
                builderArguments,
                builderLocation);
        }

        public SpecBuilderDesc ExtractAutoBuilder(QualifiedTypeModel builderType, ExtractorContext context) {
            var builderLocation = builderType.TypeModel.typeSymbol.Locations.First();
            IReadOnlyList<IMethodSymbol> builderMethods = MetadataHelpers
                .GetDirectBuilderMethods(builderType.TypeModel.typeSymbol, context)
                .Where(b => MetadataHelpers.TryGetQualifier(b)
                        .GetOrThrow(context)
                    == builderType.Qualifier)
                .Where(b => ValidateBuilder(b, builderLocation, context))
                .ToImmutableList();

            var numBuilderMethods = builderMethods.Count;
            if (numBuilderMethods == 0) {
                throw Diagnostics.InvalidSpecification.AsException(
                    "No direct builder method found for required builder type: " + builderType,
                    builderLocation,
                    context);
            }

            if (numBuilderMethods > 1) {
                throw Diagnostics.InvalidSpecification.AsException(
                    "More than one direct builder method found for type: " + builderType,
                    builderLocation,
                    context);
            }

            var builderMethod = builderMethods.First();
            var methodParameterTypes =
                MetadataHelpers.TryGetMethodParametersQualifiedTypes(builderMethod, context);
            if (methodParameterTypes.Count == 0) {
                throw Diagnostics.InvalidSpecification.AsException(
                    "Builder method must have at least one parameter.",
                    builderLocation,
                    context);
            }

            if (methodParameterTypes[0].TypeModel != builderType.TypeModel) {
                throw Diagnostics.InvalidSpecification.AsException(
                    "Direct builder method must accept a first parameter of the built type.",
                    builderLocation,
                    context);
            }

            IReadOnlyList<QualifiedTypeModel> builderArguments = methodParameterTypes.Count > 1
                ? methodParameterTypes.Skip(1).ToImmutableList()
                : ImmutableList.Create<QualifiedTypeModel>();

            return new SpecBuilderDesc(
                builderType,
                builderMethod.Name,
                SpecBuilderMemberType.Direct,
                builderArguments,
                builderLocation);
        }

        public SpecBuilderDesc? ExtractBuilderReference(IPropertySymbol builderProperty, ExtractorContext context) {
            var builderReferenceLocation = builderProperty.Locations.First();

            if (!ValidateBuilderReference(builderProperty, builderReferenceLocation, context)) {
                // This is not a builder reference.
                return null;
            }

            GetBuilderReferenceTypes(builderProperty,
                builderProperty.Type,
                builderReferenceLocation,
                context,
                out var builtType,
                out var parameterTypes);

            return new SpecBuilderDesc(
                builtType,
                builderProperty.Name,
                SpecBuilderMemberType.Reference,
                parameterTypes,
                builderReferenceLocation);
        }

        public SpecBuilderDesc? ExtractBuilderReference(IFieldSymbol builderField, ExtractorContext context) {
            var builderReferenceLocation = builderField.Locations.First();

            if (!ValidateBuilderReference(builderField, builderReferenceLocation, context)) {
                // This is not a builder reference.
                return null;
            }

            GetBuilderReferenceTypes(builderField,
                builderField.Type,
                builderReferenceLocation,
                context,
                out var builtType,
                out var parameterTypes);

            return new SpecBuilderDesc(
                builtType,
                builderField.Name,
                SpecBuilderMemberType.Reference,
                parameterTypes,
                builderReferenceLocation);
        }

        private static bool ValidateBuilder(
            ISymbol builderSymbol,
            Location builderLocation,
            IGeneratorContext context
        ) {
            if (builderSymbol.TryGetBuilderAttribute().GetOrThrow(context) == null) {
                return false;
            }

            if (builderSymbol.TryGetBuilderReferenceAttribute().GetOrThrow(context) != null) {
                // Cannot be a builder and a builder reference.
                throw Diagnostics.InvalidSpecification.AsException(
                    "Method cannot have both Builder and BuilderReference attributes.",
                    builderLocation,
                    context);
            }

            if (!builderSymbol.IsStatic
                || (builderSymbol.DeclaredAccessibility != Accessibility.Public
                    && builderSymbol.DeclaredAccessibility != Accessibility.Internal)) {
                return Result.Error<bool>(
                        "Builders must be public or internal static methods.",
                        builderSymbol.Locations.First(),
                        Diagnostics.InvalidSpecification)
                    .GetOrThrow(context);
            }

            return true;
        }

        private static bool ValidateBuilderReference(
            ISymbol builderReferenceSymbol,
            Location builderReferenceLocation,
            IGeneratorContext generatorCtx
        ) {
            if (builderReferenceSymbol.TryGetBuilderReferenceAttribute().GetOrThrow(generatorCtx) == null) {
                return false;
            }

            if (builderReferenceSymbol.TryGetBuilderAttribute().GetOrThrow(generatorCtx) != null) {
                // Cannot be a builder and a builder reference.
                throw Diagnostics.InvalidSpecification.AsException(
                    "Property or Field cannot have both Builder and BuilderReference attributes.",
                    builderReferenceLocation,
                    generatorCtx);
            }

            if (!builderReferenceSymbol.IsStatic
                || (builderReferenceSymbol.DeclaredAccessibility != Accessibility.Public
                    && builderReferenceSymbol.DeclaredAccessibility != Accessibility.Internal)) {
                return Result.Error<bool>(
                        "Builders references must be public or internal static methods.",
                        builderReferenceSymbol.Locations.First(),
                        Diagnostics.InvalidSpecification)
                    .GetOrThrow(generatorCtx);
            }

            return true;
        }

        private static void GetBuilderReferenceTypes(
            ISymbol builderReferenceSymbol,
            ITypeSymbol builderReferenceTypeSymbol,
            Location builderReferenceLocation,
            ExtractorContext extractorCtx,
            out QualifiedTypeModel builtType,
            out IEnumerable<QualifiedTypeModel> parameterTypes
        ) {
            var referenceTypeSymbol = builderReferenceTypeSymbol as INamedTypeSymbol;
            if (referenceTypeSymbol == null || referenceTypeSymbol.Name != "Action") {
                // Not the correct type to be a builder reference.
                throw Diagnostics.InvalidSpecification.AsException(
                    "Factory reference must be a field or property of type Action<>.",
                    builderReferenceLocation,
                    extractorCtx);
            }

            IReadOnlyList<ITypeSymbol> typeArguments = referenceTypeSymbol.TypeArguments;

            var qualifier = MetadataHelpers.TryGetQualifier(builderReferenceSymbol)
                .GetOrThrow(extractorCtx);
            var returnTypeModel = TypeModel.FromTypeSymbol(typeArguments[0]);
            builtType = new QualifiedTypeModel(
                returnTypeModel,
                qualifier);

            parameterTypes = typeArguments.Count == 1
                ? ImmutableList.Create<QualifiedTypeModel>()
                : typeArguments.Skip(1)
                    .Select(typeArgument => TypeModel.FromTypeSymbol(typeArgument))
                    .Select(typeModel => new QualifiedTypeModel(typeModel, QualifiedTypeModel.NoQualifier))
                    .ToImmutableList();
        }
    }
}
