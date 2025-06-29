// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyProviderDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Descriptors {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Model;

    internal record ExternalDependencyProviderDescriptor(
        QualifiedTypeModel ProvidedType,
        string ProviderMethodName,
        bool isPartial,
        Location Location
    ) : IDescriptor {
        public interface IBuilder {
            ExternalDependencyProviderDescriptor Build(
                IMethodSymbol providerMethod,
                DescriptorGenerationContext context
            );
        }
        public class Builder : IBuilder {
            public ExternalDependencyProviderDescriptor Build(
                IMethodSymbol providerMethod,
                DescriptorGenerationContext context
            ) {
                var providerLocation = providerMethod.Locations.First();

                if (providerMethod.ReturnsVoid) {
                    throw new InjectionException(
                        Diagnostics.InvalidSpecification,
                        $"External dependency provider {providerMethod.Name} must have a return type.",
                        providerLocation);
                }

                if (providerMethod.Parameters.Length > 0) {
                    throw new InjectionException(
                        Diagnostics.InvalidSpecification,
                        $"External dependency provider {providerMethod.Name} must not have any parameters.",
                        providerLocation);
                }

                var partialAttributes = AttributeHelpers.GetPartialAttributes(providerMethod);

                var qualifier = MetadataHelpers.GetQualifier(providerMethod);
                var returnTypeModel = TypeModel.FromTypeSymbol(providerMethod.ReturnType);
                var returnType = new QualifiedTypeModel(
                    returnTypeModel,
                    qualifier);

                var isPartial = partialAttributes.Any();
                TypeHelpers.ValidatePartialType(returnType, isPartial, providerLocation);

                return new ExternalDependencyProviderDescriptor(
                    returnType,
                    providerMethod.Name,
                    isPartial,
                    providerLocation);
            }

            private static bool GetIsPartial(ISymbol factorySymbol) {
                var partialAttributes = AttributeHelpers.GetPartialAttributes(factorySymbol);
                return partialAttributes.Any();
            }
        }
    }
}
