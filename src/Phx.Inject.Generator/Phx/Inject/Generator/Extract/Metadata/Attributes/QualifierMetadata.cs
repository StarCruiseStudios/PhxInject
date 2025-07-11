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

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal class QualifierMetadata : IDescriptor {
    public static QualifierMetadata NoQualifier { get; } = new(Common.Model.NoQualifier.Instance, Location.None);
    public AttributeMetadata? Attribute { get; }
    public IQualifier Qualifier { get; }
    public QualifierMetadata(AttributeMetadata attribute) {
        Attribute = attribute;
        Location = attribute.Location;

        if (attribute is LabelAttributeMetadata labelAttribute) {
            Qualifier = new LabelQualifier(labelAttribute.Label);
        } else if (attribute is QualifierAttributeMetadata qualifierAttribute) {
            Qualifier = new AttributeQualifier(qualifierAttribute);
        } else {
            throw new InvalidOperationException(
                $"Unexpected attribute type {attribute.GetType().Name} for qualifier.");
        }
    }

    public QualifierMetadata(IQualifier qualifier, Location location) {
        Qualifier = qualifier;
        Location = location;
        Attribute = null;
    }
    public Location Location { get; }

    public interface IExtractor {
        IResult<QualifierMetadata> Extract(ISymbol qualifiedSymbol);
    }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(
            QualifierAttributeMetadata.Extractor.Instance,
            LabelAttributeMetadata.Extractor.Instance);

        private readonly LabelAttributeMetadata.IExtractor labelAttributeExtractor;

        private readonly QualifierAttributeMetadata.IExtractor qualifierAttributeExtractor;

        internal Extractor(
            QualifierAttributeMetadata.IExtractor qualifierAttributeExtractor,
            LabelAttributeMetadata.IExtractor labelAttributeExtractor
        ) {
            this.qualifierAttributeExtractor = qualifierAttributeExtractor;
            this.labelAttributeExtractor = labelAttributeExtractor;
        }

        public IResult<QualifierMetadata> Extract(ISymbol qualifiedSymbol) {
            var labelAttributeResult = labelAttributeExtractor.CanExtract(qualifiedSymbol)
                ? labelAttributeExtractor.Extract(qualifiedSymbol)
                : null;
            if (labelAttributeResult is not null && !labelAttributeResult.IsOk) {
                return labelAttributeResult.MapError<QualifierMetadata>();
            }

            var qualifierAttributeResult = qualifierAttributeExtractor.CanExtract(qualifiedSymbol)
                ? qualifierAttributeExtractor.Extract(qualifiedSymbol)
                : null;
            if (qualifierAttributeResult is not null && !qualifierAttributeResult.IsOk) {
                return qualifierAttributeResult.MapError<QualifierMetadata>();
            }

            var labelAttribute = labelAttributeResult?.GetValue();
            var qualifierAttribute = qualifierAttributeResult?.GetValue();

            if (labelAttribute != null) {
                if (qualifierAttribute != null) {
                    return Result.Error<QualifierMetadata>(
                        $"Symbol {qualifiedSymbol.Name} can only have one Label or Qualifier attribute.",
                        qualifiedSymbol.Locations.First(),
                        Diagnostics.InvalidSpecification);
                }

                return Result.Ok(new QualifierMetadata(labelAttribute));
            }

            if (qualifierAttribute != null) {
                return Result.Ok(new QualifierMetadata(qualifierAttribute));
            }

            return Result.Ok(NoQualifier);
        }
    }
}
