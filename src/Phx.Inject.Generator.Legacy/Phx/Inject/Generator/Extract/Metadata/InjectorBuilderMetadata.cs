// -----------------------------------------------------------------------------
// <copyright file="InjectorBuilderMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record InjectorBuilderMetadata(
    TypeModel InjectorInterfaceType,
    QualifiedTypeModel BuiltType,
    string BuilderMethodName,
    IMethodSymbol BuilderMethodSymbol
) : IMetadata {
    public Location Location {
        get => BuilderMethodSymbol.GetLocationOrDefault();
    }

    public interface IExtractor {
        bool CanExtract(IMethodSymbol builderMethodSymbol);
        InjectorBuilderMetadata Extract(
            TypeModel injectorInterfaceType,
            IMethodSymbol builderMethodSymbol,
            ExtractorContext parentCtx
        );
    }

    public class Extractor(QualifierMetadata.IAttributeExtractor qualifierExtractor) : IExtractor {
        public static IExtractor Instance { get; } = new Extractor(QualifierMetadata.AttributeExtractor.Instance);

        public bool CanExtract(IMethodSymbol builderMethodSymbol) {
            return VerifyExtract(builderMethodSymbol, null);
        }

        public InjectorBuilderMetadata Extract(
            TypeModel injectorInterfaceType,
            IMethodSymbol builderMethodSymbol,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting injector builder {builderMethodSymbol}",
                builderMethodSymbol,
                currentCtx => {
                    VerifyExtract(builderMethodSymbol, currentCtx);

                    var qualifier = qualifierExtractor.Extract(builderMethodSymbol, currentCtx);
                    var builtType = builderMethodSymbol.Parameters[0].Type.ToQualifiedTypeModel(qualifier);

                    return new InjectorBuilderMetadata(
                        injectorInterfaceType,
                        builtType,
                        builderMethodSymbol.Name,
                        builderMethodSymbol);
                });
        }

        private bool VerifyExtract(IMethodSymbol builderMethodSymbol, IGeneratorContext? currentCtx) {
            if (!builderMethodSymbol.ReturnsVoid) {
                return currentCtx == null
                    ? false
                    : throw Diagnostics.InternalError.AsException(
                        "Cannot extract injector builder from a method that has a return value.",
                        builderMethodSymbol.GetLocationOrDefault(),
                        currentCtx);
            }

            if (currentCtx != null) {
                if (builderMethodSymbol.Parameters.Length != 1) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Injector builder {builderMethodSymbol.Name} must have exactly 1 parameter.",
                        builderMethodSymbol.GetLocationOrDefault(),
                        currentCtx);
                }
            }

            return true;
        }
    }
}
