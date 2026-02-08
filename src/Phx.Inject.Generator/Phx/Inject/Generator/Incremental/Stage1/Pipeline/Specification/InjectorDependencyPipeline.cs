// -----------------------------------------------------------------------------
// <copyright file="InjectorDependencyPipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1.Pipeline.Specification;

internal class InjectorDependencyPipeline(
    ICodeElementValidator elementValidator,
    IAttributeTransformer<InjectorDependencyAttributeMetadata> injectorDependencyAttributeTransformer,
    SpecFactoryMethodTransformer specFactoryMethodTransformer,
    SpecFactoryPropertyTransformer specFactoryPropertyTransformer
) : ISyntaxValuesPipeline<InjectorDependencyInterfaceMetadata> {
    public static readonly InjectorDependencyPipeline Instance = new(
        new InterfaceElementValidator(
            CodeElementAccessibility.PublicOrInternal
        ),
        InjectorDependencyAttributeTransformer.Instance,
        SpecFactoryMethodTransformer.Instance,
        SpecFactoryPropertyTransformer.Instance);
    
    public IncrementalValuesProvider<InjectorDependencyInterfaceMetadata> Select(SyntaxValueProvider syntaxProvider) {
        return syntaxProvider.ForAttributeWithMetadataName(
            InjectorDependencyAttributeMetadata.AttributeClassName,
            (syntaxNode, _) => elementValidator.IsValidSyntax(syntaxNode),
            (context, _) => {
                var targetSymbol = (ITypeSymbol)context.TargetSymbol;
                var injectorDependencyAttributeMetadata =
                    injectorDependencyAttributeTransformer.Transform(targetSymbol);

                var injectorDependencyInterfaceType = targetSymbol.ToTypeModel();
                
                var members = targetSymbol.GetMembers();
                var methods = members.OfType<IMethodSymbol>().ToImmutableList();
                var properties = members.OfType<IPropertySymbol>().ToImmutableList();
                
                var factoryMethods = methods
                    .Where(specFactoryMethodTransformer.CanTransform)
                    .Select(specFactoryMethodTransformer.Transform)
                    .ToImmutableArray();
                
                var factoryProperties = properties
                    .Where(specFactoryPropertyTransformer.CanTransform)
                    .Select(specFactoryPropertyTransformer.Transform)
                    .ToImmutableArray();
                
                return new InjectorDependencyInterfaceMetadata(
                    injectorDependencyInterfaceType,
                    factoryMethods,
                    factoryProperties,
                    injectorDependencyAttributeMetadata,
                    targetSymbol.GetLocationOrDefault().GeneratorIgnored()
                );
            });
    }
}
