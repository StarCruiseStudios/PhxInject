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
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Pipeline.Injector;

internal class InjectorActivatorTransformer(ICodeElementValidator elementValidator) {
    public static readonly InjectorActivatorTransformer Instance = new(
        new MethodElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            isStatic: false,
            minParameterCount: 1,
            maxParameterCount: 1,
            returnsVoid: true,
            requiredAttributes: ImmutableList.Create(ChildInjectorAttributeTransformer.Instance)
        ));

    public bool CanTransform(IMethodSymbol methodSymbol) {
        return elementValidator.IsValidSymbol(methodSymbol);
    }

    public InjectorActivatorMetadata Transform(IMethodSymbol methodSymbol) {
        var name = methodSymbol.Name;
        var activatedType = methodSymbol.Parameters[0].Type.ToTypeModel();
        return new InjectorActivatorMetadata(
            name,
            new QualifiedTypeMetadata(activatedType, NoQualifierMetadata.Instance),
            methodSymbol.GetLocationOrDefault().GeneratorIgnored());
    }
}
