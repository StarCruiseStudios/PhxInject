// -----------------------------------------------------------------------------
// <copyright file="AttributeDescriptors.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal interface IAttributeMetadata : IDescriptor {
    ISymbol AttributedSymbol { get; }
    INamedTypeSymbol AttributeTypeSymbol { get; }
    AttributeData AttributeData { get; }
}

internal class AttributeMetadata : IAttributeMetadata {
    protected AttributeMetadata(ISymbol attributedSymbol, AttributeData attributeData) {
        AttributedSymbol = attributedSymbol;
        AttributeTypeSymbol = attributeData.AttributeClass!;
        AttributeData = attributeData;
        Location = GetAttributeLocation(attributeData, attributedSymbol);
    }

    protected AttributeMetadata(ISymbol attributedSymbol, INamedTypeSymbol attributeTypeSymbol) {
        AttributedSymbol = attributedSymbol;
        AttributeTypeSymbol = attributeTypeSymbol;
        AttributeData = new EmptyAttributeData();
        Location = attributedSymbol.Locations.First();
    }
    public ISymbol AttributedSymbol { get; }
    public INamedTypeSymbol AttributeTypeSymbol { get; }
    public AttributeData AttributeData { get; }
    public Location Location { get; }

    protected static Location GetAttributeLocation(AttributeData attributeData, ISymbol attributedSymbol) {
        return attributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation()
            ?? attributedSymbol.Locations.First();
    }

    private class EmptyAttributeData : AttributeData {
        protected override INamedTypeSymbol? CommonAttributeClass { get; } = null;
        protected override IMethodSymbol? CommonAttributeConstructor { get; } = null;
        protected override SyntaxReference? CommonApplicationSyntaxReference { get; } = null;

        protected override ImmutableArray<TypedConstant> CommonConstructorArguments { get; } =
            ImmutableArray<TypedConstant>.Empty;

        protected override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments { get; } =
            ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty;
    }
    
    public interface IAttributeExtractor {
        bool CanExtract(ISymbol attributedSymbol, string attributeClassName);
        IResult<AttributeMetadata> ExtractOne(ISymbol attributedSymbol, string attributeClassName);
        IReadOnlyList<IResult<AttributeMetadata>> ExtractAll(ISymbol attributedSymbol, string attributeClassName);
    }

    public class AttributeExtractor : IAttributeExtractor {
        public static IAttributeExtractor Instance = new AttributeExtractor(AttributeHelper.Instance);

        private readonly IAttributeHelper attributeHelper;

        internal AttributeExtractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }
        
        public bool CanExtract(ISymbol attributedSymbol, string attributeClassName) {
            return attributeHelper.HasAttribute(attributedSymbol, attributeClassName);
        }

        public IResult<AttributeMetadata> ExtractOne(ISymbol attributedSymbol, string attributeClassName) {
            return attributeHelper.ExpectSingleAttribute(
                attributedSymbol,
                attributeClassName,
                attributeData => Result.Ok(new AttributeMetadata(attributedSymbol, attributeData)));
        }

        public IReadOnlyList<IResult<AttributeMetadata>> ExtractAll(ISymbol attributedSymbol, string attributeClassName) {
            return attributeHelper.GetAttributes(
                attributedSymbol,
                attributeClassName,
                attributeData => Result.Ok(new AttributeMetadata(attributedSymbol, attributeData)));
        }
    }
}
