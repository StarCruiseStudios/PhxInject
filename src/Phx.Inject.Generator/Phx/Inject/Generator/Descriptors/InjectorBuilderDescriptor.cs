// -----------------------------------------------------------------------------
//  <copyright file="InjectorBuilderDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Descriptors {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Model;

    internal delegate InjectorBuilderDescriptor? CreateInjectorBuilderDescriptor(
        IMethodSymbol builderMethod,
        DescriptorGenerationContext context
    );

    internal record InjectorBuilderDescriptor(
        QualifiedTypeModel BuiltType,
        string BuilderMethodName,
        Location Location
    ) : IDescriptor {
        public class Builder {
            public InjectorBuilderDescriptor? Build(
                IMethodSymbol builderMethod,
                DescriptorGenerationContext context
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
                return new InjectorBuilderDescriptor(
                    new QualifiedTypeModel(builtType, qualifier),
                    builderMethod.Name,
                    builderLocation);
            }
        }
    }
}
