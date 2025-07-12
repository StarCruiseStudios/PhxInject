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

    public interface IExtractor {
        QualifierMetadata Extract(ISymbol qualifiedSymbol, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(
            CustomQualifierAttributeMetadata.AttributeExtractor.Instance,
            LabelAttributeMetadata.Extractor.Instance);

        private readonly LabelAttributeMetadata.IExtractor labelAttributeExtractor;
        private readonly CustomQualifierAttributeMetadata.IAttributeExtractor qualifierAttributeTypeExtractor;

        internal Extractor(
            CustomQualifierAttributeMetadata.IAttributeExtractor qualifierAttributeTypeExtractor,
            LabelAttributeMetadata.IExtractor labelAttributeExtractor
        ) {
            this.qualifierAttributeTypeExtractor = qualifierAttributeTypeExtractor;
            this.labelAttributeExtractor = labelAttributeExtractor;
        }

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
                        qualifiedSymbol.Locations.First(),
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
}
