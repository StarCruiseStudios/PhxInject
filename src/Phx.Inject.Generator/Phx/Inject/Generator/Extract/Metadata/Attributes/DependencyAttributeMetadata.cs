// -----------------------------------------------------------------------------
// <copyright file="DependencyAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal class DependencyAttributeMetadata : AttributeDesc {
    public const string DependencyAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(DependencyAttribute)}";

    public TypeModel DependencyType { get; }
    public DependencyAttributeMetadata(TypeModel dependencyType, ISymbol attributedSymbol, AttributeData attributeData)
        : base(attributedSymbol, attributeData) {
        DependencyType = dependencyType;
    }

    public interface IExtractor : IAttributeMetadataExtractor<DependencyAttributeMetadata> { }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(
            AttributeHelper.Instance,
            InjectorAttributeMetadata.Extractor.Instance);

        private readonly IAttributeHelper attributeHelper;
        private readonly InjectorAttributeMetadata.IExtractor injectorAttributeExtractor;

        internal Extractor(
            IAttributeHelper attributeHelper,
            InjectorAttributeMetadata.IExtractor injectorAttributeExtractor
        ) {
            this.attributeHelper = attributeHelper;
            this.injectorAttributeExtractor = injectorAttributeExtractor;
        }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeHelper.HasAttribute(attributedSymbol, DependencyAttributeClassName);
        }

        public IResult<DependencyAttributeMetadata> Extract(ISymbol attributedSymbol) {
            return attributeHelper.ExpectSingleAttribute(
                attributedSymbol,
                DependencyAttributeClassName,
                attributeData => {
                    var constructorArgument = attributeData.ConstructorArguments
                        .Where(argument => argument.Kind == TypedConstantKind.Type)
                        .Select(argument => argument.Value)
                        .OfType<ITypeSymbol>()
                        .ToImmutableList();

                    if (constructorArgument.Count != 1) {
                        return Result.Error<DependencyAttributeMetadata>(
                            $"Dependency for symbol {attributedSymbol.Name} must provide a dependency type.",
                            attributeData.GetLocation() ?? attributedSymbol.Locations.First(),
                            Diagnostics.InvalidSpecification);
                    }

                    return Result.Ok(new DependencyAttributeMetadata(constructorArgument.Single().ToTypeModel(),
                        attributedSymbol,
                        attributeData));
                });
        }

        public void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
            if (!injectorAttributeExtractor.CanExtract(attributedSymbol)) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Type {attributedSymbol.Name} must be an injector type to declare a {DependencyAttributeClassName}.",
                    attributedSymbol.Locations.First(),
                    generatorCtx);
            }

            injectorAttributeExtractor.ValidateAttributedType(attributedSymbol, generatorCtx);
        }
    }
}
