// -----------------------------------------------------------------------------
// <copyright file="FabricationModeExtractor.cs" company="Star Cruise Studios LLC">
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
using Phx.Inject.Common.Util;

namespace Phx.Inject.Generator.Extract.Metadata;

internal static class FactoryFabricationModeMetadata {
    internal interface IExtractor {
        FactoryFabricationMode Extract(
            ISymbol attributedSymbol,
            AttributeData attributeData,
            IGeneratorContext parentCtx);
    }

    internal class Extractor : IExtractor {
        public static readonly IExtractor Instance = new Extractor();

        public FactoryFabricationMode Extract(
            ISymbol attributedSymbol,
            AttributeData attributeData,
            IGeneratorContext parentCtx) {
            IReadOnlyList<FactoryFabricationMode> fabricationModes = attributeData.ConstructorArguments
                .Where(argument => argument.Type!.GetFullyQualifiedName() == TypeNames.FabricationModeClassName)
                .Select(argument => (FactoryFabricationMode)argument.Value!)
                .ToImmutableList();

            var fabricationMode = FactoryFabricationMode.Recurrent;
            switch (fabricationModes.Count) {
                case > 1:
                    throw Diagnostics.InvalidSpecification.AsException(
                        "Factories can only have a single fabrication mode.",
                        attributeData.GetAttributeLocation(attributedSymbol),
                        parentCtx);
                case 1:
                    fabricationMode = fabricationModes.Single();
                    break;
            }

            return fabricationMode;
        }
    }
}
