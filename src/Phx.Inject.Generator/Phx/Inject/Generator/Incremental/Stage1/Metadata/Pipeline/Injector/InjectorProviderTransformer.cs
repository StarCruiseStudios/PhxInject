// -----------------------------------------------------------------------------
// <copyright file="InjectorProviderTransformer.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Injector;

/// <summary>
///     Transforms injector provider methods into metadata.
/// </summary>
internal sealed class InjectorProviderTransformer(
    ICodeElementValidator elementValidator,
    ITransformer<ISymbol, IQualifierMetadata> qualifierTransformer
) : ITransformer<IMethodSymbol, InjectorProviderMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static readonly InjectorProviderTransformer Instance = new(
        new MethodElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            isStatic: false,
            maxParameterCount: 0,
            returnsVoid: false,
            prohibitedAttributes: ImmutableList.Create(ChildInjectorAttributeTransformer.Instance)
        ),
        QualifierTransformer.Instance
    );

    /// <inheritdoc />
    public bool CanTransform(IMethodSymbol methodSymbol) {
        return elementValidator.IsValidSymbol(methodSymbol);
    }

    /// <inheritdoc />
    public IResult<InjectorProviderMetadata> Transform(IMethodSymbol methodSymbol) {
        return DiagnosticsRecorder.Capture(diagnostics => {
            var name = methodSymbol.Name;
            var providedType = methodSymbol.ReturnType.ToTypeModel();
            var qualifier = qualifierTransformer.Transform(methodSymbol).OrThrow(diagnostics);
            return new InjectorProviderMetadata(
                name,
                new QualifiedTypeMetadata(providedType, qualifier),
                methodSymbol.GetLocationOrDefault().GeneratorIgnored());
        });
    }
}
