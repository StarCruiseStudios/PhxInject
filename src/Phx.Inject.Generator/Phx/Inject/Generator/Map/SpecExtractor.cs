// -----------------------------------------------------------------------------
//  <copyright file="SpecExtractor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Phx.Inject.Common;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Map;

internal interface ISpecDefMapper {
    SpecDesc? ExtractConstructorSpecForContext(
        DefGenerationContext defGenerationCtx
    );
}

internal class SpecDefMapper : ISpecDefMapper {
    private readonly SpecDesc.IAutoSpecExtractor autoSpecExtractor;

    public SpecDefMapper(
        SpecDesc.IAutoSpecExtractor autoSpecExtractor
    ) {
        this.autoSpecExtractor = autoSpecExtractor;
    }

    public SpecDefMapper() : this(
        new SpecDesc.AutoSpecExtractor()
    ) { }

    public SpecDesc? ExtractConstructorSpecForContext(
        DefGenerationContext defGenerationCtx
    ) {
        var providedTypes = new HashSet<QualifiedTypeModel>();
        var neededTypes = new HashSet<QualifiedTypeModel>();

        var providedBuilders = new HashSet<QualifiedTypeModel>();
        var neededBuilders = new HashSet<QualifiedTypeModel>();

        foreach (var provider in defGenerationCtx.Injector.Providers) {
            if (provider.ProvidedType.TypeModel.NamespacedBaseTypeName == TypeNames.FactoryClassName) {
                neededTypes.Add(new QualifiedTypeModel(
                    provider.ProvidedType.TypeModel.TypeArguments[0],
                    provider.ProvidedType.Qualifier));
            } else {
                neededTypes.Add(provider.ProvidedType);
            }
        }

        foreach (var builder in defGenerationCtx.Injector.Builders) {
            neededBuilders.Add(builder.BuiltType);
        }

        foreach (var specDesc in defGenerationCtx.Specifications.Values) {
            foreach (var factory in specDesc.Factories) {
                providedTypes.Add(factory.ReturnType);

                foreach (var parameterType in factory.Parameters) {
                    if (parameterType.TypeModel.NamespacedBaseTypeName == TypeNames.FactoryClassName) {
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
                foreach (var parameterType in GetAutoConstructorParameterTypes(type, defGenerationCtx)) {
                    if (neededTypes.Add(parameterType)) {
                        typeSearchQueue.Enqueue(parameterType);
                    }
                }
            }
        }

        IReadOnlyList<QualifiedTypeModel> autoFactoryTypes = neededTypes.Except(providedTypes)
            .Where(TypeHelpers.IsAutoFactoryEligible)
            .ToImmutableList();
        IReadOnlyList<QualifiedTypeModel> autoBuilderTypes = neededBuilders.Except(providedBuilders).ToImmutableList();

        var needsConstructorSpec = autoFactoryTypes.Any() || autoBuilderTypes.Any();
        return needsConstructorSpec
            ? autoSpecExtractor.Extract(
                defGenerationCtx.Injector.InjectorType,
                autoFactoryTypes,
                autoBuilderTypes,
                new ExtractorContext("mapping specification", null, defGenerationCtx))
            : null;
    }

    private HashSet<QualifiedTypeModel> GetAutoConstructorParameterTypes(
        QualifiedTypeModel type,
        DefGenerationContext defGenerationCtx) {
        var results = new HashSet<QualifiedTypeModel>();
        results.UnionWith(
            MetadataHelpers.TryGetConstructorParameterQualifiedTypes(type.TypeModel.TypeSymbol,
                defGenerationCtx));
        results.UnionWith(MetadataHelpers
            .GetRequiredPropertyQualifiedTypes(type.TypeModel.TypeSymbol, defGenerationCtx)
            .Values);
        return results;
    }
}
