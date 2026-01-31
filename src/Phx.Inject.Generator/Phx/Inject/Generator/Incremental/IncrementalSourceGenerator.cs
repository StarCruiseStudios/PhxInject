// -----------------------------------------------------------------------------
// <copyright file="IncrementalSourceGenerator.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
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
    PhxInjectAttributeMetadata.IValuesProvider phxInjectAttributeValuesProvider,
    PhxInjectSettings.IValuesProvider phxInjectSettingsValuesProvider
) : IIncrementalGenerator {
    public IncrementalSourceGenerator() : this(
        PhxInjectAttributeMetadata.ValuesProvider.Instance,
        PhxInjectSettings.ValuesProvider.Instance
    ) { }

    public void Initialize(IncrementalGeneratorInitializationContext generatorInitializationContext) {
        var phxInjectSettingsPipeline = generatorInitializationContext.SyntaxProvider
            .ForAttributeWithMetadataName(
                PhxInjectAttributeMetadata.AttributeClassName,
                phxInjectAttributeValuesProvider.CanProvide,
                phxInjectAttributeValuesProvider.Transform)
            .Select(phxInjectSettingsValuesProvider.Transform)
            .Collect()
            .Select((settings, cancellationToken) => settings.Length switch {
                0 => new PhxInjectSettings(null),
                1 => settings[0],
                _ => throw new InvalidOperationException("Only one PhxInject attribute is allowed per assembly.")
            });

        generatorInitializationContext.RegisterSourceOutput(phxInjectSettingsPipeline,
            (sourceProductionContext, phxInjectSettings) => {
                sourceProductionContext.AddSource("settings.cs",
                    $"class TestSettings {{\n" +
                    $"  private string x = \"Phx.Inject.Generator: Using settings: {phxInjectSettings}\";\n" +
                    $"\n}}");
                sourceProductionContext.ReportDiagnostic(Diagnostics.DebugMessage.CreateDiagnostic(
                    $"Phx.Inject.Generator: Using settings: {phxInjectSettings}",
                    phxInjectSettings.Location));
            });
    }
}
