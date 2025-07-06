// -----------------------------------------------------------------------------
// <copyright file="AttributeDescriptors.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Model;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal interface IAttributeDesc {
    ISymbol AttributedSymbol { get; }
    INamedTypeSymbol AttributeTypeSymbol { get; }
    AttributeData AttributeData { get; }
}

internal abstract class AttributeDesc : IAttributeDesc {
    public ISymbol AttributedSymbol { get; }
    public INamedTypeSymbol AttributeTypeSymbol { get; }
    public AttributeData AttributeData { get; }

    protected AttributeDesc(ISymbol attributedSymbol,AttributeData attributeData) {
        AttributedSymbol = attributedSymbol;
        AttributeTypeSymbol = attributeData.AttributeClass!;
        AttributeData = attributeData;
    }
}

internal class BuilderAttributeDesc : AttributeDesc {
    public BuilderAttributeDesc(ISymbol attributedSymbol, AttributeData attributeData) 
        : base(attributedSymbol, attributeData) { }
}

internal class BuilderReferenceAttributeDesc : AttributeDesc {
    public BuilderReferenceAttributeDesc(ISymbol attributedSymbol, AttributeData attributeData) 
        : base(attributedSymbol, attributeData) { }
}

internal class ChildInjectorAttributeDesc : AttributeDesc {
    public ChildInjectorAttributeDesc(ISymbol attributedSymbol, AttributeData attributeData) 
        : base(attributedSymbol, attributeData) { }
}

internal class DependencyAttributeDesc : AttributeDesc {
    public ITypeSymbol DependencyType { get; }
    public DependencyAttributeDesc(ITypeSymbol dependencyType,ISymbol attributedSymbol, AttributeData attributeData) 
        : base(attributedSymbol, attributeData)
    {
        DependencyType = dependencyType;
    }
}

internal class FactoryAttributeDesc : AttributeDesc {
    public FactoryFabricationMode FabricationMode { get; }
    public FactoryAttributeDesc(
        FactoryFabricationMode fabricationMode,
        ISymbol attributedSymbol,
        AttributeData attributeData)
        : base(attributedSymbol, attributeData
    ) {
        FabricationMode = fabricationMode;
        
    }
}

internal class FactoryReferenceAttributeDesc : AttributeDesc {
    public FactoryFabricationMode FabricationMode { get; }
    public FactoryReferenceAttributeDesc(
        FactoryFabricationMode fabricationMode,
        ISymbol attributedSymbol,
        AttributeData attributeData)
        : base(attributedSymbol, attributeData
    ) {
        FabricationMode = fabricationMode;
    }
}

internal class InjectorAttributeDesc : AttributeDesc {
    public string? GeneratedClassName { get; }
    public IReadOnlyList<ITypeSymbol> Specifications { get; }
    public InjectorAttributeDesc(
        string? generatedClassName,
        IReadOnlyList<ITypeSymbol> specifications,
        ISymbol attributedSymbol,
        AttributeData attributeData)
        : base(attributedSymbol, attributeData
    ) {
        GeneratedClassName = generatedClassName;
        Specifications = specifications;
    }
}

internal class LabelAttributeDesc : AttributeDesc {
    public string Label { get; }
    public LabelAttributeDesc(string label, ISymbol attributedSymbol, AttributeData attributeData) 
        : base(attributedSymbol, attributeData)
    {
        Label = label;
    }
}

internal class LinkAttributeDesc : AttributeDesc {
    public ITypeSymbol InputType { get; }
    public ITypeSymbol OutputType { get; }
    public LinkAttributeDesc(ITypeSymbol inputType, ITypeSymbol outputType, ISymbol attributedSymbol, AttributeData attributeData) 
        : base(attributedSymbol, attributeData)
    {
        InputType = inputType;
        OutputType = outputType;
    }
}

internal class PartialAttributeDesc : AttributeDesc {
    public PartialAttributeDesc(ISymbol attributedSymbol, AttributeData attributeData) 
        : base(attributedSymbol, attributeData) { }
}

internal class PhxInjectAttributeDesc : AttributeDesc {
    public int? TabSize { get; }
    public string? GeneratedFileExtension { get; }
    public bool? NullableEnabled { get; }
    public bool? AllowConstructorFactories { get; }

    public PhxInjectAttributeDesc(
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
}

internal class QualifierAttributeDesc : AttributeDesc {
    public QualifierAttributeDesc(ISymbol attributedSymbol, AttributeData attributeData) 
        : base(attributedSymbol, attributeData) { }
}

internal class SpecificationAttributeDesc : AttributeDesc {
    public SpecificationAttributeDesc(ISymbol attributedSymbol, AttributeData attributeData) 
        : base(attributedSymbol, attributeData) { }
}