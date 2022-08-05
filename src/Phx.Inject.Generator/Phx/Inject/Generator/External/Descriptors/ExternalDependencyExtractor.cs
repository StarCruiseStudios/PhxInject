// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyExtractor.cs" company="Star Cruise Studios LLC">
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
    using Phx.Inject.Generator.Model.External.Descriptors;

    internal class ExternalDependencyExtractor {
        private readonly CreateExternalDependencyDescriptor createExternalDependency;

        public ExternalDependencyExtractor(CreateExternalDependencyDescriptor createExternalDependency) {
            this.createExternalDependency = createExternalDependency;
        }

        public ExternalDependencyExtractor() : this(
                new ExternalDependencyDescriptor.Builder(new ExternalDependencyProviderDescriptor.Builder().Build)
                        .Build) { }

        public IReadOnlyList<ExternalDependencyDescriptor> Extract(
                IEnumerable<TypeDeclarationSyntax> syntaxNodes,
                DescriptorGenerationContext context
        ) {
            return SymbolProcessors.GetTypeSymbolsFromDeclarations(syntaxNodes, context.GenerationContext)
                    .SelectMany(SymbolProcessors.GetExternalDependencyTypes)
                    .GroupBy(typeSymbol => typeSymbol, SymbolEqualityComparer.Default)
                    .Select(group => group.First())
                    .Where(IsExternalDependencySymbol)
                    .Select(type => createExternalDependency(type, context))
                    .ToImmutableList();
        }

        private static bool IsExternalDependencySymbol(ITypeSymbol symbol) {
            if (symbol.TypeKind != TypeKind.Interface) {
                throw new InjectionException(
                        Diagnostics.InvalidSpecification,
                        $"External Dependency type {symbol.Name} must be an interface.",
                        symbol.Locations.First());
            }

            return true;
        }
    }
}
