// -----------------------------------------------------------------------------
// <copyright file="InjectorDependencyPipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;
using static Phx.Inject.Common.Util.StringBuilderUtil;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Specification;

/// <summary>
/// Pipeline for processing InjectorDependency interface declarations into metadata.
/// </summary>
internal sealed class InjectorDependencyPipeline(
    ICodeElementValidator elementValidator,
    IAttributeTransformer<InjectorDependencyAttributeMetadata> injectorDependencyAttributeTransformer,
    ITransformer<IMethodSymbol, SpecFactoryMethodMetadata> specFactoryMethodTransformer,
    ITransformer<IPropertySymbol, SpecFactoryPropertyMetadata> specFactoryPropertyTransformer
) : ISyntaxValuesPipeline<InjectorDependencyInterfaceMetadata> {
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly InjectorDependencyPipeline Instance = new(
        InjectorDependencyInterfaceMetadata.ElementValidator,
        InjectorDependencyAttributeTransformer.Instance,
        SpecFactoryMethodTransformer.Instance,
        SpecFactoryPropertyTransformer.Instance);
    
    /// <inheritdoc />
    public IncrementalValuesProvider<IResult<InjectorDependencyInterfaceMetadata>> Select(
        SyntaxValueProvider syntaxProvider
    ) {
        return syntaxProvider.ForAttributeWithMetadataName(
            InjectorDependencyAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => DiagnosticsRecorder.Capture(diagnostics => {
                var targetSymbol = (ITypeSymbol)context.TargetSymbol;
                var injectorDependencyAttributeMetadata = injectorDependencyAttributeTransformer
                    .Transform(targetSymbol)
                    .OrThrow(diagnostics);

                var injectorDependencyInterfaceType = targetSymbol.ToTypeModel();
                
                var members = targetSymbol.GetMembers();
                var methods = members.OfType<IMethodSymbol>().ToImmutableList();
                var properties = members.OfType<IPropertySymbol>().ToImmutableList();
                
                var factoryMethods = methods
                    .Where(specFactoryMethodTransformer.CanTransform)
                    .Select(specFactoryMethodTransformer.Transform)
                    .SelectOrThrow(diagnostics)
                    .ToEquatableList();
                
                var factoryProperties = properties
                    .Where(specFactoryPropertyTransformer.CanTransform)
                    .Select(specFactoryPropertyTransformer.Transform)
                    .SelectOrThrow(diagnostics)
                    .ToEquatableList();
                
                return new InjectorDependencyInterfaceMetadata(
                    injectorDependencyInterfaceType,
                    factoryMethods,
                    factoryProperties,
                    injectorDependencyAttributeMetadata,
                    targetSymbol.GetLocationOrDefault().GeneratorIgnored()
                );
            }));
    }

    /// <summary>
    ///     DEBUG ONLY: Prints injector dependency interface metadata to diagnostic source files.
    /// </summary>
    /// <param name="context">The generator context for registering outputs.</param>
    /// <param name="segment">The injector dependency pipeline segment.</param>
    public void Print(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<IResult<InjectorDependencyInterfaceMetadata>> segment
    ) {
        context.RegisterSourceOutput(segment,
            (sourceProductionContext, injectorDependency) => {
                var diagnostics = new DiagnosticsRecorder();
                var injectorDepValue = injectorDependency.GetValue(diagnostics);
                var source = BuildString(b => {
                    b.AppendLine($"namespace Phx.Inject.Generator.Incremental.Metadata;");
                    b.AppendLine();
                    b.AppendLine($"class Generated{injectorDepValue.InjectorDependencyInterfaceType.BaseTypeName} {{");
                    foreach (var factoryMethod in injectorDepValue.FactoryMethods) {
                        b.Append($"  // FactoryMethod: {factoryMethod.FactoryReturnType} {factoryMethod.FactoryMethodName}(");
                        b.Append(string.Join(", ", factoryMethod.Parameters));
                        b.AppendLine(")");
                    }
                    foreach (var factoryProperty in injectorDepValue.FactoryProperties) {
                        b.AppendLine($"  // FactoryProperty: {factoryProperty.FactoryReturnType} {factoryProperty.FactoryPropertyName}");
                    }
                    b.AppendLine("}");
                });
                sourceProductionContext.AddSource($"Metadata\\Generated{injectorDepValue.InjectorDependencyInterfaceType.NamespacedBaseTypeName}.cs",
                    source);
            });
    }
}
