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
        SpecBuilderDesc? ExtractBuilder(IMethodSymbol builderMethod, DescGenerationContext context);
        SpecBuilderDesc ExtractDirectBuilder(QualifiedTypeModel builderType, DescGenerationContext context);
        SpecBuilderDesc? ExtractBuilderReference(IPropertySymbol builderProperty, DescGenerationContext context);
        SpecBuilderDesc? ExtractBuilderReference(IFieldSymbol builderField, DescGenerationContext context);
    }

    public class Extractor : IExtractor {
        public SpecBuilderDesc? ExtractBuilder(IMethodSymbol builderMethod, DescGenerationContext context) {
            var builderLocation = builderMethod.Locations.First();

            if (!ValidateBuilder(builderMethod, builderLocation, context.GenerationContext)) {
                // This is not a builder method.
                return null;
            }

            var methodParameterTypes =
                MetadataHelpers.GetMethodParametersQualifiedTypes(builderMethod, context.GenerationContext);
            if (methodParameterTypes.Count == 0) {
                throw new InjectionException(
                    context.GenerationContext,
                    Diagnostics.InvalidSpecification,
                    "Builder method must have at least one parameter.",
                    builderLocation);
            }

            var qualifier = MetadataHelpers.GetQualifier(builderMethod, context.GenerationContext);
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

        public SpecBuilderDesc ExtractDirectBuilder(QualifiedTypeModel builderType, DescGenerationContext context) {
            var builderLocation = builderType.TypeModel.typeSymbol.Locations.First();
            IReadOnlyList<IMethodSymbol> builderMethods = MetadataHelpers
                .GetDirectBuilderMethods(builderType.TypeModel.typeSymbol, context.GenerationContext)
                .Where(b => MetadataHelpers.GetQualifier(b, context.GenerationContext) == builderType.Qualifier)
                .Where(b => ValidateBuilder(b, builderLocation, context.GenerationContext))
                .ToImmutableList();

            var numBuilderMethods = builderMethods.Count;
            if (numBuilderMethods == 0) {
                throw new InjectionException(
                    context.GenerationContext,
                    Diagnostics.InvalidSpecification,
                    "No direct builder method found for required builder type: " + builderType,
                    builderLocation);
            }

            if (numBuilderMethods > 1) {
                throw new InjectionException(
                    context.GenerationContext,
                    Diagnostics.InvalidSpecification,
                    "More than one direct builder method found for type: " + builderType,
                    builderLocation);
            }

            var builderMethod = builderMethods.First();
            var methodParameterTypes =
                MetadataHelpers.GetMethodParametersQualifiedTypes(builderMethod, context.GenerationContext);
            if (methodParameterTypes.Count == 0) {
                throw new InjectionException(
                    context.GenerationContext,
                    Diagnostics.InvalidSpecification,
                    "Builder method must have at least one parameter.",
                    builderLocation);
            }

            if (methodParameterTypes[0].TypeModel != builderType.TypeModel) {
                throw new InjectionException(
                    context.GenerationContext,
                    Diagnostics.InvalidSpecification,
                    "Direct builder method must accept a first parameter of the built type.",
                    builderLocation);
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

        public SpecBuilderDesc? ExtractBuilderReference(IPropertySymbol builderProperty, DescGenerationContext context) {
            var builderReferenceLocation = builderProperty.Locations.First();

            if (!ValidateBuilderReference(builderProperty, builderReferenceLocation, context.GenerationContext)) {
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

        public SpecBuilderDesc? ExtractBuilderReference(IFieldSymbol builderField, DescGenerationContext context) {
            var builderReferenceLocation = builderField.Locations.First();

            if (!ValidateBuilderReference(builderField, builderReferenceLocation, context.GenerationContext)) {
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
            GeneratorExecutionContext context
        ) {
            var builderAttribute = builderSymbol.GetBuilderAttribute(context);
            if (builderAttribute == null) {
                // This is not a builder method.
                return false;
            }

            var builderReferenceAttribute = builderSymbol.GetBuilderReferenceAttributes(context);
            if (builderReferenceAttribute != null) {
                // Cannot be a builder and a builder reference.
                throw new InjectionException(
                    context,
                    Diagnostics.InvalidSpecification,
                    "Method cannot have both Builder and BuilderReference attributes.",
                    builderLocation);
            }

            return true;
        }

        private static bool ValidateBuilderReference(
            ISymbol builderReferenceSymbol,
            Location builderReferenceLocation,
            GeneratorExecutionContext context
        ) {
            var builderReferenceAttribute = builderReferenceSymbol.GetBuilderReferenceAttributes(context);
            if (builderReferenceAttribute == null) {
                // This is not a builder reference.
                return false;
            }

            var builderAttribute = builderReferenceSymbol.GetBuilderAttribute(context);
            if (builderAttribute != null) {
                // Cannot be a builder and a builder reference.
                throw new InjectionException(
                    context,
                    Diagnostics.InvalidSpecification,
                    "Property or Field cannot have both Builder and BuilderReference attributes.",
                    builderReferenceLocation);
            }

            return true;
        }

        private static void GetBuilderReferenceTypes(
            ISymbol builderReferenceSymbol,
            ITypeSymbol builderReferenceTypeSymbol,
            Location builderReferenceLocation,
            DescGenerationContext context,
            out QualifiedTypeModel builtType,
            out IEnumerable<QualifiedTypeModel> parameterTypes
        ) {
            var referenceTypeSymbol = builderReferenceTypeSymbol as INamedTypeSymbol;
            if (referenceTypeSymbol == null || referenceTypeSymbol.Name != "Action") {
                // Not the correct type to be a builder reference.
                throw new InjectionException(
                    context.GenerationContext,
                    Diagnostics.InvalidSpecification,
                    "Factory reference must be a field or property of type Action<>.",
                    builderReferenceLocation);
            }

            IReadOnlyList<ITypeSymbol> typeArguments = referenceTypeSymbol.TypeArguments;

            var qualifier = MetadataHelpers.GetQualifier(builderReferenceSymbol, context.GenerationContext);
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
