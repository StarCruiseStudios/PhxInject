// -----------------------------------------------------------------------------
// <copyright file="InjectorAttributeMetadata.cs" company="Star Cruise Studios LLC">
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

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal class InjectorAttributeMetadata : AttributeMetadata {
    public const string InjectorAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(InjectorAttribute)}";

    public string? GeneratedClassName { get; }
    public IReadOnlyList<TypeModel> Specifications { get; }
    public InjectorAttributeMetadata(
        string? generatedClassName,
        IReadOnlyList<TypeModel> specifications,
        ISymbol attributedSymbol,
        AttributeData attributeData)
        : base(attributedSymbol, attributeData) {
        GeneratedClassName = generatedClassName;
        Specifications = specifications;
    }

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        IResult<InjectorAttributeMetadata> Extract(ISymbol attributedSymbol);
        void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(AttributeHelper.Instance);

        private readonly IAttributeHelper attributeHelper;

        internal Extractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeHelper.HasAttribute(attributedSymbol, InjectorAttributeClassName);
        }

        public IResult<InjectorAttributeMetadata> Extract(ISymbol attributedSymbol) {
            return attributeHelper.ExpectSingleAttributeResult(
                attributedSymbol,
                InjectorAttributeClassName,
                attributeData => {
                    var generatedClassName = attributeData.ConstructorArguments
                        .FirstOrDefault(argument => argument.Kind != TypedConstantKind.Array)
                        .Value as string;

                    var specifications = attributeData.ConstructorArguments
                        .Where(argument => argument.Kind == TypedConstantKind.Array)
                        .SelectMany(argument => argument.Values)
                        .Select(type => type.Value as ITypeSymbol)
                        .OfType<ITypeSymbol>()
                        .Select(it => it.ToTypeModel())
                        .ToImmutableList();

                    return Result.Ok(new InjectorAttributeMetadata(generatedClassName,
                        specifications,
                        attributedSymbol,
                        attributeData));
                });
        }

        public void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
            if (attributedSymbol is not ITypeSymbol {
                    TypeKind: TypeKind.Interface,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Injector type {attributedSymbol.Name} must be a public or internal interface.",
                    attributedSymbol.Locations.First(),
                    generatorCtx);
            }
        }
    }
}
