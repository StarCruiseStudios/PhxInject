// -----------------------------------------------------------------------------
//  <copyright file="ActivatorDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record ActivatorDesc(
    QualifiedTypeModel BuiltType,
    string BuilderMethodName,
    Location Location
) : IDescriptor {
    public interface IExtractor {
        ActivatorDesc? Extract(
            IMethodSymbol builderMethod,
            ExtractorContext extractorCtx
        );
    }

    public class Extractor : IExtractor {
        private readonly QualifierMetadata.IExtractor qualifierExtractor;
        public static IExtractor Instance { get; } = new Extractor(QualifierMetadata.Extractor.Instance);

        public Extractor(QualifierMetadata.IExtractor qualifierExtractor) {
            this.qualifierExtractor = qualifierExtractor;
        }

        public ActivatorDesc? Extract(
            IMethodSymbol builderMethod,
            ExtractorContext extractorCtx
        ) {
            var currentCtx = extractorCtx.GetChildContext(builderMethod);
            var builderLocation = builderMethod.Locations.First();

            if (!builderMethod.ReturnsVoid) {
                // This is a provider, not a builder.
                return null;
            }

            if (builderMethod.Parameters.Length != 1) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Injector builder {builderMethod.Name} must have exactly 1 parameter.",
                    builderLocation,
                    currentCtx);
            }

            var qualifier = qualifierExtractor.Extract(builderMethod).GetOrThrow(currentCtx);
            var builtType = builderMethod.Parameters[0].Type.ToQualifiedTypeModel(qualifier);

            return new ActivatorDesc(
                builtType,
                builderMethod.Name,
                builderLocation);
        }
    }
}
