// -----------------------------------------------------------------------------
//  <copyright file="InjectorProviderDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Metadata;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record InjectorProviderDesc(
    QualifiedTypeModel ProvidedType,
    string ProviderMethodName,
    Location Location
) : IDescriptor {
    public interface IExtractor {
        InjectorProviderDesc? Extract(
            IMethodSymbol providerMethod,
            ExtractorContext extractorCtx
        );
    }

    public class Extractor : IExtractor {
        private readonly ChildInjectorAttributeMetadata.IExtractor childInjectorAttributeExtractor;
        private readonly QualifierMetadata.IExtractor qualifierExtractor;
        public Extractor(
            ChildInjectorAttributeMetadata.IExtractor childInjectorAttributeExtractor,
            QualifierMetadata.IExtractor qualifierExtractor
        ) {
            this.childInjectorAttributeExtractor = childInjectorAttributeExtractor;
            this.qualifierExtractor = qualifierExtractor;
        }

        public Extractor() : this(
            ChildInjectorAttributeMetadata.Extractor.Instance,
            QualifierMetadata.Extractor.Instance
        ) { }

        public InjectorProviderDesc? Extract(
            IMethodSymbol providerMethod,
            ExtractorContext extractorCtx
        ) {
            return extractorCtx.UseChildContext(providerMethod,
                currentCtx => {
                    var providerLocation = providerMethod.Locations.First();

                    if (providerMethod.ReturnsVoid) {
                        // This is a builder, not a provider.
                        return null;
                    }

                    if (childInjectorAttributeExtractor.CanExtract(providerMethod)) {
                        // This is an injector child factory, not a provider.
                        return null;
                    }

                    if (providerMethod.Parameters.Length > 0) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            $"Injector provider {providerMethod.Name} must not have any parameters.",
                            providerLocation,
                            currentCtx);
                    }

                    var returnType = TypeModel.FromTypeSymbol(providerMethod.ReturnType);
                    var qualifier = qualifierExtractor.Extract(providerMethod, currentCtx);
                    return new InjectorProviderDesc(
                        new QualifiedTypeModel(returnType, qualifier),
                        providerMethod.Name,
                        providerLocation);
                });
        }
    }
}
