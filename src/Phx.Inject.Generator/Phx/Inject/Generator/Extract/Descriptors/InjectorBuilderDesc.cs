// -----------------------------------------------------------------------------
//  <copyright file="InjectorBuilderDesc.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Metadata;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record InjectorBuilderDesc(
    QualifiedTypeModel BuiltType,
    string BuilderMethodName,
    Location Location
) : IDescriptor {
    public interface IExtractor {
        InjectorBuilderDesc? Extract(
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

        public InjectorBuilderDesc? Extract(
            IMethodSymbol builderMethod,
            ExtractorContext extractorCtx
        ) {
            return extractorCtx.UseChildContext(
                "extracting injector builder",
                builderMethod,
                currentCtx => {
                    var builderLocation = builderMethod.GetLocationOrDefault();

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

                    var qualifier = qualifierExtractor.Extract(builderMethod, currentCtx);
                    var builtType = builderMethod.Parameters[0].Type.ToQualifiedTypeModel(qualifier);

                    return new InjectorBuilderDesc(
                        builtType,
                        builderMethod.Name,
                        builderLocation);
                });
        }
    }
}
