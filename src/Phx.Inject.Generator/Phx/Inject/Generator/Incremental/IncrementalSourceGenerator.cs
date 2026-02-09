// -----------------------------------------------------------------------------
// <copyright file="IncrementalSourceGenerator.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Text;
using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Auto;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Auto;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Settings;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Specification;
using Phx.Inject.Generator.Incremental.Stage2.Model;
using Phx.Inject.Generator.Incremental.Stage2.Pipeline;

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
    ISyntaxValuesPipeline<InjectorDependencyInterfaceMetadata> injectorDependencyPipeline,
    ISyntaxValuesPipeline<SpecClassMetadata> specClassPipeline,
    ISyntaxValuesPipeline<SpecInterfaceMetadata> specInterfacePipeline,
    ISyntaxValuesPipeline<AutoFactoryMetadata> autoFactoryPipeline,
    ISyntaxValuesPipeline<AutoBuilderMetadata> autoBuilderPipeline
) : IIncrementalGenerator {
    public IncrementalSourceGenerator() : this(
        PhxInjectSettingsPipeline.Instance,
        InjectorInterfacePipeline.Instance,
        InjectorDependencyPipeline.Instance,
        SpecClassPipeline.Instance,
        SpecInterfacePipeline.Instance,
        AutoFactoryPipeline.Instance,
        AutoBuilderPipeline.Instance
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
                var output = new StringBuilder();
                output.AppendLine($"class Generated{injector.InjectorInterfaceType.BaseTypeName} {{");
                foreach (var provider in injector.Providers) {
                    output.AppendLine($"  // Provider: {provider.ProvidedType} {provider.ProviderMethodName}");
                }
                foreach (var activator in injector.Activators) {
                    output.AppendLine($"  // Activator: {activator.ActivatedType} {activator.ActivatorMethodName}");
                }
                foreach (var childProvider in injector.ChildProviders) {
                    output.Append($"  // ChildProvider: {childProvider.ChildInjectorType} {childProvider.ChildProviderMethodName}(");
                    output.Append(string.Join(", ", childProvider.Parameters));
                    output.AppendLine(")");
                }
                output.AppendLine("}");
                
                sourceProductionContext.AddSource($"Generated{injector.InjectorInterfaceType.NamespacedBaseTypeName}.cs",
                    output.ToString());
            });
        
        var injectorDependencyPipelineSegment = injectorDependencyPipeline.Select(generatorInitializationContext.SyntaxProvider);
        generatorInitializationContext.RegisterSourceOutput(injectorDependencyPipelineSegment,
            (sourceProductionContext, injectorDependency) => {
                var output = new StringBuilder();
                output.AppendLine($"class Generated{injectorDependency.InjectorDependencyInterfaceType.BaseTypeName} {{");
                foreach (var factoryMethod in injectorDependency.FactoryMethods) {
                    output.Append($"  // FactoryMethod: {factoryMethod.FactoryReturnType} {factoryMethod.FactoryMethodName}(");
                    output.Append(string.Join(", ", factoryMethod.Parameters));
                    output.AppendLine(")");
                }
                foreach (var factoryProperty in injectorDependency.FactoryProperties) {
                    output.AppendLine($"  // FactoryProperty: {factoryProperty.FactoryReturnType} {factoryProperty.FactoryPropertyName}");
                }
                output.AppendLine("}");
                
                sourceProductionContext.AddSource($"Generated{injectorDependency.InjectorDependencyInterfaceType.NamespacedBaseTypeName}.cs",
                    output.ToString());
            });
        
        var specClassPipelineSegment = specClassPipeline.Select(generatorInitializationContext.SyntaxProvider);
        generatorInitializationContext.RegisterSourceOutput(specClassPipelineSegment,
            (sourceProductionContext, specClass) => {
                var output = new StringBuilder();
                output.AppendLine($"class Generated{specClass.SpecType.BaseTypeName} {{");
                
                // Build Stage 2 provider map for this specification
                var providerMap = QualifiedTypeMapBuilder.BuildFromSpecification(specClass);
                
                // Output Stage 2 information
                output.AppendLine("  // === Stage 2: Provider Map ===");
                foreach (var providedType in providerMap.GetProvidedTypes()) {
                    var providers = providerMap.GetProviders(providedType);
                    output.AppendLine($"  // ProvidedType: {providedType}");
                    foreach (var provider in providers) {
                        output.AppendLine($"  //   Provider: {provider.GetType().Name}");
                        foreach (var dep in provider.Dependencies) {
                            output.AppendLine($"  //     Requires: {dep}");
                        }
                    }
                }
                
                output.AppendLine();
                output.AppendLine("  // === Stage 1: Raw Metadata ===");
                foreach (var factoryMethod in specClass.FactoryMethods) {
                    output.Append($"  // FactoryMethod: {factoryMethod.FactoryReturnType} {factoryMethod.FactoryMethodName}(");
                    output.Append(string.Join(", ", factoryMethod.Parameters));
                    output.AppendLine(")");
                }
                foreach (var factoryProperty in specClass.FactoryProperties) {
                    output.AppendLine($"  // FactoryProperty: {factoryProperty.FactoryReturnType} {factoryProperty.FactoryPropertyName}");
                }
                foreach (var factoryReference in specClass.FactoryReferences) {
                    output.Append($"  // FactoryReference: {factoryReference.FactoryReturnType} {factoryReference.FactoryReferenceName}(");
                    output.Append(string.Join(", ", factoryReference.Parameters));
                    output.AppendLine(")");
                }
                foreach (var builderMethod in specClass.BuilderMethods) {
                    output.Append($"  // BuilderMethod: {builderMethod.BuiltType} {builderMethod.BuilderMethodName}(");
                    output.Append(string.Join(", ", builderMethod.Parameters));
                    output.AppendLine(")");
                }
                foreach (var builderReference in specClass.BuilderReferences) {
                    output.Append($"  // BuilderReference: {builderReference.BuiltType} {builderReference.BuilderReferenceName}(");
                    output.Append(string.Join(", ", builderReference.Parameters));
                    output.AppendLine(")");
                }
                foreach (var link in specClass.Links) {
                    output.AppendLine($"  // Link: [{link.InputLabel ?? link.InputQualifier?.ToString() ?? ""}]{link.Input} -> [{link.OutputLabel ?? link.OutputQualifier?.ToString() ?? ""}]{link.Output}");
                }
                output.AppendLine("}");
                
                sourceProductionContext.AddSource($"Generated{specClass.SpecType.NamespacedBaseTypeName}.cs",
                    output.ToString());
            });
        
        var specInterfacePipelineSegment = specInterfacePipeline.Select(generatorInitializationContext.SyntaxProvider);
        generatorInitializationContext.RegisterSourceOutput(specInterfacePipelineSegment,
            (sourceProductionContext, specInterface) => {
                var output = new StringBuilder();
                output.AppendLine($"class Generated{specInterface.SpecInterfaceType.BaseTypeName} {{");
                foreach (var factoryMethod in specInterface.FactoryMethods) {
                    output.Append($"  // FactoryMethod: {factoryMethod.FactoryReturnType} {factoryMethod.FactoryMethodName}(");
                    output.Append(string.Join(", ", factoryMethod.Parameters));
                    output.AppendLine(")");
                }
                foreach (var factoryProperty in specInterface.FactoryProperties) {
                    output.AppendLine($"  // FactoryProperty: {factoryProperty.FactoryReturnType} {factoryProperty.FactoryPropertyName}");
                }
                foreach (var factoryReference in specInterface.FactoryReferences) {
                    output.Append($"  // FactoryReference: {factoryReference.FactoryReturnType} {factoryReference.FactoryReferenceName}(");
                    output.Append(string.Join(", ", factoryReference.Parameters));
                    output.AppendLine(")");
                }
                foreach (var builderMethod in specInterface.BuilderMethods) {
                    output.Append($"  // BuilderMethod: {builderMethod.BuiltType} {builderMethod.BuilderMethodName}(");
                    output.Append(string.Join(", ", builderMethod.Parameters));
                    output.AppendLine(")");
                }
                foreach (var builderReference in specInterface.BuilderReferences) {
                    output.Append($"  // BuilderReference: {builderReference.BuiltType} {builderReference.BuilderReferenceName}(");
                    output.Append(string.Join(", ", builderReference.Parameters));
                    output.AppendLine(")");
                }
                foreach (var link in specInterface.Links) {
                    output.AppendLine($"  // Link: [{link.InputLabel ?? link.InputQualifier?.ToString() ?? ""}]{link.Input} -> [{link.OutputLabel ?? link.OutputQualifier?.ToString() ?? ""}]{link.Output}");
                }
                output.AppendLine("}");
                
                sourceProductionContext.AddSource($"Generated{specInterface.SpecInterfaceType.NamespacedBaseTypeName}.cs",
                    output.ToString());
            });
        
        var autoFactoryPipelineSegment = autoFactoryPipeline.Select(generatorInitializationContext.SyntaxProvider);
        generatorInitializationContext.RegisterSourceOutput(autoFactoryPipelineSegment,
            (sourceProductionContext, autoFactory) => {
                var output = new StringBuilder();
                output.AppendLine($"class Generated{autoFactory.AutoFactoryType.TypeMetadata.BaseTypeName} {{");
                output.Append("  // Constructor(");
                output.Append(string.Join(", ", autoFactory.Parameters));
                output.AppendLine(")");
                foreach (var requiredProperty in autoFactory.RequiredProperties) {
                    output.AppendLine($"  // RequiredProperty: {requiredProperty.RequiredPropertyType} {requiredProperty.RequiredPropertyName}");
                }
                output.AppendLine("}");
                
                sourceProductionContext.AddSource($"Generated{autoFactory.AutoFactoryType.TypeMetadata.NamespacedBaseTypeName}.cs",
                    output.ToString());
            });
        
        var autoBuilderPipelineSegment = autoBuilderPipeline.Select(generatorInitializationContext.SyntaxProvider);
        generatorInitializationContext.RegisterSourceOutput(autoBuilderPipelineSegment,
            (sourceProductionContext, autoBuilder) => {
                var output = new StringBuilder();
                output.AppendLine($"class Generated{autoBuilder.BuiltType.TypeMetadata.BaseTypeName}{autoBuilder.AutoBuilderMethodName} {{");
                output.Append($"  // BuilderMethod: {autoBuilder.BuiltType} {autoBuilder.AutoBuilderMethodName}(");
                output.Append(string.Join(", ", autoBuilder.Parameters));
                output.AppendLine(")");
                output.AppendLine("}");
                
                sourceProductionContext.AddSource($"Generated{autoBuilder.BuiltType.TypeMetadata.NamespacedBaseTypeName}{autoBuilder.AutoBuilderMethodName}.cs",
                    output.ToString());
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
