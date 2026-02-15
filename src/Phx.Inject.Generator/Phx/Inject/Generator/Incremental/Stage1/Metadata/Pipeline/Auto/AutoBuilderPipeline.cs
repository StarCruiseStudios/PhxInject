// -----------------------------------------------------------------------------
// <copyright file="AutoBuilderPipeline.cs" company="Star Cruise Studios LLC">
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
///     Pipeline for processing AutoBuilder attributes into metadata.
/// </summary>
internal sealed class AutoBuilderPipeline(
    ICodeElementValidator elementValidator,
    ITransformer<IMethodSymbol, AutoBuilderMetadata> autoBuilderTransformer
) : ISyntaxValuesPipeline<AutoBuilderMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static readonly AutoBuilderPipeline Instance = new(
        new MethodElementValidator(
            CodeElementAccessibility.PublicOrInternal,
            isAbstract: false
        ),
        AutoBuilderTransformer.Instance);

    /// <inheritdoc />
    public IncrementalValuesProvider<IResult<AutoBuilderMetadata>> Select(
        SyntaxValueProvider syntaxProvider
    ) {
        return syntaxProvider.ForAttributeWithMetadataName(
            AutoBuilderAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => DiagnosticsRecorder.Capture(diagnostics => {
                return autoBuilderTransformer
                    .Transform((IMethodSymbol)context.TargetSymbol)
                    .OrThrow(diagnostics);
            }));
    }

    /// <summary>
    ///     DEBUG ONLY: Prints auto builder metadata to diagnostic source files.
    /// </summary>
    /// <param name="context">The generator context for registering outputs.</param>
    /// <param name="segment">The auto builder pipeline segment.</param>
    public void Print(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<IResult<AutoBuilderMetadata>> segment
    ) {
        context.RegisterSourceOutput(segment,
            (sourceProductionContext, autoBuilder) => {
                var diagnostics = new DiagnosticsRecorder();
                var autoBuilderValue = autoBuilder.GetValue(diagnostics);
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{autoBuilderValue.BuiltType.TypeMetadata.BaseTypeName}{autoBuilderValue.AutoBuilderMethodName} {{");
                    b.Append($"  // BuilderMethod: {autoBuilderValue.BuiltType} {autoBuilderValue.AutoBuilderMethodName}(");
                    b.Append(string.Join(", ", autoBuilderValue.Parameters));
                    b.AppendLine(")");
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{autoBuilderValue.BuiltType.TypeMetadata.NamespacedBaseTypeName}{autoBuilderValue.AutoBuilderMethodName}.cs",
                    source);
            });
    }
}
