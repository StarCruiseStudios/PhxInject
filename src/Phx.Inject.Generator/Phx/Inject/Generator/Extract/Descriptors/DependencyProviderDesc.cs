// -----------------------------------------------------------------------------
//  <copyright file="DependencyProviderDesc.cs" company="Star Cruise Studios LLC">
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
    IMethodSymbol Symbol,
    QualifiedTypeModel ProvidedType,
    PartialAttributeDesc? PartialAttribute
) : IDescriptor {
    public string ProviderMethodName => Symbol.Name;
    public bool IsPartial => PartialAttribute != null;
    
    public Location Location => Symbol.Locations.First();

    public static void RequireDependencyProvider(
        IMethodSymbol providerMethod,
        IGeneratorContext generatorCtx
    ) {
        ExceptionAggregator.Try(
            "Validating dependency provider",
            generatorCtx,
            _ => {
                if (providerMethod.ReturnsVoid) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Dependency provider {providerMethod.Name} must have a return type.",
                        providerMethod.Locations.First(),
                        generatorCtx);
                }
            },
            _ => {
                if (providerMethod.Parameters.Length > 0) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Dependency provider {providerMethod.Name} must not have any parameters.",
                        providerMethod.Locations.First(),
                        generatorCtx);
                }
            });
    }
    
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

            RequireDependencyProvider(providerMethod, currentCtx);
            var qualifier = providerMethod.GetQualifier().GetOrThrow(currentCtx);
            var returnType = providerMethod.ReturnType.ToQualifiedTypeModel(qualifier);
            var partialAttribute = providerMethod.TryGetPartialAttribute().GetOrThrow(currentCtx);
            if (partialAttribute != null) {
                TypeModel.RequirePartialType(returnType.TypeModel, providerMethod.Locations.First(), currentCtx);
            }

            return new DependencyProviderDesc(providerMethod, returnType, partialAttribute);
        }
    }
}
