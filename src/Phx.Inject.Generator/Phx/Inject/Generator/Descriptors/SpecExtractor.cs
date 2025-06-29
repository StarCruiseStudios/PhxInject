// -----------------------------------------------------------------------------
//  <copyright file="SpecExtractor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Descriptors {
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Definitions;
    using Phx.Inject.Generator.Model;

    internal class SpecExtractor {
        private readonly CreateSpecDescriptor createSpecDescriptor;
        private readonly CreateConstructorSpecDescriptor createConstructorSpecDescriptor;

        public SpecExtractor(
            CreateSpecDescriptor createSpecDescriptor,
            CreateConstructorSpecDescriptor createConstructorSpecDescriptor
        ) {
            this.createSpecDescriptor = createSpecDescriptor;
            this.createConstructorSpecDescriptor = createConstructorSpecDescriptor;
        }

        public SpecExtractor() : this(
            new SpecDescriptor.Builder(
                new SpecFactoryDescriptor.Builder().BuildFactory,
                new SpecFactoryDescriptor.Builder().BuildFactory,
                new SpecFactoryDescriptor.Builder().BuildFactoryReference,
                new SpecFactoryDescriptor.Builder().BuildFactoryReference,
                new SpecBuilderDescriptor.Builder().BuildBuilder,
                new SpecBuilderDescriptor.Builder().BuildBuilderReference,
                new SpecBuilderDescriptor.Builder().BuildBuilderReference,
                new SpecLinkDescriptor.Builder().Build).Build,
            new SpecDescriptor.ConstructorBuilder(
                new SpecFactoryDescriptor.Builder().BuildConstructorFactory,
                new SpecBuilderDescriptor.Builder().BuildDirectBuilder).BuildConstructorSpec
        ) { }

        private HashSet<QualifiedTypeModel> GetAutoConstructorParameterTypes(QualifiedTypeModel type) {
            var results = new HashSet<QualifiedTypeModel>();
            results.UnionWith(MetadataHelpers.GetConstructorParameterQualifiedTypes(type.TypeModel.typeSymbol));
            results.UnionWith(MetadataHelpers.GetRequiredPropertyQualifiedTypes(type.TypeModel.typeSymbol).Values);
            return results;
        }

        public SpecDescriptor? ExtractConstructorSpecForContext(
            DefinitionGenerationContext context
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
            
            foreach (var specDescriptor in context.Specifications.Values) {
                foreach (var factory in specDescriptor.Factories) {
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

                foreach (var link in specDescriptor.Links) {
                    providedTypes.Add(link.ReturnType);
                    neededTypes.Add(link.InputType);
                }

                foreach (var builder in specDescriptor.Builders) {
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

            var missingTypes = neededTypes.Except(providedTypes).ToImmutableList();
            var missingBuilders = neededBuilders.Except(providedBuilders).ToImmutableList();
            
            var needsConstructorSpec = missingTypes.Any() || missingBuilders.Any();
            return needsConstructorSpec
                    ? createConstructorSpecDescriptor(
                        context.Injector.InjectorType,
                        missingTypes,
                        missingBuilders)
                    : null;
        }

        public IReadOnlyList<SpecDescriptor> Extract(
            IEnumerable<TypeDeclarationSyntax> syntaxNodes,
            DescriptorGenerationContext context
        ) {
            return MetadataHelpers.GetTypeSymbolsFromDeclarations(syntaxNodes, context.GenerationContext)
                .Where(IsSpecSymbol)
                .Select(symbol => createSpecDescriptor(symbol, context))
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
}
