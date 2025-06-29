// -----------------------------------------------------------------------------
//  <copyright file="InjectorExtractor.cs" company="Star Cruise Studios LLC">
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

    internal class InjectorExtractor {
        private readonly InjectorDescriptor.IBuilder injectorDescriptorBuilder;

        public InjectorExtractor(InjectorDescriptor.IBuilder injectorDescriptorBuilder) {
            this.injectorDescriptorBuilder = injectorDescriptorBuilder;
        }

        public InjectorExtractor() : this(new InjectorDescriptor.Builder()) { }

        public IReadOnlyList<InjectorDescriptor> Extract(
            IEnumerable<TypeDeclarationSyntax> syntaxNodes,
            DescriptorGenerationContext context
        ) {
            return MetadataHelpers.GetTypeSymbolsFromDeclarations(syntaxNodes, context.GenerationContext)
                .Where(IsInjectorSymbol)
                .Select(symbol => injectorDescriptorBuilder.Build(symbol, context))
                .ToImmutableList();
        }

        private static bool IsInjectorSymbol(ITypeSymbol symbol) {
            var injectorAttribute = symbol.GetInjectorAttribute();
            if (injectorAttribute == null) {
                return false;
            }

            if (symbol.TypeKind != TypeKind.Interface) {
                throw new InjectionException(
                    Diagnostics.InvalidSpecification,
                    $"Injector type {symbol.Name} must be an interface.",
                    symbol.Locations.First());
            }

            return true;
        }
    }
}
