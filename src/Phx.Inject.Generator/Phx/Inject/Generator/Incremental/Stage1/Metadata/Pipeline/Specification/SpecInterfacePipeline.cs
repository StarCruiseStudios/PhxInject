// -----------------------------------------------------------------------------
// <copyright file="SpecInterfacePipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Specification;

internal class SpecInterfacePipeline(
    ICodeElementValidator elementValidator,
    IAttributeTransformer<SpecificationAttributeMetadata> specificationAttributeTransformer,
    SpecFactoryMethodTransformer specFactoryMethodTransformer,
    SpecFactoryPropertyTransformer specFactoryPropertyTransformer,
    SpecFactoryReferenceTransformer specFactoryReferenceTransformer,
    SpecBuilderMethodTransformer specBuilderMethodTransformer,
    SpecBuilderReferenceTransformer specBuilderReferenceTransformer,
    LinkAttributeTransformer linkAttributeTransformer
) : ISyntaxValuesPipeline<SpecInterfaceMetadata> {
    public static readonly SpecInterfacePipeline Instance = new(
        new InterfaceElementValidator(
            CodeElementAccessibility.PublicOrInternal
        ),
        SpecificationAttributeTransformer.Instance,
        SpecFactoryMethodTransformer.Instance,
        SpecFactoryPropertyTransformer.Instance,
        SpecFactoryReferenceTransformer.Instance,
        SpecBuilderMethodTransformer.Instance,
        SpecBuilderReferenceTransformer.Instance,
        LinkAttributeTransformer.Instance);
    
    public IncrementalValuesProvider<SpecInterfaceMetadata> Select(SyntaxValueProvider syntaxProvider) {
        return syntaxProvider.ForAttributeWithMetadataName(
            SpecificationAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => {
                var targetSymbol = (ITypeSymbol)context.TargetSymbol;
                var specificationAttributeMetadata =
                    specificationAttributeTransformer.Transform(targetSymbol);

                var specInterfaceType = targetSymbol.ToTypeModel();
                
                var members = targetSymbol.GetMembers();
                var methods = members.OfType<IMethodSymbol>().ToImmutableList();
                var properties = members.OfType<IPropertySymbol>().ToImmutableList();
                var fields = members.OfType<IFieldSymbol>().ToImmutableList();
                
                var factoryMethods = methods
                    .Where(specFactoryMethodTransformer.CanTransform)
                    .Select(specFactoryMethodTransformer.Transform)
                    .ToImmutableArray();
                
                var factoryProperties = properties
                    .Where(specFactoryPropertyTransformer.CanTransform)
                    .Select(specFactoryPropertyTransformer.Transform)
                    .ToImmutableArray();
                
                var factoryReferences = properties
                    .Where(specFactoryReferenceTransformer.CanTransform)
                    .Select(specFactoryReferenceTransformer.Transform)
                    .Concat(fields
                        .Where(specFactoryReferenceTransformer.CanTransform)
                        .Select(specFactoryReferenceTransformer.Transform)
                    )
                    .ToImmutableArray();
                
                var builderMethods = methods
                    .Where(specBuilderMethodTransformer.CanTransform)
                    .Select(specBuilderMethodTransformer.Transform)
                    .ToImmutableArray();
                
                var builderReferences = properties
                    .Where(specBuilderReferenceTransformer.CanTransform)
                    .Select(specBuilderReferenceTransformer.Transform)
                    .Concat(fields
                        .Where(specBuilderReferenceTransformer.CanTransform)
                        .Select(specBuilderReferenceTransformer.Transform)
                    )
                    .ToImmutableArray();
                
                var links = linkAttributeTransformer.Transform(targetSymbol);
                
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
