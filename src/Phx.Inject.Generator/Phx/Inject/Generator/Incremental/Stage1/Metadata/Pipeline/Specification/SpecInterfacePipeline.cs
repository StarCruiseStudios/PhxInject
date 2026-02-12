// -----------------------------------------------------------------------------
// <copyright file="SpecInterfacePipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Specification;

internal class SpecInterfacePipeline(
    ICodeElementValidator elementValidator,
    IAttributeTransformer<SpecificationAttributeMetadata> specificationAttributeTransformer,
    ITransformer<IMethodSymbol, SpecFactoryMethodMetadata> specFactoryMethodTransformer,
    ITransformer<IPropertySymbol, SpecFactoryPropertyMetadata> specFactoryPropertyTransformer,
    ITransformer<ISymbol, SpecFactoryReferenceMetadata> specFactoryReferenceTransformer,
    ITransformer<IMethodSymbol, SpecBuilderMethodMetadata> specBuilderMethodTransformer,
    ITransformer<ISymbol, SpecBuilderReferenceMetadata> specBuilderReferenceTransformer,
    IAttributeListTransformer<LinkAttributeMetadata> linkAttributeTransformer
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
    
    public IncrementalValuesProvider<IResult<SpecInterfaceMetadata>> Select(
        SyntaxValueProvider syntaxProvider
    ) {
        return syntaxProvider.ForAttributeWithMetadataName(
            SpecificationAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => DiagnosticsRecorder.Capture(diagnostics => {
                var targetSymbol = (ITypeSymbol)context.TargetSymbol;
                var specificationAttributeMetadata = specificationAttributeTransformer
                    .Transform(targetSymbol)
                    .OrThrow(diagnostics);

                var specInterfaceType = targetSymbol.ToTypeModel();
                
                var members = targetSymbol.GetMembers();
                var methods = members.OfType<IMethodSymbol>().ToImmutableList();
                var properties = members.OfType<IPropertySymbol>().ToImmutableList();
                var fields = members.OfType<IFieldSymbol>().ToImmutableList();
                
                var factoryMethods = methods
                    .Where(specFactoryMethodTransformer.CanTransform)
                    .Select(specFactoryMethodTransformer.Transform)
                    .SelectOrThrow(diagnostics)
                    .ToEquatableList();
                
                var factoryProperties = properties
                    .Where(specFactoryPropertyTransformer.CanTransform)
                    .Select(specFactoryPropertyTransformer.Transform)
                    .SelectOrThrow(diagnostics)
                    .ToEquatableList();
                
                var factoryReferences = properties
                    .Where(specFactoryReferenceTransformer.CanTransform)
                    .Select(specFactoryReferenceTransformer.Transform)
                    .SelectOrThrow(diagnostics)
                    .Concat(fields
                        .Where(specFactoryReferenceTransformer.CanTransform)
                        .Select(specFactoryReferenceTransformer.Transform)
                        .SelectOrThrow(diagnostics))
                    .ToEquatableList();
                
                var builderMethods = methods
                    .Where(specBuilderMethodTransformer.CanTransform)
                    .Select(specBuilderMethodTransformer.Transform)
                    .SelectOrThrow(diagnostics)
                    .ToEquatableList();
                
                var builderReferences = properties
                    .Where(specBuilderReferenceTransformer.CanTransform)
                    .Select(specBuilderReferenceTransformer.Transform)
                    .SelectOrThrow(diagnostics)
                    .Concat(fields
                        .Where(specBuilderReferenceTransformer.CanTransform)
                        .Select(specBuilderReferenceTransformer.Transform)
                        .SelectOrThrow(diagnostics)
                    )
                    .ToEquatableList();
                
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
            }));
    }
}
