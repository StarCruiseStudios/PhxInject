// -----------------------------------------------------------------------------
// <copyright file="InjectorInterfaceTransformer.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Injector;

/// <summary>
///     Transforms injector interface declarations into metadata.
/// </summary>
internal class InjectorInterfaceTransformer(
    ICodeElementValidator elementValidator,
    IAttributeTransformer<InjectorAttributeMetadata> injectorAttributeTransformer,
    ITransformer<IMethodSymbol, InjectorProviderMetadata> injectorProviderTransformer,
    ITransformer<IMethodSymbol, InjectorActivatorMetadata> injectorActivatorTransformer,
    ITransformer<IMethodSymbol, InjectorChildProviderMetadata> injectorChildProviderTransformer,
    IAttributeTransformer<DependencyAttributeMetadata> dependencyAttributeTransformer
) : ITransformer<ITypeSymbol, InjectorInterfaceMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static readonly InjectorInterfaceTransformer Instance = new(
        new InterfaceElementValidator(
            CodeElementAccessibility.PublicOrInternal
        ),
        InjectorAttributeTransformer.Instance,
        InjectorProviderTransformer.Instance,
        InjectorActivatorTransformer.Instance,
        InjectorChildProviderTransformer.Instance,
        DependencyAttributeTransformer.Instance);

    /// <inheritdoc />
    public bool CanTransform(ITypeSymbol typeSymbol) {
        return elementValidator.IsValidSymbol(typeSymbol);
    }

    /// <inheritdoc />
    public IResult<InjectorInterfaceMetadata> Transform(ITypeSymbol typeSymbol) {
        return DiagnosticsRecorder.Capture(diagnostics => {
            var injectorAttributeMetadata = injectorAttributeTransformer
                .Transform(typeSymbol)
                .OrThrow(diagnostics);

            var injectorInterfaceType = typeSymbol.ToTypeModel();
            var providers = typeSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(injectorProviderTransformer.CanTransform)
                .Select(injectorProviderTransformer.Transform)
                .SelectOrThrow(diagnostics)
                .ToEquatableList();
            var activators = typeSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(injectorActivatorTransformer.CanTransform)
                .Select(injectorActivatorTransformer.Transform)
                .SelectOrThrow(diagnostics)
                .ToEquatableList();
            var childProviders = typeSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(injectorChildProviderTransformer.CanTransform)
                .Select(injectorChildProviderTransformer.Transform)
                .SelectOrThrow(diagnostics)
                .ToEquatableList();
            var dependencyAttributeMetadata = dependencyAttributeTransformer
                .TransformOrNull(typeSymbol)?
                .OrThrow(diagnostics);
            
            return new InjectorInterfaceMetadata(
                injectorInterfaceType,
                providers,
                activators,
                childProviders,
                injectorAttributeMetadata,
                dependencyAttributeMetadata,
                typeSymbol.GetLocationOrDefault().GeneratorIgnored()
            );
        });
    }
}
