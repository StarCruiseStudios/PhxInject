﻿// -----------------------------------------------------------------------------
//  <copyright file="SpecExtractor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Specifications.Descriptors {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Descriptors;
    using Phx.Inject.Generator.Injectors.Descriptors;

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
                        new SpecFactoryDescriptor.Builder().BuildConstructorFactory).BuildConstructorSpec
        ) { }

        public IReadOnlyList<SpecDescriptor> ExtractConstructorSpecForInjector(
                InjectorDescriptor injectorDescriptor,
                DescriptorGenerationContext context) {
            this.createConstructorSpecDescriptor(
                    injectorDescriptor.InjectorType,
                    ImmutableList<ITypeSymbol>.Empty,
                    context);
            
            // TODO: Identify constructor types and define the spec descriptor
            
            return ImmutableList<SpecDescriptor>.Empty;
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
