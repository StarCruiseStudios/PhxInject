// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyExtractor.cs" company="Star Cruise Studios LLC">
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

    internal class ExternalDependencyExtractor {
        private readonly ExternalDependencyDescriptor.IBuilder externalDependencyDescriptorBuilder;

        public ExternalDependencyExtractor(ExternalDependencyDescriptor.IBuilder externalDependencyDescriptorBuilder) {
            this.externalDependencyDescriptorBuilder = externalDependencyDescriptorBuilder;
        }

        public ExternalDependencyExtractor() : this(
            new ExternalDependencyDescriptor.Builder(new ExternalDependencyProviderDescriptor.Builder())) { }

        public IReadOnlyList<ExternalDependencyDescriptor> Extract(
            IEnumerable<TypeDeclarationSyntax> syntaxNodes,
            DescriptorGenerationContext context
        ) {
            return MetadataHelpers.GetTypeSymbolsFromDeclarations(syntaxNodes, context.GenerationContext)
                .SelectMany(MetadataHelpers.GetExternalDependencyTypes)
                .GroupBy(typeSymbol => typeSymbol, SymbolEqualityComparer.Default)
                .Select(group => group.First())
                .Where(IsExternalDependencySymbol)
                .Select(type => externalDependencyDescriptorBuilder.Build(type, context))
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
