// -----------------------------------------------------------------------------
// <copyright file="SpecFactoryMethodTransformer.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Specification;

/// <summary>
/// Transforms specification factory methods into metadata.
/// </summary>
internal sealed class SpecFactoryMethodTransformer(
    ICodeElementValidator elementValidator,
    ITransformer<ISymbol, IQualifierMetadata> qualifierTransformer,
    FactoryAttributeTransformer factoryAttributeTransformer,
    PartialAttributeTransformer partialAttributeTransformer
) : ITransformer<IMethodSymbol, SpecFactoryMethodMetadata> {
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly SpecFactoryMethodTransformer Instance = new(
        MethodElementValidator.SpecificationFactoryMethod,
        QualifierTransformer.Instance,
        FactoryAttributeTransformer.Instance,
        PartialAttributeTransformer.Instance
    );

    /// <inheritdoc />
    public bool CanTransform(IMethodSymbol methodSymbol) {
        return elementValidator.IsValidSymbol(methodSymbol);
    }

    /// <inheritdoc />
    public IResult<SpecFactoryMethodMetadata> Transform(IMethodSymbol methodSymbol) {
        return DiagnosticsRecorder.Capture(diagnostics => {
            var factoryMethodName = methodSymbol.Name;
            var returnTypeQualifier = qualifierTransformer.Transform(methodSymbol).OrThrow(diagnostics);
            var factoryReturnType = methodSymbol.ReturnType.ToQualifiedTypeModel(returnTypeQualifier);

            var parameters = methodSymbol.Parameters
                .Select(param => {
                    var paramQualifier = qualifierTransformer.Transform(param).OrThrow(diagnostics);
                    return param.Type.ToQualifiedTypeModel(paramQualifier);
                })
                .ToEquatableList();

            var factoryAttribute = factoryAttributeTransformer.Transform(methodSymbol).OrThrow(diagnostics);
            var partialAttribute = partialAttributeTransformer
                .TransformOrNull(methodSymbol)?
                .OrThrow(diagnostics);

            return new SpecFactoryMethodMetadata(
                factoryMethodName,
                factoryReturnType,
                parameters,
                factoryAttribute,
                partialAttribute,
                methodSymbol.GetLocationOrDefault().GeneratorIgnored()
            );
        });
    }
}
