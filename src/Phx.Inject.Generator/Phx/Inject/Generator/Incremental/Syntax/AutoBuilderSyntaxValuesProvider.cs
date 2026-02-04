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
using Phx.Inject.Generator.Incremental.Metadata;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Metadata.Auto;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Syntax;

internal class AutoBuilderSyntaxValuesProvider : IAttributeSyntaxValuesProvider<AutoBuilderMetadata> {
    public static readonly AutoBuilderSyntaxValuesProvider Instance = new();

    public string AttributeClassName { get; } =
        $"{PhxInject.NamespaceName}.{nameof(AutoBuilderAttribute)}";

    public bool CanProvide(SyntaxNode syntaxNode, CancellationToken cancellationToken) {
        if (syntaxNode is MethodDeclarationSyntax { Modifiers: var modifiers }) {
            return modifiers
                .Any(it => it.ValueText is "internal" or "public" )
                && modifiers .All(it => it.ValueText is not "private" and not "protected" and not "abstract");
        }
            
        return false;
    }

    public AutoBuilderMetadata Transform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    ) {
        var targetSymbol = (IMethodSymbol)context.TargetSymbol;
        var attributes = targetSymbol.GetAttributes();
        
        var autoBuilderType = new QualifiedTypeMetadata(targetSymbol.ReturnType.ToTypeModel(), null, null);
        var parameters = ImmutableArray<QualifiedTypeMetadata>.Empty;
        var autoBuilderAttributeMetadata = GetAutoBuilderAttributeMetadata(targetSymbol, attributes);
        return new AutoBuilderMetadata(
            targetSymbol.Name,
            autoBuilderType,
            parameters,
            autoBuilderAttributeMetadata,
            targetSymbol.GetLocationOrDefault().GeneratorIgnored()
        );
    }

    private AutoBuilderAttributeMetadata GetAutoBuilderAttributeMetadata(
        IMethodSymbol targetSymbol,
        IEnumerable<AttributeData> attributes
    ) {
        var attributeData = attributes
            .First(attribute => attribute.GetFullyQualifiedName() == AttributeClassName);
        var attributeMetadata = AttributeMetadata.Create(targetSymbol, attributeData);

        return new AutoBuilderAttributeMetadata(attributeMetadata);
    }
}