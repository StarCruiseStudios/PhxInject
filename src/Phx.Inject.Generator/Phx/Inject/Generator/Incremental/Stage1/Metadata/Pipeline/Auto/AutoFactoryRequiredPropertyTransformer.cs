// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryRequiredPropertyTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Auto;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Auto;

internal class AutoFactoryRequiredPropertyTransformer(
    ICodeElementValidator elementValidator,
    ICodeElementValidator setterElementValidator,
    QualifierTransformer qualifierTransformer
) {
    public static readonly AutoFactoryRequiredPropertyTransformer Instance = new(
        new PropertyElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            hasSetter: true,
            isRequired: true
        ),
        new MethodElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            MethodKindFilter.Setter
        ),
        QualifierTransformer.Instance
    );

    public bool CanTransform(IPropertySymbol propertySymbol) {
        if (!elementValidator.IsValidSymbol(propertySymbol)) {
            return false;
        }

        var setMethod = propertySymbol.SetMethod;
        return setMethod != null && setterElementValidator.IsValidSymbol(setMethod);
    }

    public IResult<AutoFactoryRequiredPropertyMetadata> Transform(IPropertySymbol propertySymbol) {
        return DiagnosticsRecorder.Capture(diagnostics => {
            var propertyQualifier = qualifierTransformer.Transform(propertySymbol).GetOrThrow(diagnostics);
            return new AutoFactoryRequiredPropertyMetadata(
                propertySymbol.Name,
                new QualifiedTypeMetadata(
                    propertySymbol.Type.ToTypeModel(),
                    propertyQualifier
                ),
                propertySymbol.GetLocationOrDefault().GeneratorIgnored()
            );
        });
    }
}
