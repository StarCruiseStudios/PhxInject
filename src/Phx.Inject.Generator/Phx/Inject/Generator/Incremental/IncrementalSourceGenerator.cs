// -----------------------------------------------------------------------------
// <copyright file="IncrementalSourceGenerator.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Generator.Incremental.Metadata;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Metadata.Injector;
using Phx.Inject.Generator.Incremental.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Syntax;

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
    IAttributeSyntaxValuesProvider<PhxInjectAttributeMetadata> phxInjectAttributeSyntaxValuesProvider,
    PhxInjectSettingsMetadata.IValuesProvider phxInjectSettingsValuesProvider,
    IAttributeSyntaxValuesProvider<InjectorInterfaceMetadata> injectorInterfaceSyntaxValuesProvider,
    IAttributeSyntaxValuesProvider<InjectorDependencyInterfaceMetadata> injectorDependencyInterfaceSyntaxValuesProvider,
    IAttributeSyntaxValuesProvider<SpecClassMetadata> specClassSyntaxValuesProvider,
    IAttributeSyntaxValuesProvider<SpecInterfaceMetadata> specInterfaceSyntaxValuesProvider
) : IIncrementalGenerator {
    public IncrementalSourceGenerator() : this(
        PhxInjectAttributeSyntaxValuesProvider.Instance,
        PhxInjectSettingsMetadata.ValuesProvider.Instance,
        InjectorInterfaceSyntaxValuesProvider.Instance,
        InjectorDependencyInterfaceSyntaxValuesProvider.Instance,
        SpecClassSyntaxValuesProvider.Instance,
        SpecInterfaceSyntaxValuesProvider.Instance
    ) { }

    public void Initialize(IncrementalGeneratorInitializationContext generatorInitializationContext) {
        var phxInjectSettingsPipeline = generatorInitializationContext.SyntaxProvider
            .ForAttribute(phxInjectAttributeSyntaxValuesProvider)
            .Select(phxInjectSettingsValuesProvider.Transform)
            .Collect()
            .Select((settings, cancellationToken) => settings.Length switch {
                0 => new PhxInjectSettingsMetadata(null),
                1 => settings[0],
                _ => throw new InvalidOperationException("Only one PhxInject attribute is allowed per assembly.")
            });
        generatorInitializationContext.RegisterSourceOutput(phxInjectSettingsPipeline,
            (sourceProductionContext, settings) => {
                sourceProductionContext.AddSource($"GeneratorSettings.cs",
                    $"/// <remarks>\n" +
                    $"///     Phx.Inject.Generator: Using settings: {settings}\n" +
                    $"/// </remarks>\n" +
                    $"class GeneratorSettings{{ }}");
            });

        var injectorPipeline = generatorInitializationContext.SyntaxProvider
            .ForAttribute(injectorInterfaceSyntaxValuesProvider);
        generatorInitializationContext.RegisterSourceOutput(injectorPipeline,
            (sourceProductionContext, injector) => {
                sourceProductionContext.AddSource($"Generated{injector.InjectorInterfaceType.NamespacedBaseTypeName}.cs",
                    $"class Generated{injector.InjectorInterfaceType.BaseTypeName} {{ }}");
            });
        
        var injectorDependencyPipeline = generatorInitializationContext.SyntaxProvider
            .ForAttribute(injectorDependencyInterfaceSyntaxValuesProvider);
        generatorInitializationContext.RegisterSourceOutput(injectorDependencyPipeline,
            (sourceProductionContext, injectorDependency) => {
                sourceProductionContext.AddSource($"Generated{injectorDependency.InjectorDependencyInterfaceType.NamespacedBaseTypeName}.cs",
                    $"class Generated{injectorDependency.InjectorDependencyInterfaceType.BaseTypeName} {{ }}");
            });
        
        var specClassPipeline = generatorInitializationContext.SyntaxProvider
            .ForAttribute(specClassSyntaxValuesProvider);
        generatorInitializationContext.RegisterSourceOutput(specClassPipeline,
            (sourceProductionContext, specClass) => {
                sourceProductionContext.AddSource($"Generated{specClass.SpecType.NamespacedBaseTypeName}.cs",
                    $"class Generated{specClass.SpecType.BaseTypeName} {{ }}");
            });
        
        var specInterfacePipeline = generatorInitializationContext.SyntaxProvider
            .ForAttribute(specInterfaceSyntaxValuesProvider);
        generatorInitializationContext.RegisterSourceOutput(specInterfacePipeline,
            (sourceProductionContext, specInterface) => {
                sourceProductionContext.AddSource($"Generated{specInterface.SpecInterfaceType.NamespacedBaseTypeName}.cs",
                    $"class Generated{specInterface.SpecInterfaceType.BaseTypeName} {{ }}");
            });

        // generatorInitializationContext.RegisterSourceOutput(injectorPipeline.Combine(phxInjectSettingsPipeline),
        //     (sourceProductionContext, pair) => {
        //         var injector = pair.Left;
        //         var settings = pair.Right;
        //         
        //         sourceProductionContext.AddSource($"{injector.InjectorInterfaceType.NamespacedBaseTypeName}.settings.cs",
        //             $"/// <remarks>\n" +
        //             $"///     Phx.Inject.Generator: Using settings: {settings}\n" +
        //             $"/// </remarks>\n" +
        //             $"class Generated{injector.InjectorInterfaceType.BaseTypeName} {{ }}");
        //         sourceProductionContext.ReportDiagnostic(Diagnostics.DebugMessage.CreateDiagnostic(
        //             $"Phx.Inject.Generator: Using settings: {settings}",
        //             settings.Location.Value));
        //     });
    }
}
