// -----------------------------------------------------------------------------
//  <copyright file="SpecExtractor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Phx.Inject.Common;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Descriptors;
using Phx.Inject.Generator.Map;

namespace Phx.Inject.Generator.Extract;

internal interface ISpecExtractor {
    SpecDesc? ExtractConstructorSpecForContext(
        DefGenerationContext context
    );
    IReadOnlyList<SpecDesc> Extract(
        IEnumerable<TypeDeclarationSyntax> syntaxNodes,
        DescGenerationContext context
    );
}

internal class SpecExtractor : ISpecExtractor {
    private readonly SpecDesc.IExtractor specDescExtractor;

    public SpecExtractor(
        SpecDesc.IExtractor specDescExtractor
    ) {
        this.specDescExtractor = specDescExtractor;
    }

    public SpecExtractor() : this(
        new SpecDesc.Extractor()
    ) { }

    private HashSet<QualifiedTypeModel> GetAutoConstructorParameterTypes(QualifiedTypeModel type) {
        var results = new HashSet<QualifiedTypeModel>();
        results.UnionWith(MetadataHelpers.GetConstructorParameterQualifiedTypes(type.TypeModel.typeSymbol));
        results.UnionWith(MetadataHelpers.GetRequiredPropertyQualifiedTypes(type.TypeModel.typeSymbol).Values);
        return results;
    }

    public SpecDesc? ExtractConstructorSpecForContext(
        DefGenerationContext context
    ) {
        var providedTypes = new HashSet<QualifiedTypeModel>();
        var neededTypes = new HashSet<QualifiedTypeModel>();

        var neededBuilders = new HashSet<QualifiedTypeModel>();
        var providedBuilders = new HashSet<QualifiedTypeModel>();

        foreach (var provider in context.Injector.Providers) {
            if (provider.ProvidedType.TypeModel.QualifiedBaseTypeName == TypeHelpers.FactoryTypeName) {
                neededTypes.Add(new QualifiedTypeModel(
                    provider.ProvidedType.TypeModel.TypeArguments[0],
                    provider.ProvidedType.Qualifier));
            } else {
                neededTypes.Add(provider.ProvidedType);
            }
        }

        foreach (var builder in context.Injector.Builders) {
            neededBuilders.Add(builder.BuiltType);
        }

        foreach (var specDesc in context.Specifications.Values) {
            foreach (var factory in specDesc.Factories) {
                providedTypes.Add(factory.ReturnType);

                foreach (var parameterType in factory.Parameters) {
                    if (parameterType.TypeModel.QualifiedBaseTypeName == TypeHelpers.FactoryTypeName) {
                        var factoryType = parameterType with {
                            TypeModel = parameterType.TypeModel.TypeArguments.Single()
                        };
                        neededTypes.Add(factoryType);
                    } else {
                        neededTypes.Add(parameterType);
                    }
                }
            }

            foreach (var link in specDesc.Links) {
                providedTypes.Add(link.ReturnType);
                neededTypes.Add(link.InputType);
            }

            foreach (var builder in specDesc.Builders) {
                providedBuilders.Add(builder.BuiltType);

                foreach (var parameterType in builder.Parameters) {
                    neededTypes.Add(parameterType);
                }
            }
        }

        var typeSearchQueue = new Queue<QualifiedTypeModel>();
        foreach (var qualifiedTypeModel in neededTypes) {
            typeSearchQueue.Enqueue(qualifiedTypeModel);
        }

        while (typeSearchQueue.Count > 0) {
            var type = typeSearchQueue.Dequeue();
            if (!providedTypes.Contains(type)) {
                foreach (var parameterType in GetAutoConstructorParameterTypes(type)) {
                    if (neededTypes.Add(parameterType)) {
                        typeSearchQueue.Enqueue(parameterType);
                    }
                }
            }
        }

        IReadOnlyList<QualifiedTypeModel> missingTypes = neededTypes.Except(providedTypes).ToImmutableList();
        IReadOnlyList<QualifiedTypeModel> missingBuilders = neededBuilders.Except(providedBuilders).ToImmutableList();

        var needsConstructorSpec = missingTypes.Any() || missingBuilders.Any();
        return needsConstructorSpec
            ? specDescExtractor.ExtractConstructorSpec(
                context.Injector.InjectorType,
                missingTypes,
                missingBuilders)
            : null;
    }

    public IReadOnlyList<SpecDesc> Extract(
        IEnumerable<TypeDeclarationSyntax> syntaxNodes,
        DescGenerationContext context
    ) {
        return MetadataHelpers.GetTypeSymbolsFromDeclarations(syntaxNodes, context.GenerationContext)
            .Where(IsSpecSymbol)
            .Select(symbol => specDescExtractor.Extract(symbol, context))
            .ToImmutableList();
    }

    private static bool IsSpecSymbol(ITypeSymbol symbol) {
        var specificationAttribute = symbol.GetSpecificationAttribute();
        if (specificationAttribute == null) {
            return false;
        }

        var isStaticSpecification = symbol.TypeKind == TypeKind.Class && symbol.IsStatic;
        var isInterfaceSpecification = symbol.TypeKind == TypeKind.Interface;

        if (!isStaticSpecification && !isInterfaceSpecification) {
            throw new InjectionException(
                Diagnostics.InvalidSpecification,
                $"Specification type {symbol.Name} must be a static class or interface.",
                symbol.Locations.First());
        }

        return true;
    }
}
