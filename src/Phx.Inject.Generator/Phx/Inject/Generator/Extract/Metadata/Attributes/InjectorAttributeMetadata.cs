// -----------------------------------------------------------------------------
// <copyright file="InjectorAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal record InjectorAttributeMetadata(
    string? GeneratedClassName,
    IReadOnlyList<TypeModel> Specifications,
    AttributeMetadata AttributeMetadata
) : IDescriptor {
    public const string InjectorAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(InjectorAttribute)}";

    public Location Location { get; } = AttributeMetadata.Location;

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        InjectorAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx);
    }

    public class Extractor(AttributeMetadata.IAttributeExtractor attributeExtractor) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(AttributeMetadata.AttributeExtractor.Instance);

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeExtractor.CanExtract(attributedSymbol, InjectorAttributeClassName);
        }

        public InjectorAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx) {
            var attribute = attributeExtractor.ExtractOne(attributedSymbol, InjectorAttributeClassName, currentCtx);
            var generatedClassName = attribute.AttributeData.ConstructorArguments
                .FirstOrDefault(argument => argument.Kind != TypedConstantKind.Array)
                .Value as string;

            var specifications = attribute.AttributeData.ConstructorArguments
                .Where(argument => argument.Kind == TypedConstantKind.Array)
                .SelectMany(argument => argument.Values)
                .Select(type => type.Value as ITypeSymbol)
                .OfType<ITypeSymbol>()
                .Select(it => it.ToTypeModel())
                .ToImmutableList();

            return new InjectorAttributeMetadata(generatedClassName,
                specifications,
                attribute);
        }
    }
}
