// -----------------------------------------------------------------------------
// <copyright file="SpecFactoryRequiredPropertyMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record AutoFactoryRequiredPropertyMetadata(
    QualifiedTypeModel PropertyType,
    string PropertyName,
    IPropertySymbol PropertySymbol
) : IMetadata {
    public Location Location {
        get => PropertySymbol.GetLocationOrDefault();
    }

    public interface IExtractor {
        bool CanExtract(IPropertySymbol propertySymbol);
        AutoFactoryRequiredPropertyMetadata Extract(
            IPropertySymbol propertySymbol,
            ExtractorContext parentCtx
        );
    }

    public class Extractor(QualifierMetadata.IAttributeExtractor qualifierExtractor) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(QualifierMetadata.AttributeExtractor.Instance);
        public bool CanExtract(IPropertySymbol propertySymbol) {
            return VerifyExtract(propertySymbol, null);
        }

        public AutoFactoryRequiredPropertyMetadata Extract(
            IPropertySymbol propertySymbol,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting auto factory required property {propertySymbol}",
                propertySymbol,
                currentCtx => {
                    VerifyExtract(propertySymbol, currentCtx);

                    var qualifier = qualifierExtractor.Extract(propertySymbol, currentCtx);
                    var propertyType = propertySymbol.Type.ToQualifiedTypeModel(qualifier);

                    return new AutoFactoryRequiredPropertyMetadata(propertyType, propertySymbol.Name, propertySymbol);
                });
        }

        private bool VerifyExtract(IPropertySymbol propertySymbol, ExtractorContext? currentCtx) {
            if (!propertySymbol.IsRequired) {
                return currentCtx == null
                    ? false
                    : throw Diagnostics.InternalError.AsException(
                        $"Required property {propertySymbol} must be a required property.",
                        propertySymbol.GetLocationOrDefault(),
                        currentCtx);
            }

            if (currentCtx != null) {
                if (propertySymbol is
                    not { SetMethod.DeclaredAccessibility: Accessibility.Public or Accessibility.Internal }
                ) {
                    throw Diagnostics.InternalError.AsException(
                        $"Required property {propertySymbol} must be public or internal.",
                        propertySymbol.GetLocationOrDefault(),
                        currentCtx);
                }
            }

            return true;
        }
    }
}
