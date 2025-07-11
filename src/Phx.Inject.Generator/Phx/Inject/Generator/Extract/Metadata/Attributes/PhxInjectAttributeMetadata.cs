// -----------------------------------------------------------------------------
// <copyright file="PhxInjectAttributeDesc.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal class PhxInjectAttributeMetadata : AttributeMetadata {
    public static readonly string PhxInjectAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(PhxInjectAttribute)}";

    public int? TabSize { get; }
    public string? GeneratedFileExtension { get; }
    public bool? NullableEnabled { get; }
    public bool? AllowConstructorFactories { get; }

    public PhxInjectAttributeMetadata(
        int? tabSize,
        string? generatedFileExtension,
        bool? nullableEnabled,
        bool? allowConstructorFactories,
        ISymbol attributedSymbol,
        AttributeData attributeData
    ) : base(attributedSymbol, attributeData) {
        TabSize = tabSize;
        GeneratedFileExtension = generatedFileExtension;
        NullableEnabled = nullableEnabled;
        AllowConstructorFactories = allowConstructorFactories;
    }

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        IResult<PhxInjectAttributeMetadata> Extract(ISymbol attributedSymbol);
        void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(AttributeHelper.Instance);
        private readonly IAttributeHelper attributeHelper;

        internal Extractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeHelper.HasAttribute(attributedSymbol, PhxInjectAttributeClassName);
        }

        public IResult<PhxInjectAttributeMetadata> Extract(ISymbol attributedSymbol) {
            return attributeHelper.ExpectSingleAttribute(
                attributedSymbol,
                PhxInjectAttributeClassName,
                attributeData => {
                    var tabSize = attributeData.NamedArguments
                        .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.TabSize))
                        .Value.Value as int?;

                    var generatedFileExtension = attributeData.NamedArguments
                        .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.GeneratedFileExtension))
                        .Value.Value as string;

                    var nullableEnabled = attributeData.NamedArguments
                        .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.NullableEnabled))
                        .Value.Value as bool?;

                    var allowConstructorFactories = attributeData.NamedArguments
                        .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.AllowConstructorFactories))
                        .Value.Value as bool?;

                    return Result.Ok(
                        new PhxInjectAttributeMetadata(
                            tabSize,
                            generatedFileExtension,
                            nullableEnabled,
                            allowConstructorFactories,
                            attributedSymbol,
                            attributeData));
                });
        }

        public void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
            if (attributedSymbol is not ITypeSymbol { TypeKind: TypeKind.Class }) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"PhxInject settings type {attributedSymbol.Name} must be a class.",
                    attributedSymbol.Locations.First(),
                    generatorCtx);
            }
        }
    }
}
