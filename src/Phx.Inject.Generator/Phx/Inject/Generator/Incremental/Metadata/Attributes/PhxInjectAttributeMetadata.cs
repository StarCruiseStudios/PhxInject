// -----------------------------------------------------------------------------
// <copyright file="PhxInjectAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Model;

namespace Phx.Inject.Generator.Incremental.Metadata.Attributes;

internal record PhxInjectAttributeMetadata(
    int? TabSize,
    string? GeneratedFileExtension,
    bool? NullableEnabled,
    bool? AllowConstructorFactories,
    AttributeMetadata AttributeMetadata
) : ISourceCodeElement {
    public static readonly string AttributeClassName =
        $"{PhxInject.NamespaceName}.{nameof(PhxInjectAttribute)}";

    public SourceLocation Location { get; } = AttributeMetadata.Location;

    public interface IValuesProvider {
        bool CanProvide(SyntaxNode syntaxNode, CancellationToken cancellationToken);
        PhxInjectAttributeMetadata Transform(
            GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken);
    }

    public class ValuesProvider : IValuesProvider {
        public static readonly ValuesProvider Instance = new();

        public bool CanProvide(SyntaxNode syntaxNode, CancellationToken cancellationToken) {
            // Relies on the generator pipeline to ensure this is a PhxInjectAttribute.
            return true;
        }

        public PhxInjectAttributeMetadata Transform(
            GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken) {
            var attributeData = context.Attributes.First();
            var targetSymbol = context.TargetSymbol;
            var attributeMetadata = AttributeMetadata.Create(targetSymbol, attributeData);

            return new PhxInjectAttributeMetadata(
                attributeData.NamedArguments
                    .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.TabSize))
                    .Value.Value as int?,
                attributeData.NamedArguments
                    .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.GeneratedFileExtension))
                    .Value.Value as string,
                attributeData.NamedArguments
                    .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.NullableEnabled))
                    .Value.Value as bool?,
                attributeData.NamedArguments
                    .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.AllowConstructorFactories))
                    .Value.Value as bool?,
                attributeMetadata);
        }
    }
}
