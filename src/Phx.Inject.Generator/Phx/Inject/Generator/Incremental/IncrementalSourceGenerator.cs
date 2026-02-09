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
using Phx.Inject.Generator.Incremental.Stage2.Mappers;

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
                // Stage 2: Transform metadata into domain model
                var injectorModel = InjectorModelMapper.MapToModel(injector);
                
                var output = new StringBuilder();
                output.AppendLine($"class Generated{injectorModel.InjectorInterfaceType.BaseTypeName} {{");
                foreach (var provider in injectorModel.Providers) {
                    output.AppendLine($"  // Provider: {provider.ProvidedType} {provider.ProviderMethodName}");
                }
                foreach (var activator in injectorModel.Activators) {
                    output.AppendLine($"  // Activator: {activator.ActivatedType} {activator.ActivatorMethodName}");
                }
                foreach (var childProvider in injectorModel.ChildProviders) {
                    output.Append($"  // ChildProvider: {childProvider.ChildInjectorType} {childProvider.ChildProviderMethodName}(");
                    output.Append(string.Join(", ", childProvider.Parameters));
                    output.AppendLine(")");
                }
                output.AppendLine("}");
                
                sourceProductionContext.AddSource($"Generated{injectorModel.InjectorInterfaceType.NamespacedBaseTypeName}.cs",
                    output.ToString());
            });
        
        var injectorDependencyPipelineSegment = injectorDependencyPipeline.Select(generatorInitializationContext.SyntaxProvider);
        generatorInitializationContext.RegisterSourceOutput(injectorDependencyPipelineSegment,
            (sourceProductionContext, injectorDependency) => {
                // Stage 2: Transform metadata into unified specification model
                var specModel = SpecificationModelMapper.MapInjectorDependency(injectorDependency);
                
                var output = new StringBuilder();
                output.AppendLine($"class Generated{specModel.SpecType.BaseTypeName} {{");
                output.AppendLine($"  // InstantiationMode: {specModel.InstantiationMode}");
                foreach (var factory in specModel.Factories) {
                    output.Append($"  // Factory: {factory.ReturnType} {factory.FactoryMemberName}(");
                    output.Append(string.Join(", ", factory.Parameters));
                    output.AppendLine($") [{factory.MemberType}, {factory.FabricationMode}]");
                }
                output.AppendLine("}");
                
                sourceProductionContext.AddSource($"Generated{specModel.SpecType.NamespacedBaseTypeName}.cs",
                    output.ToString());
            });
        
        var specClassPipelineSegment = specClassPipeline.Select(generatorInitializationContext.SyntaxProvider);
        generatorInitializationContext.RegisterSourceOutput(specClassPipelineSegment,
            (sourceProductionContext, specClass) => {
                // Stage 2: Transform metadata into unified specification model
                var specModel = SpecificationModelMapper.MapSpecClass(specClass);
                
                var output = new StringBuilder();
                output.AppendLine($"class Generated{specModel.SpecType.BaseTypeName} {{");
                output.AppendLine($"  // InstantiationMode: {specModel.InstantiationMode}");
                foreach (var factory in specModel.Factories) {
                    output.Append($"  // Factory: {factory.ReturnType} {factory.FactoryMemberName}(");
                    output.Append(string.Join(", ", factory.Parameters));
                    output.AppendLine($") [{factory.MemberType}, {factory.FabricationMode}, Partial={factory.IsPartial}]");
                }
                foreach (var builder in specModel.Builders) {
                    output.Append($"  // Builder: {builder.BuiltType} {builder.BuilderMemberName}(");
                    output.Append(string.Join(", ", builder.Parameters));
                    output.AppendLine($") [{builder.MemberType}]");
                }
                foreach (var link in specModel.Links) {
                    output.AppendLine($"  // Link: [{link.InputLabel ?? link.InputQualifier?.ToString() ?? ""}]{link.Input} -> [{link.OutputLabel ?? link.OutputQualifier?.ToString() ?? ""}]{link.Output}");
                }
                output.AppendLine("}");
                
                sourceProductionContext.AddSource($"Generated{specModel.SpecType.NamespacedBaseTypeName}.cs",
                    output.ToString());
            });
        
        var specInterfacePipelineSegment = specInterfacePipeline.Select(generatorInitializationContext.SyntaxProvider);
        generatorInitializationContext.RegisterSourceOutput(specInterfacePipelineSegment,
            (sourceProductionContext, specInterface) => {
                // Stage 2: Transform metadata into unified specification model
                var specModel = SpecificationModelMapper.MapSpecInterface(specInterface);
                
                var output = new StringBuilder();
                output.AppendLine($"class Generated{specModel.SpecType.BaseTypeName} {{");
                output.AppendLine($"  // InstantiationMode: {specModel.InstantiationMode}");
                foreach (var factory in specModel.Factories) {
                    output.Append($"  // Factory: {factory.ReturnType} {factory.FactoryMemberName}(");
                    output.Append(string.Join(", ", factory.Parameters));
                    output.AppendLine($") [{factory.MemberType}, {factory.FabricationMode}, Partial={factory.IsPartial}]");
                }
                foreach (var builder in specModel.Builders) {
                    output.Append($"  // Builder: {builder.BuiltType} {builder.BuilderMemberName}(");
                    output.Append(string.Join(", ", builder.Parameters));
                    output.AppendLine($") [{builder.MemberType}]");
                }
                foreach (var link in specModel.Links) {
                    output.AppendLine($"  // Link: [{link.InputLabel ?? link.InputQualifier?.ToString() ?? ""}]{link.Input} -> [{link.OutputLabel ?? link.OutputQualifier?.ToString() ?? ""}]{link.Output}");
                }
                output.AppendLine("}");
                
                sourceProductionContext.AddSource($"Generated{specModel.SpecType.NamespacedBaseTypeName}.cs",
                    output.ToString());
            });
        
        var autoFactoryPipelineSegment = autoFactoryPipeline.Select(generatorInitializationContext.SyntaxProvider);
        generatorInitializationContext.RegisterSourceOutput(autoFactoryPipelineSegment,
            (sourceProductionContext, autoFactory) => {
                // Stage 2: Transform metadata into unified specification model
                var specModel = SpecificationModelMapper.MapAutoFactory(autoFactory);
                
                var output = new StringBuilder();
                output.AppendLine($"class Generated{specModel.SpecType.BaseTypeName} {{");
                output.AppendLine($"  // InstantiationMode: {specModel.InstantiationMode}");
                foreach (var factory in specModel.Factories) {
                    output.Append($"  // Factory: {factory.ReturnType} {factory.FactoryMemberName}(");
                    output.Append(string.Join(", ", factory.Parameters));
                    output.AppendLine($") [{factory.MemberType}, {factory.FabricationMode}]");
                    foreach (var prop in factory.RequiredProperties) {
                        output.AppendLine($"  //   RequiredProperty: {prop.PropertyType} {prop.PropertyName}");
                    }
                }
                output.AppendLine("}");
                
                sourceProductionContext.AddSource($"Generated{specModel.SpecType.NamespacedBaseTypeName}.cs",
                    output.ToString());
            });
        
        var autoBuilderPipelineSegment = autoBuilderPipeline.Select(generatorInitializationContext.SyntaxProvider);
        generatorInitializationContext.RegisterSourceOutput(autoBuilderPipelineSegment,
            (sourceProductionContext, autoBuilder) => {
                // Stage 2: Transform metadata into unified specification model
                var specModel = SpecificationModelMapper.MapAutoBuilder(autoBuilder);
                
                var output = new StringBuilder();
                output.AppendLine($"class Generated{specModel.SpecType.BaseTypeName} {{");
                output.AppendLine($"  // InstantiationMode: {specModel.InstantiationMode}");
                foreach (var builder in specModel.Builders) {
                    output.Append($"  // Builder: {builder.BuiltType} {builder.BuilderMemberName}(");
                    output.Append(string.Join(", ", builder.Parameters));
                    output.AppendLine($") [{builder.MemberType}]");
                }
                output.AppendLine("}");
                
                sourceProductionContext.AddSource($"Generated{specModel.SpecType.NamespacedBaseTypeName}.cs",
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
