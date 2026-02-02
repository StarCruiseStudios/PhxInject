// -----------------------------------------------------------------------------
// <copyright file="IncrementalSourceGenerator.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Generator.Incremental.Metadata;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Model;

namespace Phx.Inject.Generator.Incremental;

internal static class PhxInject {
    public const string NamespaceName = "Phx.Inject";
}

/// <summary>
///     <seealso
///         href="https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md"/>
/// </summary>
[Generator(LanguageNames.CSharp)]
internal class IncrementalSourceGenerator(
    IAttributeValuesProvider<PhxInjectAttributeMetadata> phxInjectAttributeValuesProvider,
    PhxInjectSettings.IValuesProvider phxInjectSettingsValuesProvider,
    IAttributeValuesProvider<InjectorAttributeMetadata> injectorAttributeValuesProvider,
    InjectorInterfaceMetadata.IValuesProvider injectorInterfaceValuesProvider
) : IIncrementalGenerator {
    public IncrementalSourceGenerator() : this(
        PhxInjectAttributeMetadata.ValuesProvider.Instance,
        PhxInjectSettings.ValuesProvider.Instance,
        InjectorAttributeMetadata.ValuesProvider.Instance,
        InjectorInterfaceMetadata.ValuesProvider.Instance
    ) { }

    public void Initialize(IncrementalGeneratorInitializationContext generatorInitializationContext) {
        var phxInjectSettingsPipeline = generatorInitializationContext.SyntaxProvider
            .ForAttribute(phxInjectAttributeValuesProvider)
            .Select(phxInjectSettingsValuesProvider.Transform)
            .Collect()
            .Select((settings, cancellationToken) => settings.Length switch {
                0 => new PhxInjectSettings(null),
                1 => settings[0],
                _ => throw new InvalidOperationException("Only one PhxInject attribute is allowed per assembly.")
            });

        var injectorPipeline = generatorInitializationContext.SyntaxProvider
            .ForAttribute(injectorAttributeValuesProvider)
            .Select(injectorInterfaceValuesProvider.Transform);

        generatorInitializationContext.RegisterSourceOutput(injectorPipeline.Combine(phxInjectSettingsPipeline),
            (sourceProductionContext, pair) => {
                var injector = pair.Left;
                var settings = pair.Right;
                
                sourceProductionContext.AddSource($"{injector.InjectorInterfaceType.NamespacedBaseTypeName}.settings.cs",
                    $"/// <remarks>\n" +
                    $"///     Phx.Inject.Generator: Using settings: {settings}\n" +
                    $"/// </remarks>\n" +
                    $"class Generated{injector.InjectorInterfaceType.BaseTypeName} {{ }}");
                sourceProductionContext.ReportDiagnostic(Diagnostics.DebugMessage.CreateDiagnostic(
                    $"Phx.Inject.Generator: Using settings: {settings}",
                    settings.Location));
            });
    }
}
