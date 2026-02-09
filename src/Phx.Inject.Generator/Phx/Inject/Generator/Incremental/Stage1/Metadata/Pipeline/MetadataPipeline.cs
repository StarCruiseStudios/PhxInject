// -----------------------------------------------------------------------------
// <copyright file="MetadataPipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Auto;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Settings;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Auto;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Settings;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Specification;
using static Phx.Inject.Common.Util.StringBuilderUtil;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline;

internal record MetadataPipelineOutput(
    IncrementalValueProvider<PhxInjectSettingsMetadata> PhxInjectSettingsPipelineSegment,
    IncrementalValuesProvider<InjectorInterfaceMetadata> InjectorInterfacePipelineSegment,
    IncrementalValuesProvider<InjectorDependencyInterfaceMetadata> InjectorDependencyPipelineSegment,
    IncrementalValuesProvider<SpecClassMetadata> SpecClassPipelineSegment,
    IncrementalValuesProvider<SpecInterfaceMetadata> SpecInterfacePipelineSegment,
    IncrementalValuesProvider<AutoFactoryMetadata> AutoFactoryPipelineSegment,
    IncrementalValuesProvider<AutoBuilderMetadata> AutoBuilderPipelineSegment
);

internal class MetadataPipeline(
    ISyntaxValuePipeline<PhxInjectSettingsMetadata> phxInjectSettingsPipeline,
    ISyntaxValuesPipeline<InjectorInterfaceMetadata> injectorPipeline,
    ISyntaxValuesPipeline<InjectorDependencyInterfaceMetadata> injectorDependencyPipeline,
    ISyntaxValuesPipeline<SpecClassMetadata> specClassPipeline,
    ISyntaxValuesPipeline<SpecInterfaceMetadata> specInterfacePipeline,
    ISyntaxValuesPipeline<AutoFactoryMetadata> autoFactoryPipeline,
    ISyntaxValuesPipeline<AutoBuilderMetadata> autoBuilderPipeline
) {
    public static readonly MetadataPipeline Instance = new(
        PhxInjectSettingsPipeline.Instance,
        InjectorInterfacePipeline.Instance,
        InjectorDependencyPipeline.Instance,
        SpecClassPipeline.Instance,
        SpecInterfacePipeline.Instance,
        AutoFactoryPipeline.Instance,
        AutoBuilderPipeline.Instance
    );
    
    public MetadataPipelineOutput Process(IncrementalGeneratorInitializationContext generatorInitializationContext) {
        var phxInjectSettingsPipelineSegment = phxInjectSettingsPipeline.Select(generatorInitializationContext.SyntaxProvider);
        var injectorInterfacePipelineSegment = injectorPipeline.Select(generatorInitializationContext.SyntaxProvider);
        var injectorDependencyPipelineSegment = injectorDependencyPipeline.Select(generatorInitializationContext.SyntaxProvider);
        var specClassPipelineSegment = specClassPipeline.Select(generatorInitializationContext.SyntaxProvider);
        var specInterfacePipelineSegment = specInterfacePipeline.Select(generatorInitializationContext.SyntaxProvider);
        var autoFactoryPipelineSegment = autoFactoryPipeline.Select(generatorInitializationContext.SyntaxProvider);
        var autoBuilderPipelineSegment = autoBuilderPipeline.Select(generatorInitializationContext.SyntaxProvider);
        
        return new MetadataPipelineOutput(
            phxInjectSettingsPipelineSegment,
            injectorInterfacePipelineSegment,
            injectorDependencyPipelineSegment,
            specClassPipelineSegment,
            specInterfacePipelineSegment,
            autoFactoryPipelineSegment,
            autoBuilderPipelineSegment
        );
    }
}

internal static class IncrementalGeneratorInitializationContextExtensions {
    public static MetadataPipelineOutput Process(this IncrementalGeneratorInitializationContext context, MetadataPipeline pipeline) {
        return pipeline.Process(context);
    }
    
