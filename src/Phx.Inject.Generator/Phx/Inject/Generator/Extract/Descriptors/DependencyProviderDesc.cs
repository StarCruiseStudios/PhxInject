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
    TypeModel DependencyInterface,
    QualifiedTypeModel ProvidedType,
    PartialAttributeDesc? PartialAttribute
) : IDescriptor {
    public string ProviderMethodName => Symbol.Name;
    public bool IsPartial => PartialAttribute != null;
    public Location Location => Symbol.Locations.First();

    public static void RequireDependencyProvider(
        IMethodSymbol symbol,
        IGeneratorContext generatorCtx
    ) {
        ExceptionAggregator.Try(
            "Validating dependency provider",
            generatorCtx,
            _ => {
                if (symbol.ReturnsVoid) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Dependency provider {symbol.Name} must have a return type.",
                        symbol.Locations.First(),
                        generatorCtx);
                }
            },
            _ => {
                if (symbol.Parameters.Length > 0) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Dependency provider {symbol.Name} must not have any parameters.",
                        symbol.Locations.First(),
                        generatorCtx);
                }
            });
    }
    
    public interface IExtractor {
        DependencyProviderDesc Extract(
            IMethodSymbol symbol,
            TypeModel dependencyInterface,
            ExtractorContext extractorCtx
        );
    }

    public class Extractor : IExtractor {
        public DependencyProviderDesc Extract(
            IMethodSymbol symbol,
            TypeModel dependencyInterface,
            ExtractorContext extractorCtx
        ) {
            var currentCtx = extractorCtx.GetChildContext(symbol);

            RequireDependencyProvider(symbol, currentCtx);
            var qualifier = symbol.GetQualifier().GetOrThrow(currentCtx);
            var providedType = symbol.ReturnType.ToQualifiedTypeModel(qualifier);
            var partialAttribute = symbol.TryGetPartialAttribute().GetOrThrow(currentCtx);
            if (partialAttribute != null) {
                TypeModel.RequirePartialType(providedType.TypeModel, symbol.Locations.First(), currentCtx);
            }

            return new DependencyProviderDesc(symbol, dependencyInterface, providedType, partialAttribute);
        }
    }
}
