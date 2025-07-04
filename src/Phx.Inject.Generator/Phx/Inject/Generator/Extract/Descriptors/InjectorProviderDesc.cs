// -----------------------------------------------------------------------------
//  <copyright file="InjectorProviderDescriptor.cs" company="Star Cruise Studios LLC">
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

internal record InjectorProviderDesc(
    QualifiedTypeModel ProvidedType,
    string ProviderMethodName,
    Location Location
) : IDescriptor {
    public interface IExtractor {
        InjectorProviderDesc? Extract(
            IMethodSymbol providerMethod,
            ExtractorContext context
        );
    }

    public class Extractor : IExtractor {
        public InjectorProviderDesc? Extract(
            IMethodSymbol providerMethod,
            ExtractorContext context
        ) {
            var providerLocation = providerMethod.Locations.First();

            if (providerMethod.ReturnsVoid) {
                // This is a builder, not a provider.
                return null;
            }

            if (providerMethod.TryGetChildInjectorAttribute().GetOrThrow(context.GenerationContext) != null) {
                // This is an injector child factory, not a provider.
                return null;
            }

            if (providerMethod.Parameters.Length > 0) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Injector provider {providerMethod.Name} must not have any parameters.",
                    providerLocation,
                    context.GenerationContext);
            }

            var returnType = TypeModel.FromTypeSymbol(providerMethod.ReturnType);
            var qualifier = MetadataHelpers.TryGetQualifier(providerMethod, context.GenerationContext)
                .GetOrThrow(context.GenerationContext);
            return new InjectorProviderDesc(
                new QualifiedTypeModel(returnType, qualifier),
                providerMethod.Name,
                providerLocation);
        }
    }
}
