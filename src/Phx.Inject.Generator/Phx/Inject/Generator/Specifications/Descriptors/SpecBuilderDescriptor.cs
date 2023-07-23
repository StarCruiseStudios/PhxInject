// -----------------------------------------------------------------------------
//  <copyright file="SpecBuilderDescriptor.cs" company="Star Cruise Studios LLC">
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

    internal delegate SpecBuilderDescriptor? CreateSpecBuilderDescriptor(
            IMethodSymbol builderMethod,
            DescriptorGenerationContext context
    );

    internal delegate SpecBuilderDescriptor? CreateSpecBuilderReferencePropertyDescriptor(
            IPropertySymbol builderMethod,
            DescriptorGenerationContext context
    );

    internal delegate SpecBuilderDescriptor? CreateSpecBuilderReferenceFieldDescriptor(
            IFieldSymbol builderMethod,
            DescriptorGenerationContext context
    );

    internal record SpecBuilderDescriptor(
            QualifiedTypeModel BuiltType,
            string BuilderMemberName,
            SpecBuilderMemberType SpecBuilderMemberType,
            IEnumerable<QualifiedTypeModel> Parameters,
            Location Location
    ) : IDescriptor {
        public class Builder {
            public SpecBuilderDescriptor? BuildBuilder(
                    IMethodSymbol builderMethod,
                    DescriptorGenerationContext context) {
                var builderLocation = builderMethod.Locations.First();

                if (!ValidateBuilder(builderMethod, builderLocation, context)) {
                    // This is not a builder method.
                    return null;
                }

                var methodParameterTypes = MetadataHelpers.GetMethodParametersQualifiedTypes(builderMethod);
                if (methodParameterTypes.Count == 0) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            "Builder method must have at least one parameter.",
                            builderLocation);
                }

                var qualifier = MetadataHelpers.GetQualifier(builderMethod);
                // Use the qualifier from the method, not the parameter.
                var builtType = methodParameterTypes[0] with {
                    Qualifier = qualifier
                };
                var builderArguments = methodParameterTypes.Count > 1
                        ? methodParameterTypes.GetRange(index: 1, methodParameterTypes.Count - 1)
                        : ImmutableList.Create<QualifiedTypeModel>();

                return new SpecBuilderDescriptor(
                        builtType,
                        builderMethod.Name,
                        SpecBuilderMemberType.Method,
                        builderArguments,
                        builderLocation);
            }

            public SpecBuilderDescriptor? BuildBuilderReference(
                    IPropertySymbol builderProperty,
                    DescriptorGenerationContext context) {
                var builderReferenceLocation = builderProperty.Locations.First();

                if (!ValidateBuilderReference(builderProperty, builderReferenceLocation, context)) {
                    // This is not a builder reference.
                    return null;
                }

                GetBuilderReferenceTypes(builderProperty,
                        builderProperty.Type,
                        builderReferenceLocation,
                        out var builtType,
                        out var parameterTypes);

                return new SpecBuilderDescriptor(
                        builtType,
                        builderProperty.Name,
                        SpecBuilderMemberType.Reference,
                        parameterTypes,
                        builderReferenceLocation);
            }

            public SpecBuilderDescriptor? BuildBuilderReference(
                    IFieldSymbol builderField,
                    DescriptorGenerationContext context) {
                var builderReferenceLocation = builderField.Locations.First();

                if (!ValidateBuilderReference(builderField, builderReferenceLocation, context)) {
                    // This is not a builder reference.
                    return null;
                }

                GetBuilderReferenceTypes(builderField,
                        builderField.Type,
                        builderReferenceLocation,
                        out var builtType,
                        out var parameterTypes);

                return new SpecBuilderDescriptor(
                        builtType,
                        builderField.Name,
                        SpecBuilderMemberType.Reference,
                        parameterTypes,
                        builderReferenceLocation);
            }

            private static bool ValidateBuilder(
                    ISymbol builderSymbol,
                    Location builderLocation,
                    DescriptorGenerationContext context
            ) {
                var builderAttributes = builderSymbol.GetBuilderAttributes();
                var numBuilderAttributes = builderAttributes.Count;
                if (numBuilderAttributes == 0) {
                    // This is not a builder method.
                    return false;
                }

                if (numBuilderAttributes > 1) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            "Method can only have a single builder attribute.",
                            builderLocation);
                }

                var builderReferenceAttributes = builderSymbol.GetBuilderReferenceAttributes();
                if (builderReferenceAttributes.Count > 0) {
                    // Cannot be a builder and a builder reference.
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            "Method cannot have both Builder and BuilderReference attributes.",
                            builderLocation);
                }

                return true;
            }

            private static bool ValidateBuilderReference(
                    ISymbol builderReferenceSymbol,
                    Location builderReferenceLocation,
                    DescriptorGenerationContext context
            ) {
                var builderReferenceAttributes = builderReferenceSymbol.GetBuilderReferenceAttributes();
                var numBuilderReferenceAttributes = builderReferenceAttributes.Count;
                if (numBuilderReferenceAttributes == 0) {
                    // This is not a builder reference.
                    return false;
                }

                if (numBuilderReferenceAttributes > 1) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            "Method can only have a single builder reference attribute.",
                            builderReferenceLocation);
                }

                var builderAttributes = builderReferenceSymbol.GetBuilderAttributes();
                if (builderAttributes.Count > 0) {
                    // Cannot be a builder and a builder reference.
                    throw new InjectionException(
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
                    out QualifiedTypeModel builtType,
                    out IEnumerable<QualifiedTypeModel> parameterTypes
            ) {
                var referenceTypeSymbol = builderReferenceTypeSymbol as INamedTypeSymbol;
                if (referenceTypeSymbol == null || referenceTypeSymbol.Name != "Action") {
                    // Not the correct type to be a builder reference.
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            "Factory reference must be a field or property of type Action<>.",
                            builderReferenceLocation);
                }

                var typeArguments = referenceTypeSymbol.TypeArguments;

                var qualifier = MetadataHelpers.GetQualifier(builderReferenceSymbol);
                var returnTypeModel = TypeModel.FromTypeSymbol(typeArguments[0]);
                builtType = new QualifiedTypeModel(
                        returnTypeModel,
                        qualifier);

                parameterTypes = typeArguments.Length == 1
                        ? ImmutableList.Create<QualifiedTypeModel>()
                        : typeArguments.Skip(1)
                                .Select(typeArgument => TypeModel.FromTypeSymbol(typeArgument))
                                .Select(typeModel => new QualifiedTypeModel(typeModel, QualifiedTypeModel.NoQualifier))
                                .ToImmutableList();
            }
        }
    }
}
