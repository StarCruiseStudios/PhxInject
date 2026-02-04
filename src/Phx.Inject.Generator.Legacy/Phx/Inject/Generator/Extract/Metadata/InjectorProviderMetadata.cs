// -----------------------------------------------------------------------------
// <copyright file="InjectorProviderMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record InjectorProviderMetadata(
    TypeModel InjectorInterfaceType,
    QualifiedTypeModel ProvidedType,
    string ProviderMethodName,
    IMethodSymbol ProviderMethodSymbol
) : IMetadata {
    public Location Location {
        get => ProviderMethodSymbol.GetLocationOrDefault();
    }

    public interface IExtractor {
        bool CanExtract(IMethodSymbol providerMethodSymbol);
        InjectorProviderMetadata Extract(
            TypeModel injectorInterfaceType,
            IMethodSymbol providerMethodSymbol,
            ExtractorContext parentCtx
        );
    }

    public class Extractor(
        ChildInjectorAttributeMetadata.IExtractor childInjectorAttributeExtractor,
        QualifierMetadata.IAttributeExtractor qualifierExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(
            ChildInjectorAttributeMetadata.Extractor.Instance,
            QualifierMetadata.AttributeExtractor.Instance);

        public bool CanExtract(IMethodSymbol providerMethodSymbol) {
            return VerifyExtract(providerMethodSymbol, null);
        }

        public InjectorProviderMetadata Extract(
            TypeModel injectorInterfaceType,
            IMethodSymbol providerMethodSymbol,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting injector provider {providerMethodSymbol}",
                providerMethodSymbol,
                currentCtx => {
                    VerifyExtract(providerMethodSymbol, currentCtx);

                    var qualifier = qualifierExtractor.Extract(providerMethodSymbol, currentCtx);
                    var returnType = providerMethodSymbol.ReturnType.ToQualifiedTypeModel(qualifier);
                    var providerMethodName = providerMethodSymbol.Name;

                    return new InjectorProviderMetadata(
                        injectorInterfaceType,
                        returnType,
                        providerMethodName,
                        providerMethodSymbol);
                });
        }

        private bool VerifyExtract(IMethodSymbol providerMethodSymbol, IGeneratorContext? currentCtx) {
            if (childInjectorAttributeExtractor.CanExtract(providerMethodSymbol)) {
                return currentCtx == null
                    ? false
                    : throw Diagnostics.InternalError.AsException(
                        "Cannot extract injector provider from a child injector factory method.",
                        providerMethodSymbol.GetLocationOrDefault(),
                        currentCtx);
            }

            if (providerMethodSymbol.ReturnsVoid) {
                return currentCtx == null
                    ? false
                    : throw Diagnostics.InternalError.AsException(
                        $"Injector provider {providerMethodSymbol.Name} must have a return type.",
                        providerMethodSymbol.GetLocationOrDefault(),
                        currentCtx);
            }

            if (currentCtx != null) {
                if (providerMethodSymbol is not {
                        DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                    }
                ) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Injector provider {providerMethodSymbol.Name} must be a public or internal method.",
                        providerMethodSymbol.GetLocationOrDefault(),
                        currentCtx);
                }

                if (providerMethodSymbol.Parameters.Length > 0) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Injector provider {providerMethodSymbol.Name} must not have any parameters.",
                        providerMethodSymbol.GetLocationOrDefault(),
                        currentCtx);
                }
            }

            return true;
        }
    }
}
