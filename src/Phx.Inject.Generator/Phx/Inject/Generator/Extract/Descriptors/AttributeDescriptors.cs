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

namespace Phx.Inject.Generator.Extract.Descriptors;

internal interface IAttributeDesc : IDescriptor {
    ISymbol AttributedSymbol { get; }
    INamedTypeSymbol AttributeTypeSymbol { get; }
    AttributeData AttributeData { get; }
}

internal abstract class AttributeDesc : IAttributeDesc {
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

internal class DependencyAttributeDesc : AttributeDesc {
    public const string DependencyAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(DependencyAttribute)}";

    public ITypeSymbol DependencyType { get; }
    public DependencyAttributeDesc(ITypeSymbol dependencyType, ISymbol attributedSymbol, AttributeData attributeData)
        : base(attributedSymbol, attributeData) {
        DependencyType = dependencyType;
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

internal class InjectorAttributeDesc : AttributeDesc {
    public const string InjectorAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(InjectorAttribute)}";

    public string? GeneratedClassName { get; }
    public IReadOnlyList<ITypeSymbol> Specifications { get; }
    public InjectorAttributeDesc(
        string? generatedClassName,
        IReadOnlyList<ITypeSymbol> specifications,
        ISymbol attributedSymbol,
        AttributeData attributeData)
        : base(attributedSymbol, attributeData) {
        GeneratedClassName = generatedClassName;
        Specifications = specifications;
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

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        IResult<QualifierAttributeDesc> Extract(ISymbol attributedSymbol);
        void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        private readonly IAttributeHelper attributeHelper;

        public Extractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }

        public Extractor() : this(new AttributeHelper()) { }

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

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        IResult<SpecificationAttributeDesc> Extract(ISymbol attributedSymbol);
        void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        private readonly IAttributeHelper attributeHelper;

        public Extractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }

        public Extractor() : this(new AttributeHelper()) { }

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
