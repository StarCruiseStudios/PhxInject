// -----------------------------------------------------------------------------
// <copyright file="SpecClassPipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
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

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Specification;

internal class SpecClassPipeline(
    ICodeElementValidator elementValidator,
    IAttributeTransformer<SpecificationAttributeMetadata> specificationAttributeTransformer,
    SpecFactoryMethodTransformer specFactoryMethodTransformer,
    SpecFactoryPropertyTransformer specFactoryPropertyTransformer,
    SpecFactoryReferenceTransformer specFactoryReferenceTransformer,
    SpecBuilderMethodTransformer specBuilderMethodTransformer,
    SpecBuilderReferenceTransformer specBuilderReferenceTransformer,
    LinkAttributeTransformer linkAttributeTransformer
) : ISyntaxValuesPipeline<SpecClassMetadata> {
    public static readonly SpecClassPipeline Instance = new(
        new ClassElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            isStatic: true
        ),
        SpecificationAttributeTransformer.Instance,
        SpecFactoryMethodTransformer.Instance,
        SpecFactoryPropertyTransformer.Instance,
        SpecFactoryReferenceTransformer.Instance,
        SpecBuilderMethodTransformer.Instance,
        SpecBuilderReferenceTransformer.Instance,
        LinkAttributeTransformer.Instance);
    
    public IncrementalValuesProvider<IResult<SpecClassMetadata>> Select(
        SyntaxValueProvider syntaxProvider
    ) {
        return syntaxProvider.ForAttributeWithMetadataName(
            SpecificationAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => DiagnosticsRecorder.Capture(diagnostics => {
                var targetSymbol = (ITypeSymbol)context.TargetSymbol;
                SpecificationAttributeMetadata? specificationAttributeMetadata = null;
                try {
                    specificationAttributeMetadata = specificationAttributeTransformer.Transform(targetSymbol);
                } catch (Exception ex) {
                    throw new GeneratorException(new DiagnosticInfo(
                        Diagnostics.DiagnosticType.UnexpectedError,
                        $"Error transforming Specification attribute: {ex.Message}",
                        LocationInfo.CreateFrom(targetSymbol.GetLocationOrDefault())
                    ));
                }

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
                    .Select(s => {
                        try {
                            return specFactoryReferenceTransformer.Transform(s);
                        } catch (Exception ex) {
                            throw new GeneratorException(new DiagnosticInfo(
                                Diagnostics.DiagnosticType.UnexpectedError,
                                $"Error transforming factory reference: {ex.Message}",
                                LocationInfo.CreateFrom(s.GetLocationOrDefault())
                            ));
                        }
                    })
                    .Concat(fields
                        .Where(specFactoryReferenceTransformer.CanTransform)
                        .Select(s => {
                            try {
                                return specFactoryReferenceTransformer.Transform(s);
                            } catch (Exception ex) {
                                throw new GeneratorException(new DiagnosticInfo(
                                    Diagnostics.DiagnosticType.UnexpectedError,
                                    $"Error transforming factory reference: {ex.Message}",
                                    LocationInfo.CreateFrom(s.GetLocationOrDefault())
                                ));
                            }
                        })
                    )
                    .ToImmutableArray();
                
                var builderMethods = methods
                    .Where(specBuilderMethodTransformer.CanTransform)
                    .Select(specBuilderMethodTransformer.Transform)
                    .ToImmutableArray();
                
                var builderReferences = properties
                    .Where(specBuilderReferenceTransformer.CanTransform)
                    .Select(s => {
                        try {
                            return specBuilderReferenceTransformer.Transform(s);
                        } catch (Exception ex) {
                            throw new GeneratorException(new DiagnosticInfo(
                                Diagnostics.DiagnosticType.UnexpectedError,
                                $"Error transforming builder reference: {ex.Message}",
                                LocationInfo.CreateFrom(s.GetLocationOrDefault())
                            ));
                        }
                    })
                    .Concat(fields
                        .Where(specBuilderReferenceTransformer.CanTransform)
                        .Select(s => {
                            try {
                                return specBuilderReferenceTransformer.Transform(s);
                            } catch (Exception ex) {
                                throw new GeneratorException(new DiagnosticInfo(
                                    Diagnostics.DiagnosticType.UnexpectedError,
                                    $"Error transforming builder reference: {ex.Message}",
                                    LocationInfo.CreateFrom(s.GetLocationOrDefault())
                                ));
                            }
                        })
                    )
                    .ToImmutableArray();
                
                var links = linkAttributeTransformer.Transform(targetSymbol);
                
                return new SpecClassMetadata(
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
