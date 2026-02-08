// -----------------------------------------------------------------------------
// <copyright file="AutoBuilderPipeline.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Types;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Pipeline.Auto;

internal class AutoBuilderPipeline(
    ICodeElementValidator elementValidator,
    IAttributeTransformer<AutoBuilderAttributeMetadata> autoBuilderAttributeTransformer,
    QualifierTransformer qualifierTransformer
) : ISyntaxValuesPipeline<AutoBuilderMetadata> {
    public static readonly AutoBuilderPipeline Instance = new(
        new MethodElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            isAbstract: false
        ),
        AutoBuilderAttributeTransformer.Instance,
        QualifierTransformer.Instance);
    
    public IncrementalValuesProvider<AutoBuilderMetadata> Select(SyntaxValueProvider syntaxProvider) {
        return syntaxProvider.ForAttributeWithMetadataName(
            AutoBuilderAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => {
                var targetSymbol = (IMethodSymbol)context.TargetSymbol;
                var autoBuilderAttributeMetadata =
                    autoBuilderAttributeTransformer.Transform(targetSymbol);

                // The built type is the first parameter of the builder method
                // Get qualifier from the method itself (for Label attribute)
                var builtTypeQualifier = qualifierTransformer.Transform(targetSymbol);
                var builtType = new QualifiedTypeMetadata(
                    targetSymbol.Parameters[0].Type.ToTypeModel(),
                    builtTypeQualifier
                );
                
                // Remaining parameters (skip first parameter which is the built type)
                var parameters = targetSymbol.Parameters.Skip(1)
                    .Select(param => {
                        var paramQualifier = qualifierTransformer.Transform(param);
                        return new QualifiedTypeMetadata(
                            param.Type.ToTypeModel(),
                            paramQualifier
                        );
                    })
                    .ToImmutableArray();

                return new AutoBuilderMetadata(
                    targetSymbol.Name,
                    builtType,
                    parameters,
                    autoBuilderAttributeMetadata,
                    targetSymbol.GetLocationOrDefault().GeneratorIgnored()
                );
            });
    }
}
