// -----------------------------------------------------------------------------
// <copyright file="InjectorPipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Stage1.Metadata;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Pipeline.Injector;

internal class InjectorInterfacePipeline(
    IAttributeTransformer<InjectorAttributeMetadata> injectorAttributeTransformer
) : ISyntaxValuesPipeline<InjectorInterfaceMetadata> {
    public static readonly InjectorInterfacePipeline Instance = new(
        InjectorAttributeTransformer.Instance);
    
    public IncrementalValuesProvider<InjectorInterfaceMetadata> Select(SyntaxValueProvider syntaxProvider) {
        return syntaxProvider.ForAttributeWithMetadataName(
            PhxInjectAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => {
                if (syntaxNode is InterfaceDeclarationSyntax { Modifiers: var modifiers }) {
                    return modifiers
                        .All(it => it.ValueText switch {
                            "private" or "protected" => false,
                            "internal" or "public" => true,
                            _ => true
                        });
                }

                return false;
            },
            (context, _) => {
                var targetSymbol = (ITypeSymbol)context.TargetSymbol;
                var injectorAttributeMetadata =
                    injectorAttributeTransformer.Transform(targetSymbol, context.Attributes);

                var injectorInterfaceType = targetSymbol.ToTypeModel();
                var providers = ImmutableArray<InjectorProviderMetadata>.Empty;
                var activators = ImmutableArray<InjectorActivatorMetadata>.Empty;
                var childFactories = ImmutableArray<InjectorChildProviderMetadata>.Empty;
                DependencyAttributeMetadata? dependencyAttributeMetadata = null;
                return new InjectorInterfaceMetadata(
                    injectorInterfaceType,
                    providers,
                    activators,
                    childFactories,
                    injectorAttributeMetadata,
                    dependencyAttributeMetadata,
                    targetSymbol.GetLocationOrDefault().GeneratorIgnored()
                );
            });
    }
}