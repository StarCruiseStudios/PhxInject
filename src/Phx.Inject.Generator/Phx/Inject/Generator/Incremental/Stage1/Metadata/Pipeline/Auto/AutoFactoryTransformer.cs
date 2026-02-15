// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

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

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Auto;

/// <summary>
///     Transforms AutoFactory class declarations into metadata.
/// </summary>
internal sealed class AutoFactoryTransformer(
    ICodeElementValidator elementValidator,
    ICodeElementValidator constructorValidator,
    IAttributeTransformer<AutoFactoryAttributeMetadata> autoFactoryAttributeTransformer,
    ITransformer<ISymbol, IQualifierMetadata> qualifierTransformer,
    ITransformer<IPropertySymbol, AutoFactoryRequiredPropertyMetadata> autoFactoryRequiredPropertyTransformer
) : ITransformer<ITypeSymbol, AutoFactoryMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static readonly AutoFactoryTransformer Instance = new(
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

    /// <inheritdoc />
    public bool CanTransform(ITypeSymbol typeSymbol) {
        return elementValidator.IsValidSymbol(typeSymbol);
    }

    /// <inheritdoc />
    public IResult<AutoFactoryMetadata> Transform(ITypeSymbol typeSymbol) {
        return DiagnosticsRecorder.Capture(diagnostics => {
            var autoFactoryAttributeMetadata = autoFactoryAttributeTransformer
                .Transform(typeSymbol)
                .OrThrow(diagnostics);

            var autoFactoryType = typeSymbol.ToQualifiedTypeModel(NoQualifierMetadata.Instance);
            
            // Extract constructor parameters
            var constructors = typeSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(constructorValidator.IsValidSymbol)
                .ToList();

            var parameters = EquatableList<QualifiedTypeMetadata>.Empty;
            if (constructors.Count == 1) {
                var constructor = constructors[0];
                parameters = constructor.Parameters
                    .Select(param => {
                        var paramQualifier = qualifierTransformer.Transform(param).OrThrow(diagnostics);
                        return param.Type.ToQualifiedTypeModel(paramQualifier);
                    })
                    .ToEquatableList();
            }
            
            // Extract required properties
            var requiredProperties = typeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(autoFactoryRequiredPropertyTransformer.CanTransform)
                .Select(autoFactoryRequiredPropertyTransformer.Transform)
                .SelectOrThrow(diagnostics)
                .ToEquatableList();

            return new AutoFactoryMetadata(
                autoFactoryType,
                parameters,
                requiredProperties,
                autoFactoryAttributeMetadata,
                typeSymbol.GetLocationOrDefault().GeneratorIgnored()
            );
        });
    }
}
