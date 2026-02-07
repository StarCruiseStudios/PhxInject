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
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage1;

internal class InjectorInterfaceSyntaxValuesProvider : IAttributeSyntaxValuesProvider<InjectorInterfaceMetadata> {
    public static readonly InjectorInterfaceSyntaxValuesProvider Instance = new();

    public string AttributeClassName { get; } =
        $"{PhxInject.NamespaceName}.{nameof(InjectorAttribute)}";

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

    public InjectorInterfaceMetadata Transform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    ) {
        var targetSymbol = (ITypeSymbol)context.TargetSymbol;
        var attributes = targetSymbol.GetAttributes();
        
        var injectorInterfaceType = targetSymbol.ToTypeModel();
        var providers = ImmutableArray<InjectorProviderMetadata>.Empty;
        var activators = ImmutableArray<InjectorActivatorMetadata>.Empty;
        var childFactories = ImmutableArray<InjectorChildProviderMetadata>.Empty;
        var injectorAttributeMetadata = GetInjectorAttributeMetadata(targetSymbol, attributes);
        DependencyAttributeMetadata? dependencyAttributeMetadata = null;
        return new InjectorInterfaceMetadata(
            injectorInterfaceType,
            providers,
            activators,
            childFactories,
            injectorAttributeMetadata,
            dependencyAttributeMetadata,
            targetSymbol.GetLocationOrDefault().GeneratorIgnored()
        );
    }

    private InjectorAttributeMetadata GetInjectorAttributeMetadata(
        ITypeSymbol targetSymbol,
        IEnumerable<AttributeData> attributes
    ) {
        var attributeData = attributes
            .First(attribute => attribute.GetFullyQualifiedName() == AttributeClassName);
        var attributeMetadata = AttributeMetadata.Create(targetSymbol, attributeData);

        var generatedClassName = attributeData.NamedArguments
                                     .FirstOrDefault(arg => arg.Key == nameof(InjectorAttribute.GeneratedClassName))
                                     .Value.Value as string
                                 ?? attributeData.ConstructorArguments
                                     .FirstOrDefault(argument => argument.Kind != TypedConstantKind.Array)
                                     .Value as string;
            
        var specifications = attributeData.ConstructorArguments
            .Where(argument => argument.Kind != TypedConstantKind.Array)
            .SelectMany(argument => argument.Values)
            .Select(type => type.Value as ITypeSymbol)
            .OfType<ITypeSymbol>()
            .Select(it => it.ToTypeModel())
            .ToImmutableList();
            
        return new InjectorAttributeMetadata(
            generatedClassName,
            specifications,
            attributeMetadata);
    }
}