// -----------------------------------------------------------------------------
// <copyright file="AutoBuilderPipeline.cs" company="Star Cruise Studios LLC">
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

internal class AutoBuilderPipeline(
    ICodeElementValidator elementValidator,
    IAttributeTransformer<AutoBuilderAttributeMetadata> autoBuilderAttributeTransformer,
    ITransformer<ISymbol, IQualifierMetadata> qualifierTransformer
) : ISyntaxValuesPipeline<AutoBuilderMetadata> {
    public static readonly AutoBuilderPipeline Instance = new(
        new MethodElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            isAbstract: false
        ),
        AutoBuilderAttributeTransformer.Instance,
        QualifierTransformer.Instance);
    
    public IncrementalValuesProvider<IResult<AutoBuilderMetadata>> Select(
        SyntaxValueProvider syntaxProvider
    ) {
        return syntaxProvider.ForAttributeWithMetadataName(
            AutoBuilderAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => DiagnosticsRecorder.Capture(diagnostics => {
                var targetSymbol = (IMethodSymbol)context.TargetSymbol;
                var autoBuilderAttributeMetadata = autoBuilderAttributeTransformer
                    .Transform(targetSymbol)
                    .OrThrow(diagnostics);
                
                // The built type is the first parameter of the builder method
                // Get qualifier from the method itself (for Label attribute)
                var builtTypeQualifier = qualifierTransformer.Transform(targetSymbol).OrThrow(diagnostics);
                var builtType = targetSymbol.Parameters[0].Type.ToQualifiedTypeModel(builtTypeQualifier);
                
                // Remaining parameters (skip first parameter which is the built type)
                var parameters = targetSymbol.Parameters.Skip(1)
                    .Select(param => {
                        var paramQualifier = qualifierTransformer.Transform(param).OrThrow(diagnostics);
                        return param.Type.ToQualifiedTypeModel(paramQualifier);
                    })
                    .ToEquatableList();

                return new AutoBuilderMetadata(
                    targetSymbol.Name,
                    builtType,
                    parameters,
                    autoBuilderAttributeMetadata,
                    targetSymbol.GetLocationOrDefault().GeneratorIgnored()
                );
            }));
    }
}
