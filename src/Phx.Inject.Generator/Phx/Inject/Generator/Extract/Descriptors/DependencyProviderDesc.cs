// -----------------------------------------------------------------------------
//  <copyright file="DependencyProviderDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
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
            ExtractorContext extractorCtx
        );
    }

    public class Extractor : IExtractor {
        public DependencyProviderDesc Extract(
            IMethodSymbol providerMethod,
            ExtractorContext extractorCtx
        ) {
            var currentCtx = extractorCtx.GetChildContext(providerMethod);
            var providerLocation = providerMethod.Locations.First();

            if (providerMethod.ReturnsVoid) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Dependency provider {providerMethod.Name} must have a return type.",
                    providerLocation,
                    currentCtx);
            }

            if (providerMethod.Parameters.Length > 0) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Dependency provider {providerMethod.Name} must not have any parameters.",
                    providerLocation,
                    currentCtx);
            }

            var partialAttributes = providerMethod.TryGetPartialAttribute().GetOrThrow(currentCtx);

            var qualifier = MetadataHelpers.TryGetQualifier(providerMethod)
                .GetOrThrow(currentCtx);
            var returnTypeModel = TypeModel.FromTypeSymbol(providerMethod.ReturnType);
            var returnType = new QualifiedTypeModel(
                returnTypeModel,
                qualifier);

            var isPartial = partialAttributes != null;
            TypeHelpers.ValidatePartialType(returnType, isPartial, providerLocation, currentCtx);

            return new DependencyProviderDesc(
                returnType,
                providerMethod.Name,
                isPartial,
                providerLocation);
        }
    }
}
