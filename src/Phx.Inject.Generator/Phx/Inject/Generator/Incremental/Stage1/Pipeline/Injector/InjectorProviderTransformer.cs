// -----------------------------------------------------------------------------
// <copyright file="InjectorProviderTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Pipeline.Injector;

internal class InjectorProviderTransformer {
    public static readonly InjectorProviderTransformer Instance = new();

    public bool CanTransform(IMethodSymbol methodSymbol) {
        return methodSymbol is {
            ReturnsVoid: false,
            IsStatic: false,
            Parameters.Length: 0,
            DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
        };
    }

    public InjectorProviderMetadata Transform(IMethodSymbol methodSymbol) {
        var name = methodSymbol.Name;
        var providedType = methodSymbol.ReturnType.ToTypeModel();
        return new InjectorProviderMetadata(
            name,
            new QualifiedTypeMetadata(providedType, NoQualifierMetadata.Instance),
            methodSymbol.GetLocationOrDefault().GeneratorIgnored());
    }
}
