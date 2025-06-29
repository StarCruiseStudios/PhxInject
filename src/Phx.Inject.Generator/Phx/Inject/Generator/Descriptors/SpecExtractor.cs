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

        private HashSet<QualifiedTypeModel> GetParameterTypes(
            ITypeSymbol type,
            HashSet<QualifiedTypeModel> providedTypes) {
            var neededTypes = new HashSet<QualifiedTypeModel>();
            
            // Add constructor parameters
            MetadataHelpers.GetConstructorParameterQualifiedTypes(type).ForEach(parameter => {
                if (!providedTypes.Contains(parameter)) {
                    neededTypes.Add(parameter);
                    neededTypes.UnionWith(GetParameterTypes(parameter.TypeModel.typeSymbol, providedTypes));
                }
            });
            
            // Add required properties
            foreach (var property in MetadataHelpers.GetRequiredPropertyQualifiedTypes(type).Values) {
                if (!providedTypes.Contains(property)) {
                    neededTypes.Add(property);
                    neededTypes.UnionWith(GetParameterTypes(property.TypeModel.typeSymbol, providedTypes));
                }
            }
            
            return neededTypes;
        }

        public IReadOnlyList<SpecDescriptor> ExtractConstructorSpecForContext(
            DefinitionGenerationContext context
        ) {
            var providedTypes = new HashSet<QualifiedTypeModel>();
            var neededTypes = new HashSet<QualifiedTypeModel>();
            
            var neededBuilders = new HashSet<QualifiedTypeModel>();
            var providedBuilders = new HashSet<QualifiedTypeModel>();

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

            var transitiveTypes = new HashSet<QualifiedTypeModel>();
            foreach (var neededType in neededTypes) {
                if (!providedTypes.Contains(neededType)) {
                    transitiveTypes.UnionWith(GetParameterTypes(neededType.TypeModel.typeSymbol, providedTypes));
                }
            }

            neededTypes.UnionWith(transitiveTypes);

            var missingTypes = neededTypes.Except(providedTypes).ToImmutableList();
            var missingBuilders = neededBuilders.Except(providedBuilders).ToImmutableList();
            
            return missingTypes.Any() || missingBuilders.Any()
                ? new List<SpecDescriptor>() {
                    createConstructorSpecDescriptor(
                        context.Injector.InjectorType,
                        missingTypes,
                        missingBuilders)
                }
                : ImmutableList<SpecDescriptor>.Empty;
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
