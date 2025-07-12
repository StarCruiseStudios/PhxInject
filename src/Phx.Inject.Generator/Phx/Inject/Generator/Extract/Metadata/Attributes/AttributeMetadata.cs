// -----------------------------------------------------------------------------
// <copyright file="AttributeDescriptors.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal interface IAttributeMetadata : IDescriptor {
    ISymbol AttributedSymbol { get; }
    INamedTypeSymbol AttributeTypeSymbol { get; }
    AttributeData AttributeData { get; }
}

internal record AttributeMetadata(
    ISymbol AttributedSymbol,
    INamedTypeSymbol AttributeTypeSymbol,
    AttributeData AttributeData,
    Location Location
) : IAttributeMetadata {
    private AttributeMetadata(ISymbol attributedSymbol, AttributeData attributeData) : this(
        attributedSymbol,
        attributeData.GetNamedTypeSymbol(),
        attributeData,
        attributeData.GetAttributeLocation(attributedSymbol)
    ) { }

    private AttributeMetadata(ISymbol attributedSymbol, INamedTypeSymbol attributeTypeSymbol) : this(
        attributedSymbol,
        attributeTypeSymbol,
        new EmptyAttributeData(),
        attributedSymbol.GetLocationOrDefault()
    ) { }

    private class EmptyAttributeData : AttributeData {
        protected override INamedTypeSymbol? CommonAttributeClass { get => null; }
        protected override IMethodSymbol? CommonAttributeConstructor { get => null; }
        protected override SyntaxReference? CommonApplicationSyntaxReference { get => null; }

        protected override ImmutableArray<TypedConstant> CommonConstructorArguments {
            get => ImmutableArray<TypedConstant>.Empty;
        }

        protected override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments {
            get => ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty;
        }
    }

    public interface IAttributeExtractor {
        bool CanExtract(ISymbol attributedSymbol, string attributeClassName);
        AttributeMetadata ExtractOne(
            ISymbol attributedSymbol,
            string attributeClassName,
            IGeneratorContext generatorCtx);
        IReadOnlyList<AttributeMetadata> ExtractAll(
            ISymbol attributedSymbol,
            string attributeClassName,
            IGeneratorContext generatorCtx);
    }

    public class AttributeExtractor(IAttributeHelper attributeHelper) : IAttributeExtractor {
        public static readonly IAttributeExtractor Instance = new AttributeExtractor(AttributeHelper.Instance);

        public bool CanExtract(ISymbol attributedSymbol, string attributeClassName) {
            return attributeHelper.HasAttribute(attributedSymbol, attributeClassName);
        }

        public AttributeMetadata ExtractOne(
            ISymbol attributedSymbol,
            string attributeClassName,
            IGeneratorContext generatorCtx) {
            return attributeHelper.ExpectSingleAttribute(
                attributedSymbol,
                attributeClassName,
                generatorCtx,
                attributeData => new AttributeMetadata(attributedSymbol, attributeData));
        }

        public IReadOnlyList<AttributeMetadata> ExtractAll(
            ISymbol attributedSymbol,
            string attributeClassName,
            IGeneratorContext generatorCtx) {
            return attributeHelper.GetAttributes(
                attributedSymbol,
                attributeClassName,
                generatorCtx,
                attributeData => new AttributeMetadata(attributedSymbol, attributeData));
        }
    }

    public interface ITypeExtractor {
        AttributeMetadata Extract(
            ISymbol attributedSymbol,
            ISymbol attributeTypeSymbol,
            IGeneratorContext generatorCtx);
    }

    public class TypeExtractor : ITypeExtractor {
        public static readonly ITypeExtractor Instance = new TypeExtractor();

        internal TypeExtractor() { }

        public AttributeMetadata Extract(
            ISymbol attributedSymbol,
            ISymbol attributeTypeSymbol,
            IGeneratorContext generatorCtx) {
            var namedSymbol = attributeTypeSymbol as INamedTypeSymbol;
            if (namedSymbol == null) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Expected attribute {attributeTypeSymbol} to be a type.",
                    attributedSymbol.GetLocationOrDefault(),
                    generatorCtx);
            }

            return new AttributeMetadata(attributedSymbol, namedSymbol);
        }
    }

    internal interface IAttributeHelper {
        bool HasAttribute(ISymbol symbol, string attributeClassName);
        IReadOnlyList<T> GetAttributes<T>(
            ISymbol symbol,
            string attributeClassName,
            IGeneratorContext generatorCtx,
            Func<AttributeData, T> create);
        T ExpectSingleAttribute<T>(
            ISymbol symbol,
            string attributeClassName,
            IGeneratorContext generatorCtx,
            Func<AttributeData, T> create
        );
    }

    internal class AttributeHelper : IAttributeHelper {
        public static IAttributeHelper Instance { get; } = new AttributeHelper();
        public bool HasAttribute(ISymbol symbol, string attributeClassName) {
            return symbol.GetAttributes()
                .Any(attributeData => attributeData.GetFullyQualifiedName() == attributeClassName);
        }

        public IReadOnlyList<T> GetAttributes<T>(
            ISymbol symbol,
            string attributeClassName,
            IGeneratorContext generatorCtx,
            Func<AttributeData, T> create) {
            return symbol.GetAttributes()
                .Where(attributeData => attributeData.GetFullyQualifiedName() == attributeClassName)
                .SelectCatching(
                    generatorCtx.Aggregator,
                    attributeData => $"extracting attribute ${attributeData.GetFullyQualifiedName()}",
                    create)
                .ToImmutableList();
        }

        public T ExpectSingleAttribute<T>(
            ISymbol symbol,
            string attributeClassName,
            IGeneratorContext generatorCtx,
            Func<AttributeData, T> create
        ) {
            var attributes = symbol.GetAttributes()
                .Where(attributeData => attributeData.GetFullyQualifiedName() == attributeClassName)
                .ToImmutableList();

            return attributes.Count switch {
                1 => create(attributes.Single()),
                > 1 => throw Diagnostics.InvalidSpecification.AsException(
                    $"Type {symbol.Name} cannot have more than one {attributeClassName}. Found {attributes.Count}.",
                    symbol.GetLocationOrDefault(),
                    generatorCtx),
                _ => throw Diagnostics.InvalidSpecification.AsException(
                    $"Type {symbol.Name} must have an {attributeClassName}.",
                    symbol.GetLocationOrDefault(),
                    generatorCtx)
            };
        }
    }
}
