// -----------------------------------------------------------------------------
// <copyright file="SpecBuilderMethodTransformer.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Specification;

/// <summary>
/// Transforms specification builder methods into metadata.
/// </summary>
internal sealed class SpecBuilderMethodTransformer(
    ICodeElementValidator elementValidator,
    ITransformer<ISymbol, IQualifierMetadata> qualifierTransformer,
    IAttributeTransformer<BuilderAttributeMetadata> builderAttributeTransformer
) : ITransformer<IMethodSymbol, SpecBuilderMethodMetadata> {
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly SpecBuilderMethodTransformer Instance = new(
        new MethodElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            minParameterCount: 1,
            returnsVoid: true,
            requiredAttributes: ImmutableList.Create<IAttributeChecker>(BuilderAttributeTransformer.Instance),
            prohibitedAttributes: ImmutableList.Create<IAttributeChecker>(FactoryAttributeTransformer.Instance)
        ),
        QualifierTransformer.Instance,
        BuilderAttributeTransformer.Instance
    );

    /// <inheritdoc />
    public bool CanTransform(IMethodSymbol methodSymbol) {
        return elementValidator.IsValidSymbol(methodSymbol);
    }

    /// <inheritdoc />
    public IResult<SpecBuilderMethodMetadata> Transform(IMethodSymbol methodSymbol) {
        return DiagnosticsRecorder.Capture(diagnostics => {
            var builderMethodName = methodSymbol.Name;
            var builtTypeQualifier = qualifierTransformer.Transform(methodSymbol).OrThrow(diagnostics);
            var builtType = methodSymbol.Parameters[0].Type.ToQualifiedTypeModel(builtTypeQualifier);

            var parameters = methodSymbol.Parameters.Skip(1)
                .Select(param => {
                    var paramQualifier = qualifierTransformer.Transform(param).OrThrow(diagnostics);
                    return param.Type.ToQualifiedTypeModel(paramQualifier);
                })
                .ToEquatableList();

            var builderAttribute = builderAttributeTransformer.Transform(methodSymbol).OrThrow(diagnostics);

            return new SpecBuilderMethodMetadata(
                builderMethodName,
                builtType,
                parameters,
                builderAttribute,
                methodSymbol.GetLocationOrDefault().GeneratorIgnored()
            );
        });
    }
}