    public static MetadataPipelineOutput Print(this MetadataPipelineOutput output, IncrementalGeneratorInitializationContext context) {
        context.RegisterSourceOutput(output.PhxInjectSettingsPipelineSegment,
            (sourceProductionContext, settings) => {
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"/// <remarks>");
                    b.AppendLine($"///     Phx.Inject.Generator: Using settings: {settings}");
                    b.AppendLine($"/// </remarks>");
                    b.AppendLine($"class GeneratorSettings{{ }}");
                });
                sourceProductionContext.AddSource($"Metadata\\GeneratorSettings.cs", source);
            });
        context.RegisterSourceOutput(output.InjectorInterfacePipelineSegment,
            (sourceProductionContext, injector) => {
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{injector.InjectorInterfaceType.BaseTypeName} {{");
                    foreach (var provider in injector.Providers) {
                        b.AppendLine(
                            $"  // Provider: {provider.ProvidedType} {provider.ProviderMethodName}");
                    }

                    foreach (var activator in injector.Activators) {
                        b.AppendLine(
                            $"  // Activator: {activator.ActivatedType} {activator.ActivatorMethodName}");
                    }

                    foreach (var childProvider in injector.ChildProviders) {
                        b.Append(
                            $"  // ChildProvider: {childProvider.ChildInjectorType} {childProvider.ChildProviderMethodName}(");
                        b.Append(string.Join(", ", childProvider.Parameters));
                        b.AppendLine(")");
                    }

                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource(
                    $"Metadata\\Generated{injector.InjectorInterfaceType.NamespacedBaseTypeName}.cs",
                    source);
            });
        context.RegisterSourceOutput(output.InjectorDependencyPipelineSegment,
            (sourceProductionContext, injectorDependency) => {
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{injectorDependency.InjectorDependencyInterfaceType.BaseTypeName} {{");
                    foreach (var factoryMethod in injectorDependency.FactoryMethods) {
                        b.Append($"  // FactoryMethod: {factoryMethod.FactoryReturnType} {factoryMethod.FactoryMethodName}(");
                        b.Append(string.Join(", ", factoryMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var factoryProperty in injectorDependency.FactoryProperties) {
                        b.AppendLine($"  // FactoryProperty: {factoryProperty.FactoryReturnType} {factoryProperty.FactoryPropertyName}");
                    }
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{injectorDependency.InjectorDependencyInterfaceType.NamespacedBaseTypeName}.cs",
                    source);
            });
        context.RegisterSourceOutput(output.SpecClassPipelineSegment,
            (sourceProductionContext, specClass) => {
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{specClass.SpecType.BaseTypeName} {{");
                    foreach (var factoryMethod in specClass.FactoryMethods) {
                        b.Append($"  // FactoryMethod: {factoryMethod.FactoryReturnType} {factoryMethod.FactoryMethodName}(");
                        b.Append(string.Join(", ", factoryMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var factoryProperty in specClass.FactoryProperties) {
                        b.AppendLine($"  // FactoryProperty: {factoryProperty.FactoryReturnType} {factoryProperty.FactoryPropertyName}");
                    }
                    foreach (var factoryReference in specClass.FactoryReferences) {
                        b.Append($"  // FactoryReference: {factoryReference.FactoryReturnType} {factoryReference.FactoryReferenceName}(");
                        b.Append(string.Join(", ", factoryReference.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var builderMethod in specClass.BuilderMethods) {
                        b.Append($"  // BuilderMethod: {builderMethod.BuiltType} {builderMethod.BuilderMethodName}(");
                        b.Append(string.Join(", ", builderMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var builderReference in specClass.BuilderReferences) {
                        b.Append($"  // BuilderReference: {builderReference.BuiltType} {builderReference.BuilderReferenceName}(");
                        b.Append(string.Join(", ", builderReference.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var link in specClass.Links) {
                        b.AppendLine($"  // Link: [{link.InputLabel ?? link.InputQualifier?.ToString() ?? ""}]{link.Input} -> [{link.OutputLabel ?? link.OutputQualifier?.ToString() ?? ""}]{link.Output}");
                    }
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{specClass.SpecType.NamespacedBaseTypeName}.cs",
                    source);
            });
        context.RegisterSourceOutput(output.SpecInterfacePipelineSegment,
            (sourceProductionContext, specInterface) => {
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{specInterface.SpecInterfaceType.BaseTypeName} {{");
                    foreach (var factoryMethod in specInterface.FactoryMethods) {
                        b.Append($"  // FactoryMethod: {factoryMethod.FactoryReturnType} {factoryMethod.FactoryMethodName}(");
                        b.Append(string.Join(", ", factoryMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var factoryProperty in specInterface.FactoryProperties) {
                        b.AppendLine($"  // FactoryProperty: {factoryProperty.FactoryReturnType} {factoryProperty.FactoryPropertyName}");
                    }
                    foreach (var factoryReference in specInterface.FactoryReferences) {
                        b.Append($"  // FactoryReference: {factoryReference.FactoryReturnType} {factoryReference.FactoryReferenceName}(");
                        b.Append(string.Join(", ", factoryReference.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var builderMethod in specInterface.BuilderMethods) {
                        b.Append($"  // BuilderMethod: {builderMethod.BuiltType} {builderMethod.BuilderMethodName}(");
                        b.Append(string.Join(", ", builderMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var builderReference in specInterface.BuilderReferences) {
                        b.Append($"  // BuilderReference: {builderReference.BuiltType} {builderReference.BuilderReferenceName}(");
                        b.Append(string.Join(", ", builderReference.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var link in specInterface.Links) {
                        b.AppendLine($"  // Link: [{link.InputLabel ?? link.InputQualifier?.ToString() ?? ""}]{link.Input} -> [{link.OutputLabel ?? link.OutputQualifier?.ToString() ?? ""}]{link.Output}");
                    }
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{specInterface.SpecInterfaceType.NamespacedBaseTypeName}.cs",
                    source);
            });
        context.RegisterSourceOutput(output.AutoFactoryPipelineSegment,
            (sourceProductionContext, autoFactory) => {
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{autoFactory.AutoFactoryType.TypeMetadata.BaseTypeName} {{");
                    b.Append("  // Constructor(");
                    b.Append(string.Join(", ", autoFactory.Parameters));
                    b.AppendLine(")");
                    foreach (var requiredProperty in autoFactory.RequiredProperties) {
                        b.AppendLine($"  // RequiredProperty: {requiredProperty.RequiredPropertyType} {requiredProperty.RequiredPropertyName}");
                    }
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{autoFactory.AutoFactoryType.TypeMetadata.NamespacedBaseTypeName}.cs",
                    source);
            });
        context.RegisterSourceOutput(output.AutoBuilderPipelineSegment,
            (sourceProductionContext, autoBuilder) => {
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{autoBuilder.BuiltType.TypeMetadata.BaseTypeName}{autoBuilder.AutoBuilderMethodName} {{");
                    b.Append($"  // BuilderMethod: {autoBuilder.BuiltType} {autoBuilder.AutoBuilderMethodName}(");
                    b.Append(string.Join(", ", autoBuilder.Parameters));
                    b.AppendLine(")");
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{autoBuilder.BuiltType.TypeMetadata.NamespacedBaseTypeName}{autoBuilder.AutoBuilderMethodName}.cs",
                    source);
            });

        return output;
    }
}