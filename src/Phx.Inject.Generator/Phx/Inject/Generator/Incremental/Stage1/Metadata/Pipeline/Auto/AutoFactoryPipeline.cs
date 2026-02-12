// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryPipeline.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Auto;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Auto;

internal class AutoFactoryPipeline(
    ICodeElementValidator elementValidator,
    ICodeElementValidator constructorValidator,
    IAttributeTransformer<AutoFactoryAttributeMetadata> autoFactoryAttributeTransformer,
    ITransformer<ISymbol, IQualifierMetadata> qualifierTransformer,
    ITransformer<IPropertySymbol, AutoFactoryRequiredPropertyMetadata> autoFactoryRequiredPropertyTransformer
) : ISyntaxValuesPipeline<AutoFactoryMetadata> {
    public static readonly AutoFactoryPipeline Instance = new(
        new ClassElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            isStatic: false,
            isAbstract: false
        ),
        new MethodElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            MethodKindFilter.Constructor
        ),
        AutoFactoryAttributeTransformer.Instance,
        QualifierTransformer.Instance,
        AutoFactoryRequiredPropertyTransformer.Instance);
    
    public IncrementalValuesProvider<IResult<AutoFactoryMetadata>> Select(
        SyntaxValueProvider syntaxProvider
    ) {
        return syntaxProvider.ForAttributeWithMetadataName(
            AutoFactoryAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => DiagnosticsRecorder.Capture(diagnostics => {
                var targetSymbol = (ITypeSymbol)context.TargetSymbol;
                var autoFactoryAttributeMetadata = autoFactoryAttributeTransformer
                    .Transform(targetSymbol)
                    .OrThrow(diagnostics);

                var autoFactoryType = targetSymbol.ToQualifiedTypeModel(NoQualifierMetadata.Instance);
                
                // Extract constructor parameters
                var constructors = targetSymbol.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(constructorValidator.IsValidSymbol)
                    .ToList();

                var parameters = ImmutableArray<QualifiedTypeMetadata>.Empty;
                if (constructors.Count == 1) {
                    var constructor = constructors[0];
                    parameters = constructor.Parameters
                        .Select(param => {
                            var paramQualifier = qualifierTransformer.Transform(param).OrThrow(diagnostics);
                            return param.Type.ToQualifiedTypeModel(paramQualifier);
                        })
                        .ToImmutableArray();
                }
                
                // Extract required properties
                var requiredProperties = targetSymbol.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(autoFactoryRequiredPropertyTransformer.CanTransform)
                    .Select(autoFactoryRequiredPropertyTransformer.Transform)
                    .SelectOrThrow(diagnostics)
                    .ToImmutableArray();

                return new AutoFactoryMetadata(
                    autoFactoryType,
                    parameters,
                    requiredProperties,
                    autoFactoryAttributeMetadata,
                    targetSymbol.GetLocationOrDefault().GeneratorIgnored()
                );
            }));
    }
}
