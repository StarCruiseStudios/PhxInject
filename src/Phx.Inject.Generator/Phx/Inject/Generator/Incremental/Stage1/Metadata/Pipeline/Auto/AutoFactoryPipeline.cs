// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryPipeline.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Auto;
using Phx.Inject.Generator.Incremental.Util;
using static Phx.Inject.Common.Util.StringBuilderUtil;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Auto;

/// <summary>
///     Pipeline for processing AutoFactory attributes into metadata.
/// </summary>
internal sealed class AutoFactoryPipeline(
    ICodeElementValidator elementValidator,
    ITransformer<ITypeSymbol, AutoFactoryMetadata> autoFactoryTransformer
) : ISyntaxValuesPipeline<AutoFactoryMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static readonly AutoFactoryPipeline Instance = new(
        ClassElementValidator.PublicInstanceClass,
        AutoFactoryTransformer.Instance);

    /// <inheritdoc />
    public IncrementalValuesProvider<IResult<AutoFactoryMetadata>> Select(
        SyntaxValueProvider syntaxProvider
    ) {
        return syntaxProvider.ForAttributeWithMetadataName(
            AutoFactoryAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => DiagnosticsRecorder.Capture(diagnostics => {
                return autoFactoryTransformer
                    .Transform((ITypeSymbol)context.TargetSymbol)
                    .OrThrow(diagnostics);
            }));
    }

    /// <summary>
    ///     DEBUG ONLY: Prints auto factory metadata to diagnostic source files.
    /// </summary>
    /// <param name="context">The generator context for registering outputs.</param>
    /// <param name="segment">The auto factory pipeline segment.</param>
    public void Print(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<IResult<AutoFactoryMetadata>> segment
    ) {
        context.RegisterSourceOutput(segment,
            (sourceProductionContext, autoFactory) => {
                var diagnostics = new DiagnosticsRecorder();
                var autoFactoryValue = autoFactory.GetValue(diagnostics);
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{autoFactoryValue.AutoFactoryType.TypeMetadata.BaseTypeName} {{");
                    b.Append("  // Constructor(");
                    b.Append(string.Join(", ", autoFactoryValue.Parameters));
                    b.AppendLine(")");
                    foreach (var requiredProperty in autoFactoryValue.RequiredProperties) {
                        b.AppendLine($"  // RequiredProperty: {requiredProperty.RequiredPropertyType} {requiredProperty.RequiredPropertyName}");
                    }
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{autoFactoryValue.AutoFactoryType.TypeMetadata.NamespacedBaseTypeName}.cs",
                    source);
            });
    }
}
