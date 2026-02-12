// -----------------------------------------------------------------------------
// <copyright file="InjectorActivatorTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

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

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Injector;

internal class InjectorActivatorTransformer(
    ICodeElementValidator elementValidator,
    ITransformer<ISymbol, IQualifierMetadata> qualifierTransformer
) : ITransformer<IMethodSymbol, InjectorActivatorMetadata> {
    public static readonly InjectorActivatorTransformer Instance = new(
        new MethodElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            isStatic: false,
            minParameterCount: 1,
            maxParameterCount: 1,
            returnsVoid: true,
            requiredAttributes: ImmutableList.Create(ChildInjectorAttributeTransformer.Instance)
        ),
        QualifierTransformer.Instance
    );

    public bool CanTransform(IMethodSymbol methodSymbol) {
        return elementValidator.IsValidSymbol(methodSymbol);
    }

    public IResult<InjectorActivatorMetadata> Transform(IMethodSymbol methodSymbol) {
        return DiagnosticsRecorder.Capture(diagnostics => {
            var name = methodSymbol.Name;
            var activatedType = methodSymbol.Parameters[0].Type.ToTypeModel();
            var qualifier = qualifierTransformer.Transform(methodSymbol).OrThrow(diagnostics);
            return new InjectorActivatorMetadata(
                name,
                new QualifiedTypeMetadata(activatedType, qualifier),
                methodSymbol.GetLocationOrDefault().GeneratorIgnored());
        });
    }
}
