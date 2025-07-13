// -----------------------------------------------------------------------------
// <copyright file="DependencyAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal record DependencyAttributeMetadata(TypeModel DependencyType, AttributeMetadata AttributeMetadata)
    : IMetadata {
    public const string DependencyAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(DependencyAttribute)}";

    public Location Location { get; } = AttributeMetadata.Location;

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        DependencyAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx);
    }

    public class Extractor(AttributeMetadata.IAttributeExtractor attributeExtractor) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(AttributeMetadata.AttributeExtractor.Instance);

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeExtractor.CanExtract(attributedSymbol, DependencyAttributeClassName);
        }

        public DependencyAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx) {
            var attribute = attributeExtractor.ExtractOne(attributedSymbol, DependencyAttributeClassName, currentCtx);

            var constructorArgument = attribute.AttributeData.ConstructorArguments
                .Where(argument => argument.Kind == TypedConstantKind.Type)
                .Select(argument => argument.Value)
                .OfType<ITypeSymbol>()
                .ToImmutableList();

            if (constructorArgument.Count != 1) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Dependency for symbol {attributedSymbol.Name} must provide a dependency type.",
                    attribute.AttributeData.GetAttributeLocation(attributedSymbol),
                    currentCtx);
            }

            return new DependencyAttributeMetadata(
                constructorArgument.Single().ToTypeModel(),
                attribute);
        }
    }
}
