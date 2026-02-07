// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryPipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Auto;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Pipeline.Auto;

internal class AutoFactoryPipeline(
    IAttributeTransformer<AutoFactoryAttributeMetadata> autoFactoryAttributeTransformer
) : ISyntaxValuesPipeline<AutoFactoryMetadata> {
    public static readonly AutoFactoryPipeline Instance = new(
        AutoFactoryAttributeTransformer.Instance);
    
    public IncrementalValuesProvider<AutoFactoryMetadata> Select(SyntaxValueProvider syntaxProvider) {
        return syntaxProvider.ForAttributeWithMetadataName(
            AutoFactoryAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => {
                if (syntaxNode is ClassDeclarationSyntax { Modifiers: var modifiers }) {
                    return modifiers
                        .All(it => it.ValueText switch {
                            "private" or "protected" or "static" or "abstract" => false,
                            "internal" or "public" => true,
                            _ => true
                        });
                }
                
                return false;
            },
            (context, _) => {
                var targetSymbol = (ITypeSymbol)context.TargetSymbol;
                var autoFactoryAttributeMetadata =
                    autoFactoryAttributeTransformer.Transform(targetSymbol, context.Attributes);

                var autoFactoryType = new QualifiedTypeMetadata(targetSymbol.ToTypeModel(), NoQualifierMetadata.Instance);
                var parameters = ImmutableArray<QualifiedTypeMetadata>.Empty;
                var requiredProperties = ImmutableArray<AutoFactoryRequiredPropertyMetadata>.Empty;
                return new AutoFactoryMetadata(
                    autoFactoryType,
                    parameters,
                    requiredProperties,
                    autoFactoryAttributeMetadata,
                    targetSymbol.GetLocationOrDefault().GeneratorIgnored()
                );
            });
    }
}
