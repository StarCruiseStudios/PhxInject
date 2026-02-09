// -----------------------------------------------------------------------------
// <copyright file="SpecFactoryPropertyTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Specification;

internal class SpecFactoryPropertyTransformer(
    ICodeElementValidator elementValidator,
    QualifierTransformer qualifierTransformer,
    FactoryAttributeTransformer factoryAttributeTransformer,
    PartialAttributeTransformer partialAttributeTransformer
) {
    public static readonly SpecFactoryPropertyTransformer Instance = new(
        new PropertyElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            hasGetter: true,
            hasSetter: false,
            requiredAttributes: ImmutableList.Create<IAttributeChecker>(FactoryAttributeTransformer.Instance),
            prohibitedAttributes: ImmutableList.Create<IAttributeChecker>(
                FactoryReferenceAttributeTransformer.Instance,
                BuilderAttributeTransformer.Instance,
                BuilderReferenceAttributeTransformer.Instance
            )
        ),
        QualifierTransformer.Instance,
        FactoryAttributeTransformer.Instance,
        PartialAttributeTransformer.Instance
    );

    public bool CanTransform(IPropertySymbol propertySymbol) {
        return elementValidator.IsValidSymbol(propertySymbol);
    }

    public SpecFactoryPropertyMetadata Transform(IPropertySymbol propertySymbol) {
        var factoryPropertyName = propertySymbol.Name;
        var returnTypeQualifier = qualifierTransformer.Transform(propertySymbol);
        var factoryReturnType = new QualifiedTypeMetadata(
            propertySymbol.Type.ToTypeModel(),
            returnTypeQualifier
        );

        var factoryAttribute = factoryAttributeTransformer.Transform(propertySymbol);
        var partialAttribute = partialAttributeTransformer.HasAttribute(propertySymbol)
            ? partialAttributeTransformer.Transform(propertySymbol)
            : null;

        return new SpecFactoryPropertyMetadata(
            factoryPropertyName,
            factoryReturnType,
            factoryAttribute,
            partialAttribute,
            propertySymbol.GetLocationOrDefault().GeneratorIgnored()
        );
    }
}
