// -----------------------------------------------------------------------------
// <copyright file="InjectorInterfacePipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;
using static Phx.Inject.Common.Util.StringBuilderUtil;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Injector;

/// <summary>
///     Pipeline for processing Injector interface declarations into metadata.
/// </summary>
internal sealed class InjectorInterfacePipeline(
    ICodeElementValidator elementValidator,
    ITransformer<ITypeSymbol, InjectorInterfaceMetadata> injectorInterfaceTransformer
) : ISyntaxValuesPipeline<InjectorInterfaceMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static readonly InjectorInterfacePipeline Instance = new(
        InjectorInterfaceMetadata.ElementValidator,
        InjectorInterfaceTransformer.Instance);

    /// <inheritdoc />
    public IncrementalValuesProvider<IResult<InjectorInterfaceMetadata>> Select(
        SyntaxValueProvider syntaxProvider
    ) {
        return syntaxProvider.ForAttributeWithMetadataName(
            InjectorAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => DiagnosticsRecorder.Capture(diagnostics => {
                return injectorInterfaceTransformer
                    .Transform((ITypeSymbol)context.TargetSymbol)
                    .OrThrow(diagnostics);
            }));
    }

    /// <summary>
    ///     DEBUG ONLY: Prints injector interface metadata to diagnostic source files.
    /// </summary>
    /// <param name="context">The generator context for registering outputs.</param>
    /// <param name="segment">The injector interface pipeline segment.</param>
    public void Print(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<IResult<InjectorInterfaceMetadata>> segment
    ) {
        context.RegisterSourceOutput(segment,
            (sourceProductionContext, injector) => {
                var diagnostics = new DiagnosticsRecorder();
                var injectorValue = injector.GetValue(diagnostics);
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{injectorValue.InjectorInterfaceType.BaseTypeName} {{");
                    foreach (var provider in injectorValue.Providers) {
                        b.AppendLine(
                            $"  // Provider: {provider.ProvidedType} {provider.ProviderMethodName}");
                    }

                    foreach (var activator in injectorValue.Activators) {
                        b.AppendLine(
                            $"  // Activator: {activator.ActivatedType} {activator.ActivatorMethodName}");
                    }

                    foreach (var childProvider in injectorValue.ChildProviders) {
                        b.Append(
                            $"  // ChildProvider: {childProvider.ChildInjectorType} {childProvider.ChildProviderMethodName}(");
                        b.Append(string.Join(", ", childProvider.Parameters));
                        b.AppendLine(")");
                    }

                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource(
                    $"Metadata\\Generated{injectorValue.InjectorInterfaceType.NamespacedBaseTypeName}.cs",
                    source);
            });
    }
}