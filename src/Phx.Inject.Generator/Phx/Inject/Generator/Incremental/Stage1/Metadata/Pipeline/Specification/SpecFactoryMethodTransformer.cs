// -----------------------------------------------------------------------------
// <copyright file="SpecFactoryMethodTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

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

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Specification;

internal class SpecFactoryMethodTransformer(
    ICodeElementValidator elementValidator,
    QualifierTransformer qualifierTransformer,
    FactoryAttributeTransformer factoryAttributeTransformer,
    PartialAttributeTransformer partialAttributeTransformer
) {
    public static readonly SpecFactoryMethodTransformer Instance = new(
        new MethodElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            returnsVoid: false,
            requiredAttributes: ImmutableList.Create<IAttributeChecker>(FactoryAttributeTransformer.Instance),
            prohibitedAttributes: ImmutableList.Create<IAttributeChecker>(BuilderAttributeTransformer.Instance)
        ),
        QualifierTransformer.Instance,
        FactoryAttributeTransformer.Instance,
        PartialAttributeTransformer.Instance
    );

    public bool CanTransform(IMethodSymbol methodSymbol) {
        return elementValidator.IsValidSymbol(methodSymbol);
    }

    public IResult<SpecFactoryMethodMetadata> Transform(IMethodSymbol methodSymbol) {
        return DiagnosticsRecorder.Capture(diagnostics => {
            var factoryMethodName = methodSymbol.Name;
            var returnTypeQualifier = qualifierTransformer.Transform(methodSymbol).GetOrThrow(diagnostics);
            var factoryReturnType = new QualifiedTypeMetadata(
                methodSymbol.ReturnType.ToTypeModel(),
                returnTypeQualifier
            );

            var parameters = methodSymbol.Parameters
                .Select(param => {
                    var paramQualifier = qualifierTransformer.Transform(param).GetOrThrow(diagnostics);
                    return new QualifiedTypeMetadata(
                        param.Type.ToTypeModel(),
                        paramQualifier
                    );
                })
                .ToImmutableList();

            var factoryAttribute = factoryAttributeTransformer.Transform(methodSymbol).GetOrThrow(diagnostics);
            var partialAttribute = partialAttributeTransformer.HasAttribute(methodSymbol)
                ? partialAttributeTransformer.Transform(methodSymbol).GetOrThrow(diagnostics)
                : null;

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
