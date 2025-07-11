// -----------------------------------------------------------------------------
// <copyright file="FactoryAttributeMetadata.cs" company="Star Cruise Studios LLC">
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

internal class FactoryAttributeMetadata : AttributeMetadata {
    public const string FactoryAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(FactoryAttribute)}";

    public FactoryFabricationMode FabricationMode { get; }
    public FactoryAttributeMetadata(
        FactoryFabricationMode fabricationMode,
        ISymbol attributedSymbol,
        AttributeData attributeData)
        : base(attributedSymbol, attributeData) {
        FabricationMode = fabricationMode;
    }

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        IResult<FactoryAttributeMetadata> Extract(ISymbol attributedSymbol);
        void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
        void ValidateAttributedAutoConstructorType(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(AttributeHelper.Instance);
        private readonly IAttributeHelper attributeHelper;

        internal Extractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeHelper.HasAttribute(attributedSymbol, FactoryAttributeClassName);
        }

        public IResult<FactoryAttributeMetadata> Extract(ISymbol attributedSymbol) {
            return attributeHelper.ExpectSingleAttribute(
                attributedSymbol,
                FactoryAttributeClassName,
                attributeData => {
                    IReadOnlyList<FactoryFabricationMode> fabricationModes = attributeData.ConstructorArguments
                        .Where(argument => argument.Type!.ToString() == TypeNames.FabricationModeClassName)
                        .Select(argument => (FactoryFabricationMode)argument.Value!)
                        .ToImmutableList();

                    var fabricationMode = FactoryFabricationMode.Recurrent;
                    switch (fabricationModes.Count) {
                        case > 1:
                            return Result.Error<FactoryAttributeMetadata>(
                                "Factories can only have a single fabrication mode.",
                                GetAttributeLocation(attributeData, attributedSymbol),
                                Diagnostics.InternalError);
                        case 1:
                            fabricationMode = fabricationModes.Single();
                            break;
                    }

                    return Result.Ok(new FactoryAttributeMetadata(fabricationMode, attributedSymbol, attributeData));
                });
        }

        public void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
            if (attributedSymbol is not IMethodSymbol {
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
                and not IPropertySymbol {
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Factory {attributedSymbol.Name} must be a public or internal method or property.",
                    attributedSymbol.Locations.First(),
                    generatorCtx);
            }
        }

        public void ValidateAttributedAutoConstructorType(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
            if (attributedSymbol is not ITypeSymbol {
                    TypeKind: TypeKind.Class,
                    IsStatic: false,
                    IsAbstract: false,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Auto factory {attributedSymbol.Name} must be a public or internal non-abstract class.",
                    attributedSymbol.Locations.First(),
                    generatorCtx);
            }
        }
    }
}
