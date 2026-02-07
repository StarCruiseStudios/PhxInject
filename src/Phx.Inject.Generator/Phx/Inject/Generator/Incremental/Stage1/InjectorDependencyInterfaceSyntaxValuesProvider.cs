// -----------------------------------------------------------------------------
// <copyright file="InjectorAttributeSyntaxValuesProvider.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Stage1.Metadata;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1;

internal class InjectorDependencyInterfaceSyntaxValuesProvider : IAttributeSyntaxValuesProvider<InjectorDependencyInterfaceMetadata> {
    public static readonly InjectorDependencyInterfaceSyntaxValuesProvider Instance = new();

    public string AttributeClassName { get; } =
        $"{PhxInject.NamespaceName}.{nameof(InjectorDependencyAttribute)}";

    public bool CanProvide(SyntaxNode syntaxNode, CancellationToken cancellationToken) {
        if (syntaxNode is InterfaceDeclarationSyntax { Modifiers: var modifiers }) {
            return modifiers
                .All(it => it.ValueText switch {
                    "private" or "protected" => false,
                    "internal" or "public" => true,
                    _ => true
                });
        }
            
        return false;
    }

    public InjectorDependencyInterfaceMetadata Transform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    ) {
        var targetSymbol = (ITypeSymbol)context.TargetSymbol;
        var attributes = targetSymbol.GetAttributes();
        
        var injectorDependencyInterfaceType = targetSymbol.ToTypeModel();
        var factoryMethods = ImmutableArray<SpecFactoryMethodMetadata>.Empty;
        var factoryProperties = ImmutableArray<SpecFactoryPropertyMetadata>.Empty;
        var injectorDependencyAttributeMetadata = GetInjectorDependencyAttributeMetadata(targetSymbol, attributes);
        return new InjectorDependencyInterfaceMetadata(
            injectorDependencyInterfaceType,
            factoryMethods,
            factoryProperties,
            injectorDependencyAttributeMetadata,
            targetSymbol.GetLocationOrDefault().GeneratorIgnored()
        );
    }

    private InjectorDependencyAttributeMetadata GetInjectorDependencyAttributeMetadata(
        ITypeSymbol targetSymbol,
        IEnumerable<AttributeData> attributes
    ) {
        var attributeData = attributes
            .First(attribute => attribute.GetFullyQualifiedName() == AttributeClassName);
        var attributeMetadata = AttributeMetadata.Create(targetSymbol, attributeData);

        return new InjectorDependencyAttributeMetadata(attributeMetadata);
    }
}