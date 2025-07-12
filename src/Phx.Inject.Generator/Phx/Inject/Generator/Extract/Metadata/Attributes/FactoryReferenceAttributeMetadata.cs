// -----------------------------------------------------------------------------
// <copyright file="FactoryReferenceAttributeMetadata.cs" company="Star Cruise Studios LLC">
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

internal class FactoryReferenceAttributeMetadata : AttributeMetadata {
    public const string FactoryReferenceAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(FactoryReferenceAttribute)}";

    public FactoryFabricationMode FabricationMode { get; }
    public FactoryReferenceAttributeMetadata(
        FactoryFabricationMode fabricationMode,
        ISymbol attributedSymbol,
        AttributeData attributeData)
        : base(attributedSymbol, attributeData) {
        FabricationMode = fabricationMode;
    }

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        IResult<FactoryReferenceAttributeMetadata> Extract(ISymbol attributedSymbol);
        void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(AttributeHelper.Instance);
        private readonly IAttributeHelper attributeHelper;

        internal Extractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeHelper.HasAttribute(attributedSymbol, FactoryReferenceAttributeClassName);
        }

        public IResult<FactoryReferenceAttributeMetadata> Extract(ISymbol attributedSymbol) {
            return attributeHelper.ExpectSingleAttributeResult(
                attributedSymbol,
                FactoryReferenceAttributeClassName,
                attributeData => {
                    IReadOnlyList<FactoryFabricationMode> fabricationModes = attributeData.ConstructorArguments
                        .Where(argument => argument.Type!.ToString() == TypeNames.FabricationModeClassName)
                        .Select(argument => (FactoryFabricationMode)argument.Value!)
                        .ToImmutableList();

                    var fabricationMode = FactoryFabricationMode.Recurrent;
                    switch (fabricationModes.Count) {
                        case > 1:
                            return Result.Error<FactoryReferenceAttributeMetadata>(
                                "Factory references can only have a single fabrication mode.",
                                attributeData.GetAttributeLocation(attributedSymbol),
                                Diagnostics.InternalError);
                        case 1:
                            fabricationMode = fabricationModes.Single();
                            break;
                    }

                    return Result.Ok(
                        new FactoryReferenceAttributeMetadata(fabricationMode, attributedSymbol, attributeData));
                });
        }

        public void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
            if (attributedSymbol is not IFieldSymbol {
                    IsStatic: true,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
                and not IPropertySymbol {
                    IsStatic: true,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Factory reference {attributedSymbol.Name} must be a public or internal static property or field.",
                    attributedSymbol.Locations.First(),
                    generatorCtx);
            }
        }
    }
}
