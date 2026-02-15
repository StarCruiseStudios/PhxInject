// -----------------------------------------------------------------------------
// <copyright file="AutoBuilderTransformer.cs" company="Star Cruise Studios LLC">
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
///     Transforms AutoBuilder method declarations into metadata.
/// </summary>
internal sealed class AutoBuilderTransformer(
    ICodeElementValidator elementValidator,
    IAttributeTransformer<AutoBuilderAttributeMetadata> autoBuilderAttributeTransformer,
    ITransformer<ISymbol, IQualifierMetadata> qualifierTransformer
) : ITransformer<IMethodSymbol, AutoBuilderMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static readonly AutoBuilderTransformer Instance = new(
        new MethodElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            isAbstract: false
        ),
        AutoBuilderAttributeTransformer.Instance,
        QualifierTransformer.Instance);

    /// <inheritdoc />
    public bool CanTransform(IMethodSymbol methodSymbol) {
        return elementValidator.IsValidSymbol(methodSymbol);
    }

    /// <inheritdoc />
    public IResult<AutoBuilderMetadata> Transform(IMethodSymbol methodSymbol) {
        return DiagnosticsRecorder.Capture(diagnostics => {
            var autoBuilderAttributeMetadata = autoBuilderAttributeTransformer
                .Transform(methodSymbol)
                .OrThrow(diagnostics);
            
            // The built type is the first parameter of the builder method
            // Get qualifier from the method itself (for Label attribute)
            var builtTypeQualifier = qualifierTransformer.Transform(methodSymbol).OrThrow(diagnostics);
            var builtType = methodSymbol.Parameters[0].Type.ToQualifiedTypeModel(builtTypeQualifier);
            
            // Remaining parameters (skip first parameter which is the built type)
            var parameters = methodSymbol.Parameters.Skip(1)
                .Select(param => {
                    var paramQualifier = qualifierTransformer.Transform(param).OrThrow(diagnostics);
                    return param.Type.ToQualifiedTypeModel(paramQualifier);
                })
                .ToEquatableList();

            return new AutoBuilderMetadata(
                methodSymbol.Name,
                builtType,
                parameters,
                autoBuilderAttributeMetadata,
                methodSymbol.GetLocationOrDefault().GeneratorIgnored()
            );
        });
    }
}
