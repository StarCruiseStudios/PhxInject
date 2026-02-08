// -----------------------------------------------------------------------------
// <copyright file="SpecInterfacePipeline.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Pipeline.Specification;

internal class SpecInterfacePipeline(
    ICodeElementValidator elementValidator,
    IAttributeTransformer<SpecificationAttributeMetadata> specificationAttributeTransformer
) : ISyntaxValuesPipeline<SpecInterfaceMetadata> {
    public static readonly SpecInterfacePipeline Instance = new(
        new InterfaceElementValidator(
            CodeElementAccessibility.PublicOrInternal
        ),
        SpecificationAttributeTransformer.Instance);
    
    public IncrementalValuesProvider<SpecInterfaceMetadata> Select(SyntaxValueProvider syntaxProvider) {
        return syntaxProvider.ForAttributeWithMetadataName(
            SpecificationAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => {
                var targetSymbol = (ITypeSymbol)context.TargetSymbol;
                var specificationAttributeMetadata =
                    specificationAttributeTransformer.Transform(targetSymbol);

                var specInterfaceType = targetSymbol.ToTypeModel();
                var factoryMethods = ImmutableArray<SpecFactoryMethodMetadata>.Empty;
                var factoryProperties = ImmutableArray<SpecFactoryPropertyMetadata>.Empty;
                var factoryReferences = ImmutableArray<SpecFactoryReferenceMetadata>.Empty;
                var builderMethods = ImmutableArray<SpecBuilderMethodMetadata>.Empty;
                var builderReferences = ImmutableArray<SpecBuilderReferenceMetadata>.Empty;
                var links = ImmutableArray<LinkAttributeMetadata>.Empty;
                return new SpecInterfaceMetadata(
                    specInterfaceType,
                    factoryMethods,
                    factoryProperties,
                    factoryReferences,
                    builderMethods,
                    builderReferences,
                    links,
                    specificationAttributeMetadata,
                    targetSymbol.GetLocationOrDefault().GeneratorIgnored()
                );
            });
    }
}
