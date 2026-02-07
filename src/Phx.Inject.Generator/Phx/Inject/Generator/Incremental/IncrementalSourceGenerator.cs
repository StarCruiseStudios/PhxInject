// -----------------------------------------------------------------------------
// <copyright file="IncrementalSourceGenerator.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Generator.Incremental.Stage1;
using Phx.Inject.Generator.Incremental.Stage1.Metadata;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Auto;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Settings;

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
    ISyntaxValuePipeline<PhxInjectSettingsMetadata> phxInjectSettingsPipeline,
    ISyntaxValuesPipeline<InjectorInterfaceMetadata> injectorPipeline,
    IAttributeSyntaxValuesProvider<InjectorDependencyInterfaceMetadata> injectorDependencyInterfaceSyntaxValuesProvider,
    IAttributeSyntaxValuesProvider<SpecClassMetadata> specClassSyntaxValuesProvider,
    IAttributeSyntaxValuesProvider<SpecInterfaceMetadata> specInterfaceSyntaxValuesProvider,
    IAttributeSyntaxValuesProvider<AutoFactoryMetadata> autoFactorySyntaxValuesProvider,
    IAttributeSyntaxValuesProvider<AutoBuilderMetadata> autoBuilderSyntaxValuesProvider
) : IIncrementalGenerator {
    public IncrementalSourceGenerator() : this(
        PhxInjectSettingsPipeline.Instance,
        InjectorInterfacePipeline.Instance,
        InjectorDependencyInterfaceSyntaxValuesProvider.Instance,
        SpecClassSyntaxValuesProvider.Instance,
        SpecInterfaceSyntaxValuesProvider.Instance,
        AutoFactorySyntaxValuesProvider.Instance,
        AutoBuilderSyntaxValuesProvider.Instance
    ) { }

    public void Initialize(IncrementalGeneratorInitializationContext generatorInitializationContext) {

        var phxInjectSettingsPipelineSegment = phxInjectSettingsPipeline.Select(generatorInitializationContext.SyntaxProvider);
        generatorInitializationContext.RegisterSourceOutput(phxInjectSettingsPipelineSegment,
            (sourceProductionContext, settings) => {
                sourceProductionContext.AddSource($"GeneratorSettings.cs",
                    $"/// <remarks>\n" +
                    $"///     Phx.Inject.Generator: Using settings: {settings}\n" +
                    $"/// </remarks>\n" +
                    $"class GeneratorSettings{{ }}");
            });

        var injectorInterfacePipelineSegment = injectorPipeline.Select(generatorInitializationContext.SyntaxProvider);
        generatorInitializationContext.RegisterSourceOutput(injectorInterfacePipelineSegment,
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
        
        var autoFactoryPipeline = generatorInitializationContext.SyntaxProvider
            .ForAttribute(autoFactorySyntaxValuesProvider);
        generatorInitializationContext.RegisterSourceOutput(autoFactoryPipeline,
            (sourceProductionContext, autoFactory) => {
                sourceProductionContext.AddSource($"Generated{autoFactory.AutoFactoryType.TypeMetadata.NamespacedBaseTypeName}.cs",
                    $"class Generated{autoFactory.AutoFactoryType.TypeMetadata.BaseTypeName} {{ }}");
            });
        
        var autoBuilderPipeline = generatorInitializationContext.SyntaxProvider
            .ForAttribute(autoBuilderSyntaxValuesProvider);
        generatorInitializationContext.RegisterSourceOutput(autoBuilderPipeline,
            (sourceProductionContext, autoBuilder) => {
                sourceProductionContext.AddSource($"Generated{autoBuilder.BuiltType.TypeMetadata.NamespacedBaseTypeName}{autoBuilder.AutoBuilderMethodName}.cs",
                    $"class Generated{autoBuilder.BuiltType.TypeMetadata.BaseTypeName}{autoBuilder.AutoBuilderMethodName} {{ }}");
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
