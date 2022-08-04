﻿// -----------------------------------------------------------------------------
//  <copyright file="SpecExtractor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Input {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Phx.Inject.Generator.Model;
    using Phx.Inject.Generator.Model.Specifications.Descriptors;

    internal class SpecExtractor {
        private readonly CreateSpecDescriptor createSpecDescriptor;

        public SpecExtractor(CreateSpecDescriptor createSpecDescriptor) {
            this.createSpecDescriptor = createSpecDescriptor;
        }

        public SpecExtractor() : this(
            new SpecDescriptor.Builder(
                    new SpecFactoryDescriptor.Builder().Build,
                    new SpecBuilderDescriptor.Builder().Build,
                    new SpecLinkDescriptor.Builder().Build).Build) { }

        public IReadOnlyList<SpecDescriptor> Extract(
                IEnumerable<TypeDeclarationSyntax> syntaxNodes,
                GeneratorExecutionContext context
        ) {
            var descriptorGenerationContext = new DescriptorGenerationContext(context);
            return SymbolProcessors.GetTypeSymbolsFromDeclarations(syntaxNodes, context)
                    .Where(IsSpecSymbol)
                    .Select(symbol => createSpecDescriptor(symbol, descriptorGenerationContext))
                    .ToImmutableList();
        }

        private static bool IsSpecSymbol(ITypeSymbol symbol) {
            var specificationAttribute = SymbolProcessors.GetSpecificationAttribute(symbol);
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
