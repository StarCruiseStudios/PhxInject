// -----------------------------------------------------------------------------
// <copyright file="InjectorChildProviderTransformer.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Injector;

internal class InjectorChildProviderTransformer(
    ICodeElementValidator elementValidator,
    ChildInjectorAttributeTransformer childInjectorAttributeTransformer
) : ITransformer<IMethodSymbol, InjectorChildProviderMetadata> {
    public static readonly InjectorChildProviderTransformer Instance = new(
        new MethodElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            isStatic: false,
            returnsVoid: false,
            requiredAttributes: ImmutableList.Create<IAttributeChecker>(ChildInjectorAttributeTransformer.Instance)
        ),
        ChildInjectorAttributeTransformer.Instance
    );

    public bool CanTransform(IMethodSymbol methodSymbol) {
        return elementValidator.IsValidSymbol(methodSymbol);
    }

    public IResult<InjectorChildProviderMetadata> Transform(IMethodSymbol methodSymbol) {
        return DiagnosticsRecorder.Capture(diagnostics => {
            var name = methodSymbol.Name;
            var childInjectorAttribute =
                childInjectorAttributeTransformer.Transform(methodSymbol).OrThrow(diagnostics);
            var childInjectorType = methodSymbol.ReturnType.ToTypeModel();
            var parameters = methodSymbol.Parameters
                .Select(p => p.Type.ToTypeModel())
                .ToEquatableList();

            return new InjectorChildProviderMetadata(
                name,
                childInjectorType,
                parameters,
                childInjectorAttribute,
                methodSymbol.GetLocationOrDefault().GeneratorIgnored());
        });
    }
}
