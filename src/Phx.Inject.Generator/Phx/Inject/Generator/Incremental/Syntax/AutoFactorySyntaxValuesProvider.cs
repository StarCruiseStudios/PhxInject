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

internal class AutoFactorySyntaxValuesProvider : IAttributeSyntaxValuesProvider<AutoFactoryMetadata> {
    public static readonly AutoFactorySyntaxValuesProvider Instance = new();

    public string AttributeClassName { get; } =
        $"{PhxInject.NamespaceName}.{nameof(AutoFactoryAttribute)}";
    
    private const string FabricationModeClassName = $"{PhxInject.NamespaceName}.{nameof(FabricationMode)}";

    public bool CanProvide(SyntaxNode syntaxNode, CancellationToken cancellationToken) {
        if (syntaxNode is ClassDeclarationSyntax { Modifiers: var modifiers }) {
            return modifiers
                .All(it => it.ValueText switch {
                    "private" or "protected" or "static" or "abstract" => false,
                    "internal" or "public" => true,
                    _ => true
                });
        }
            
        return false;
    }

    public AutoFactoryMetadata Transform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    ) {
        var targetSymbol = (ITypeSymbol)context.TargetSymbol;
        var attributes = targetSymbol.GetAttributes();
        
        var autoFactoryType = new QualifiedTypeMetadata(targetSymbol.ToTypeModel(), null, null);
        var parameters = ImmutableArray<QualifiedTypeMetadata>.Empty;
        var requiredProperties = ImmutableArray<AutoFactoryRequiredPropertyMetadata>.Empty;
        var autoFactoryAttributeMetadata = GetAutoFactoryAttributeMetadata(targetSymbol, attributes);
        return new AutoFactoryMetadata(
            autoFactoryType,
            parameters,
            requiredProperties,
            autoFactoryAttributeMetadata,
            targetSymbol.GetLocationOrDefault().GeneratorIgnored()
        );
    }

    private AutoFactoryAttributeMetadata GetAutoFactoryAttributeMetadata(
        ITypeSymbol targetSymbol,
        IEnumerable<AttributeData> attributes
    ) {
        var attributeData = attributes
            .First(attribute => attribute.GetFullyQualifiedName() == AttributeClassName);
        var attributeMetadata = AttributeMetadata.Create(targetSymbol, attributeData);

        var fabricationMode = attributeData.NamedArguments
                                  .FirstOrDefault(arg => arg.Key == nameof(AutoFactoryAttribute.FabricationMode))
                                  .Value.Value as FabricationMode?
                              ?? attributeData.ConstructorArguments
                                  .Where(argument => argument.Type!.GetFullyQualifiedName() == FabricationModeClassName)
                                  .Select(argument => (FabricationMode)argument.Value!)
                                  .FirstOrDefault();

        return new AutoFactoryAttributeMetadata(fabricationMode, attributeMetadata);
    }
}