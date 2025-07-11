// -----------------------------------------------------------------------------
// <copyright file="PartialAttributeMetadata.cs" company="Star Cruise Studios LLC">
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

internal class PartialAttributeMetadata : AttributeMetadata {
    public const string PartialAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(PartialAttribute)}";

    private static readonly IImmutableSet<string> PartialTypes = ImmutableHashSet.CreateRange(new[] {
        TypeNames.IReadOnlyListClassName, TypeNames.ISetClassName, TypeNames.IReadOnlyDictionaryClassName
    });

    public PartialAttributeMetadata(ISymbol attributedSymbol, AttributeData attributeData)
        : base(attributedSymbol, attributeData) { }

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        IResult<PartialAttributeMetadata> Extract(ISymbol attributedSymbol);
        void ValidateAttributedType(ISymbol attributedSymbol, TypeModel partialType, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(AttributeHelper.Instance);
        private readonly IAttributeHelper attributeHelper;

        internal Extractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }
        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeHelper.HasAttribute(attributedSymbol, PartialAttributeClassName);
        }

        public IResult<PartialAttributeMetadata> Extract(ISymbol attributedSymbol) {
            return attributeHelper.ExpectSingleAttribute(
                attributedSymbol,
                PartialAttributeClassName,
                attributeData => Result.Ok(
                    new PartialAttributeMetadata(attributedSymbol, attributeData)));
        }

        public void ValidateAttributedType(
            ISymbol attributedSymbol,
            TypeModel partialType,
            IGeneratorContext generatorCtx) {
            ExceptionAggregator.Try(
                "Validating partial type",
                generatorCtx,
                _ => {
                    if (attributedSymbol is not IMethodSymbol
                        and not IPropertySymbol
                        and not IFieldSymbol {
                            IsStatic: true,
                            DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                        }
                    ) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            $"Partial factory {attributedSymbol.Name} must be a public or internal static method, property or field.",
                            attributedSymbol.Locations.First(),
                            generatorCtx);
                    }
                },
                _ => {
                    if (!PartialTypes.Contains(partialType.NamespacedBaseTypeName)) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            $"Partial factories must return one of [{string.Join(", ", PartialTypes)}].",
                            attributedSymbol.Locations.First(),
                            generatorCtx);
                    }
                });
        }
    }
}
