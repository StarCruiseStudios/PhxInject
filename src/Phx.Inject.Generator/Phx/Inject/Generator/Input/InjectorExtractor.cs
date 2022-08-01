﻿// -----------------------------------------------------------------------------
//  <copyright file="InjectorExtractor.cs" company="Star Cruise Studios LLC">
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
    using Phx.Inject.Generator.Model.Descriptors;

    internal class InjectorExtractor {

        private readonly CreateInjectorDescriptor createInjectorDescriptor;

        public InjectorExtractor(CreateInjectorDescriptor createInjectorDescriptor) {
            this.createInjectorDescriptor = createInjectorDescriptor;
        }

        public InjectorExtractor() : this(
                new InjectorDescriptor.Builder(
                        new InjectorProviderDescriptor.Builder().Build,
                        new InjectorBuilderDescriptor.Builder().Build).Build) { }

        public IReadOnlyList<InjectorDescriptor> Extract(
                IEnumerable<TypeDeclarationSyntax> syntaxNodes,
                GeneratorExecutionContext context,
                IReadOnlyDictionary<TypeModel, SpecDescriptor> specDescriptors
        ) {
            return SymbolProcessors.GetTypeSymbolsFromDeclarations(syntaxNodes, context)
                    .Where(IsInjectorSymbol)
                    .Select(symbol => createInjectorDescriptor(symbol, specDescriptors))
                    .ToImmutableList();
        }

        private static bool IsInjectorSymbol(ITypeSymbol symbol) {
            var injectorAttribute = SymbolProcessors.GetInjectorAttribute(symbol);
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