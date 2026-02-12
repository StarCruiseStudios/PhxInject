// -----------------------------------------------------------------------------
// <copyright file="InjectorPipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
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

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Injector;

internal class InjectorInterfacePipeline(
    ICodeElementValidator elementValidator,
    IAttributeTransformer<InjectorAttributeMetadata> injectorAttributeTransformer,
    InjectorProviderTransformer injectorProviderTransformer,
    InjectorActivatorTransformer injectorActivatorTransformer,
    InjectorChildProviderTransformer injectorChildProviderTransformer,
    DependencyAttributeTransformer dependencyAttributeTransformer
) : ISyntaxValuesPipeline<InjectorInterfaceMetadata> {
    public static readonly InjectorInterfacePipeline Instance = new(
        new InterfaceElementValidator(
            CodeElementAccessibility.PublicOrInternal
        ),
        InjectorAttributeTransformer.Instance,
        InjectorProviderTransformer.Instance,
        InjectorActivatorTransformer.Instance,
        InjectorChildProviderTransformer.Instance,
        DependencyAttributeTransformer.Instance);

    public IncrementalValuesProvider<IResult<InjectorInterfaceMetadata>> Select(
        SyntaxValueProvider syntaxProvider
    ) {
        return syntaxProvider.ForAttributeWithMetadataName(
            InjectorAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => DiagnosticsRecorder.Capture(diagnostics => {
                var targetSymbol = (ITypeSymbol)context.TargetSymbol;
                var injectorAttributeMetadata = injectorAttributeTransformer
                    .Transform(targetSymbol)
                    .GetOrThrow(diagnostics);

                var injectorInterfaceType = targetSymbol.ToTypeModel();
                var providers = targetSymbol.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(injectorProviderTransformer.CanTransform)
                    .Select(injectorProviderTransformer.Transform)
                    .SelectOrThrow(diagnostics)
                    .ToImmutableList();
                var activators = targetSymbol.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(injectorActivatorTransformer.CanTransform)
                    .Select(injectorActivatorTransformer.Transform)
                    .SelectOrThrow(diagnostics)
                    .ToImmutableList();
                var childProviders = targetSymbol.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(injectorChildProviderTransformer.CanTransform)
                    .Select(injectorChildProviderTransformer.Transform)
                    .SelectOrThrow(diagnostics)
                    .ToImmutableList();
                DependencyAttributeMetadata? dependencyAttributeMetadata = null;
                if (dependencyAttributeTransformer.HasAttribute(targetSymbol)) {
                    dependencyAttributeMetadata = dependencyAttributeTransformer.Transform(targetSymbol).GetOrThrow(diagnostics);
                }
                
                return new InjectorInterfaceMetadata(
                    injectorInterfaceType,
                    providers,
                    activators,
                    childProviders,
                    injectorAttributeMetadata,
                    dependencyAttributeMetadata,
                    targetSymbol.GetLocationOrDefault().GeneratorIgnored()
                );
            }));
    }
}