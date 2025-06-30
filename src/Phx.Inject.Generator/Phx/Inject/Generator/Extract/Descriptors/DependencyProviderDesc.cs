// -----------------------------------------------------------------------------
//  <copyright file="DependencyProviderDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Model;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record DependencyProviderDesc(
    QualifiedTypeModel ProvidedType,
    string ProviderMethodName,
    bool isPartial,
    Location Location
) : IDescriptor {
    public interface IExtractor {
        DependencyProviderDesc Extract(
            IMethodSymbol providerMethod,
            DescGenerationContext context
        );
    }

    public class Extractor : IExtractor {
        public DependencyProviderDesc Extract(
            IMethodSymbol providerMethod,
            DescGenerationContext context
        ) {
            var providerLocation = providerMethod.Locations.First();

            if (providerMethod.ReturnsVoid) {
                throw new InjectionException(
                    Diagnostics.InvalidSpecification,
                    $"Dependency provider {providerMethod.Name} must have a return type.",
                    providerLocation);
            }

            if (providerMethod.Parameters.Length > 0) {
                throw new InjectionException(
                    Diagnostics.InvalidSpecification,
                    $"Dependency provider {providerMethod.Name} must not have any parameters.",
                    providerLocation);
            }

            var partialAttributes = providerMethod.GetPartialAttributes();

            var qualifier = MetadataHelpers.GetQualifier(providerMethod);
            var returnTypeModel = TypeModel.FromTypeSymbol(providerMethod.ReturnType);
            var returnType = new QualifiedTypeModel(
                returnTypeModel,
                qualifier);

            var isPartial = partialAttributes.Any();
            TypeHelpers.ValidatePartialType(returnType, isPartial, providerLocation);

            return new DependencyProviderDesc(
                returnType,
                providerMethod.Name,
                isPartial,
                providerLocation);
        }

        private static bool GetIsPartial(ISymbol factorySymbol) {
            var partialAttributes = factorySymbol.GetPartialAttributes();
            return partialAttributes.Any();
        }
    }
}
