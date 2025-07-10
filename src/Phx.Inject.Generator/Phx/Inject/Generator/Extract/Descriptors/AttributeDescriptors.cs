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
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal interface IAttributeDesc : IDescriptor {
    ISymbol AttributedSymbol { get; }
    INamedTypeSymbol AttributeTypeSymbol { get; }
    AttributeData AttributeData { get; }
}

internal abstract class AttributeDesc : IAttributeDesc {
    public Location AttributedLocation {
        get => AttributedSymbol.Locations.First();
    }

    protected AttributeDesc(ISymbol attributedSymbol, AttributeData attributeData) {
        AttributedSymbol = attributedSymbol;
        AttributeTypeSymbol = attributeData.AttributeClass!;
        AttributeData = attributeData;
        Location = attributeData.GetLocation() ?? attributedSymbol.Locations.First();
    }

    protected AttributeDesc(ISymbol attributedSymbol, INamedTypeSymbol attributeTypeSymbol) {
        AttributedSymbol = attributedSymbol;
        AttributeTypeSymbol = attributeTypeSymbol;
        AttributeData = new EmptyAttributeData();
        Location = attributedSymbol.Locations.First();
    }
    public ISymbol AttributedSymbol { get; }
    public INamedTypeSymbol AttributeTypeSymbol { get; }
    public AttributeData AttributeData { get; }
    public Location Location { get; }

    private class EmptyAttributeData : AttributeData {
        protected override INamedTypeSymbol? CommonAttributeClass { get; } = null;
        protected override IMethodSymbol? CommonAttributeConstructor { get; } = null;
        protected override SyntaxReference? CommonApplicationSyntaxReference { get; } = null;

        protected override ImmutableArray<TypedConstant> CommonConstructorArguments { get; } =
            ImmutableArray<TypedConstant>.Empty;

        protected override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments { get; } =
            ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty;
    }
}

internal class FactoryAttributeDesc : AttributeDesc {
    public const string FactoryAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(FactoryAttribute)}";

    public FactoryFabricationMode FabricationMode { get; }
    public FactoryAttributeDesc(
        FactoryFabricationMode fabricationMode,
        ISymbol attributedSymbol,
        AttributeData attributeData)
        : base(attributedSymbol, attributeData) {
        FabricationMode = fabricationMode;
    }
}

internal class FactoryReferenceAttributeDesc : AttributeDesc {
    public const string FactoryReferenceAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(FactoryReferenceAttribute)}";

    public FactoryFabricationMode FabricationMode { get; }
    public FactoryReferenceAttributeDesc(
        FactoryFabricationMode fabricationMode,
        ISymbol attributedSymbol,
        AttributeData attributeData)
        : base(attributedSymbol, attributeData) {
        FabricationMode = fabricationMode;
    }
}

internal class LabelAttributeDesc : AttributeDesc {
    public const string LabelAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(LabelAttribute)}";

    public string Label { get; }
    public LabelAttributeDesc(string label, ISymbol attributedSymbol, AttributeData attributeData)
        : base(attributedSymbol, attributeData) {
        Label = label;
    }

    public LabelAttributeDesc(string label, ISymbol attributedSymbol, INamedTypeSymbol attributeTypeSymbol)
        : base(attributedSymbol, attributeTypeSymbol) {
        Label = label;
    }
}

internal class LinkAttributeDesc : AttributeDesc {
    public const string LinkAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(LinkAttribute)}";

    public ITypeSymbol InputType { get; }
    public ITypeSymbol OutputType { get; }
    public INamedTypeSymbol? InputQualifier { get; }
    public string? InputLabel { get; }
    public INamedTypeSymbol? OutputQualifier { get; }
    public string? OutputLabel { get; }

    public LinkAttributeDesc(
        ITypeSymbol inputType,
        ITypeSymbol outputType,
        INamedTypeSymbol? inputQualifier,
        string? inputLabel,
        INamedTypeSymbol? outputQualifier,
        string? outputLabel,
        ISymbol attributedSymbol,
        AttributeData attributeData
    ) : base(attributedSymbol, attributeData) {
        InputType = inputType;
        OutputType = outputType;
        InputQualifier = inputQualifier;
        InputLabel = inputLabel;
        OutputQualifier = outputQualifier;
        OutputLabel = outputLabel;
    }
}

internal class QualifierAttributeDesc : AttributeDesc {
    public const string QualifierAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(QualifierAttribute)}";

    public QualifierAttributeDesc(ISymbol attributedSymbol, AttributeData attributeData)
        : base(attributedSymbol, attributeData) { }

    public QualifierAttributeDesc(ISymbol attributedSymbol, INamedTypeSymbol attributeTypeSymbol)
        : base(attributedSymbol, attributeTypeSymbol) { }

    public interface IExtractor : IAttributeMetadataExtractor<QualifierAttributeDesc> { }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(AttributeHelper.Instance);
        private readonly IAttributeHelper attributeHelper;

        internal Extractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeHelper.HasAttribute(attributedSymbol, QualifierAttributeClassName);
        }

        public IResult<QualifierAttributeDesc> Extract(ISymbol attributedSymbol) {
            return attributeHelper.ExpectSingleAttribute(
                attributedSymbol,
                QualifierAttributeClassName,
                attributeData => Result.Ok(
                    new QualifierAttributeDesc(attributedSymbol, attributeData)));
        }

        public void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
            if (attributedSymbol is not IMethodSymbol {
                    IsStatic: true,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Builder {attributedSymbol.Name} must be a public or internal static method.",
                    attributedSymbol.Locations.First(),
                    generatorCtx);
            }
        }
    }
}

internal class SpecificationAttributeDesc : AttributeDesc {
    public const string SpecificationAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(SpecificationAttribute)}";

    public SpecificationAttributeDesc(ISymbol attributedSymbol, AttributeData attributeData)
        : base(attributedSymbol, attributeData) { }

    public interface IExtractor : IAttributeMetadataExtractor<SpecificationAttributeDesc> { }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(AttributeHelper.Instance);
        private readonly IAttributeHelper attributeHelper;

        internal Extractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeHelper.HasAttribute(attributedSymbol, SpecificationAttributeClassName);
        }

        public IResult<SpecificationAttributeDesc> Extract(ISymbol attributedSymbol) {
            return attributeHelper.ExpectSingleAttribute(
                attributedSymbol,
                SpecificationAttributeClassName,
                attributeData => Result.Ok(
                    new SpecificationAttributeDesc(attributedSymbol, attributeData)));
        }

        public void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
            if (attributedSymbol is not IMethodSymbol {
                    IsStatic: true,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Builder {attributedSymbol.Name} must be a public or internal static method.",
                    attributedSymbol.Locations.First(),
                    generatorCtx);
            }
        }
    }
}
