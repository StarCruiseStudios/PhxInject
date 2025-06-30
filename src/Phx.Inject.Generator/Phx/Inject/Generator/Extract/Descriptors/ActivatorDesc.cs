// -----------------------------------------------------------------------------
//  <copyright file="ActivatorDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Model;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record ActivatorDesc(
    QualifiedTypeModel BuiltType,
    string BuilderMethodName,
    Location Location
) : IDescriptor {
    public interface IExtractor {
        ActivatorDesc? Extract(
            IMethodSymbol builderMethod,
            DescGenerationContext context
        );
    }

    public class Extractor : IExtractor {
        public ActivatorDesc? Extract(
            IMethodSymbol builderMethod,
            DescGenerationContext context
        ) {
            var builderLocation = builderMethod.Locations.First();

            if (!builderMethod.ReturnsVoid) {
                // This is a provider, not a builder.
                return null;
            }

            if (builderMethod.Parameters.Length != 1) {
                throw new InjectionException(
                    Diagnostics.InvalidSpecification,
                    $"Injector builder {builderMethod.Name} must have exactly 1 parameter.",
                    builderLocation);
            }

            var builtType = TypeModel.FromTypeSymbol(builderMethod.Parameters[0].Type);
            var qualifier = MetadataHelpers.GetQualifier(builderMethod);
            return new ActivatorDesc(
                new QualifiedTypeModel(builtType, qualifier),
                builderMethod.Name,
                builderLocation);
        }
    }
}
