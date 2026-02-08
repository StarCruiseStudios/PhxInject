// -----------------------------------------------------------------------------
// <copyright file="SpecBuilderMethodTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Types;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Pipeline.Specification;

internal class SpecBuilderMethodTransformer(
    ICodeElementValidator elementValidator,
    QualifierTransformer qualifierTransformer,
    BuilderAttributeTransformer builderAttributeTransformer
) {
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

    public bool CanTransform(IMethodSymbol methodSymbol) {
        return elementValidator.IsValidSymbol(methodSymbol);
    }

    public SpecBuilderMethodMetadata Transform(IMethodSymbol methodSymbol) {
        var builderMethodName = methodSymbol.Name;
        var builtTypeQualifier = qualifierTransformer.Transform(methodSymbol);
        var builtType = new QualifiedTypeMetadata(
            methodSymbol.Parameters[0].Type.ToTypeModel(),
            builtTypeQualifier
        );
        
        var parameters = methodSymbol.Parameters.Skip(1)
            .Select(param => {
                var paramQualifier = qualifierTransformer.Transform(param);
                return new QualifiedTypeMetadata(
                    param.Type.ToTypeModel(),
                    paramQualifier
                );
            })
            .ToImmutableList();

        var builderAttribute = builderAttributeTransformer.Transform(methodSymbol);

        return new SpecBuilderMethodMetadata(
            builderMethodName,
            builtType,
            parameters,
            builderAttribute,
            methodSymbol.GetLocationOrDefault().GeneratorIgnored()
        );
    }
}
