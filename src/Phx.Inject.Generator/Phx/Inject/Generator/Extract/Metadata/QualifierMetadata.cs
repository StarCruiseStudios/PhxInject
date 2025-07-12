// -----------------------------------------------------------------------------
// <copyright file="QualifierMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Descriptors;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Metadata;

internal class QualifierMetadata : IDescriptor {
    public static QualifierMetadata NoQualifier { get; } = new(Common.Model.NoQualifier.Instance, Location.None);
    public ITypeQualifierAttributeMetadata? Attribute { get; }
    public IQualifier Qualifier { get; }
    public QualifierMetadata(ITypeQualifierAttributeMetadata typeQualifierAttribute) {
        Attribute = typeQualifierAttribute;
        Location = typeQualifierAttribute.Location;
        Qualifier = typeQualifierAttribute.Qualifier;
    }

    public QualifierMetadata(IQualifier qualifier, Location location) {
        Qualifier = qualifier;
        Location = location;
        Attribute = null;
    }
    public Location Location { get; }

    public interface IAttributeExtractor {
        QualifierMetadata Extract(ISymbol qualifiedSymbol, IGeneratorContext generatorCtx);
    }

    public class AttributeExtractor(
        CustomQualifierAttributeMetadata.IAttributeExtractor qualifierAttributeTypeExtractor,
        LabelAttributeMetadata.IAttributeExtractor labelAttributeExtractor
    ) : IAttributeExtractor {
        public static readonly IAttributeExtractor Instance = new AttributeExtractor(
            CustomQualifierAttributeMetadata.AttributeExtractor.Instance,
            LabelAttributeMetadata.AttributeExtractor.Instance);

        public QualifierMetadata Extract(ISymbol qualifiedSymbol, IGeneratorContext generatorCtx) {
            var labelAttribute = labelAttributeExtractor.CanExtract(qualifiedSymbol)
                ? labelAttributeExtractor.Extract(qualifiedSymbol, generatorCtx)
                : null;
            var qualifierAttribute = qualifierAttributeTypeExtractor.CanExtract(qualifiedSymbol)
                ? qualifierAttributeTypeExtractor.Extract(qualifiedSymbol, generatorCtx)
                : null;

            if (labelAttribute != null) {
                if (qualifierAttribute != null) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Symbol {qualifiedSymbol.Name} can only have one Label or Qualifier attribute.",
                        qualifiedSymbol.GetLocationOrDefault(),
                        generatorCtx);
                }

                return new QualifierMetadata(labelAttribute);
            }

            if (qualifierAttribute != null) {
                return new QualifierMetadata(qualifierAttribute);
            }

            return NoQualifier;
        }
    }

    public interface ICustomQualifierTypeExtractor {
        QualifierMetadata Extract(
            ISymbol attributedSymbol,
            INamedTypeSymbol customQualifierTypeSymbol,
            IGeneratorContext generatorCtx);
    }

    public class CustomQualifierTypeExtractor(
        CustomQualifierAttributeMetadata.ITypeExtractor customQualifierAttributeTypeExtractor
    ) : ICustomQualifierTypeExtractor {
        public static readonly ICustomQualifierTypeExtractor Instance = new CustomQualifierTypeExtractor(
            CustomQualifierAttributeMetadata.TypeExtractor.Instance);

        public QualifierMetadata Extract(
            ISymbol attributedSymbol,
            INamedTypeSymbol customQualifierTypeSymbol,
            IGeneratorContext generatorCtx) {
            var customQualifierAttribute = customQualifierAttributeTypeExtractor.Extract(
                attributedSymbol,
                customQualifierTypeSymbol,
                generatorCtx);
            return new QualifierMetadata(customQualifierAttribute);
        }
    }

    public interface ILabelStringExtractor {
        QualifierMetadata Extract(string Label, AttributeMetadata attributeMetadata, IGeneratorContext generatorCtx);
    }

    public class LabelStringExtractor(
        LabelAttributeMetadata.IStringExtractor labelAttributeStringExtractor
    ) : ILabelStringExtractor {
        public static readonly ILabelStringExtractor Instance = new LabelStringExtractor(
            LabelAttributeMetadata.StringExtractor.Instance);

        public QualifierMetadata Extract(
            string Label,
            AttributeMetadata attributeMetadata,
            IGeneratorContext generatorCtx) {
            var labelAttribute = labelAttributeStringExtractor.Extract(
                Label,
                attributeMetadata,
                generatorCtx);
            return new QualifierMetadata(labelAttribute);
        }
    }
}
