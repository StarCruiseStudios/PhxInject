// -----------------------------------------------------------------------------
// <copyright file="SpecInterfacePipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;
using Phx.Inject.Generator.Incremental.Util;
using static Phx.Inject.Common.Util.StringBuilderUtil;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Specification;

/// <summary>
///     Pipeline for processing Specification interface declarations into metadata.
/// </summary>
internal sealed class SpecInterfacePipeline(
    ICodeElementValidator elementValidator,
    ITransformer<ITypeSymbol, SpecInterfaceMetadata> specInterfaceTransformer
) : ISyntaxValuesPipeline<SpecInterfaceMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static readonly SpecInterfacePipeline Instance = new(
        SpecInterfaceMetadata.ElementValidator,
        SpecInterfaceTransformer.Instance);

    /// <inheritdoc />
    public IncrementalValuesProvider<IResult<SpecInterfaceMetadata>> Select(
        SyntaxValueProvider syntaxProvider
    ) {
        return syntaxProvider.ForAttributeWithMetadataName(
            SpecificationAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => DiagnosticsRecorder.Capture(diagnostics => {
                return specInterfaceTransformer
                    .Transform((ITypeSymbol)context.TargetSymbol)
                    .OrThrow(diagnostics);
            }));
    }

    /// <summary>
    ///     DEBUG ONLY: Prints spec interface metadata to diagnostic source files.
    /// </summary>
    /// <param name="context">The generator context for registering outputs.</param>
    /// <param name="segment">The spec interface pipeline segment.</param>
    public void Print(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<IResult<SpecInterfaceMetadata>> segment
    ) {
        context.RegisterSourceOutput(segment,
            (sourceProductionContext, specInterface) => {
                var diagnostics = new DiagnosticsRecorder();
                var specInterfaceValue = specInterface.GetValue(diagnostics);
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{specInterfaceValue.SpecInterfaceType.BaseTypeName} {{");
                    foreach (var factoryMethod in specInterfaceValue.FactoryMethods) {
                        b.Append($"  // FactoryMethod: {factoryMethod.FactoryReturnType} {factoryMethod.FactoryMethodName}(");
                        b.Append(string.Join(", ", factoryMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var factoryProperty in specInterfaceValue.FactoryProperties) {
                        b.AppendLine($"  // FactoryProperty: {factoryProperty.FactoryReturnType} {factoryProperty.FactoryPropertyName}");
                    }
                    foreach (var factoryReference in specInterfaceValue.FactoryReferences) {
                        b.Append($"  // FactoryReference: {factoryReference.FactoryReturnType} {factoryReference.FactoryReferenceName}(");
                        b.Append(string.Join(", ", factoryReference.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var builderMethod in specInterfaceValue.BuilderMethods) {
                        b.Append($"  // BuilderMethod: {builderMethod.BuiltType} {builderMethod.BuilderMethodName}(");
                        b.Append(string.Join(", ", builderMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var builderReference in specInterfaceValue.BuilderReferences) {
                        b.Append($"  // BuilderReference: {builderReference.BuiltType} {builderReference.BuilderReferenceName}(");
                        b.Append(string.Join(", ", builderReference.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var link in specInterfaceValue.Links) {
                        b.AppendLine($"  // Link: [{link.InputLabel ?? link.InputQualifier?.ToString() ?? ""}]{link.Input} -> [{link.OutputLabel ?? link.OutputQualifier?.ToString() ?? ""}]{link.Output}");
                    }
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{specInterfaceValue.SpecInterfaceType.NamespacedBaseTypeName}.cs",
                    source);
            });
    }
}
