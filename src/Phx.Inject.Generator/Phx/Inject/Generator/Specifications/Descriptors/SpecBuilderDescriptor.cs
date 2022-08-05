// -----------------------------------------------------------------------------
//  <copyright file="SpecBuilderDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Specifications.Descriptors {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Descriptors;

    internal delegate SpecBuilderDescriptor? CreateSpecBuilderDescriptor(
            IMethodSymbol builderMethod,
            DescriptorGenerationContext context
    );

    internal record SpecBuilderDescriptor(
            QualifiedTypeModel BuiltType,
            string BuilderMethodName,
            IEnumerable<QualifiedTypeModel> Parameters,
            Location Location
    ) : IDescriptor {
        public class Builder {
            public SpecBuilderDescriptor? Build(IMethodSymbol builderMethod, DescriptorGenerationContext context) {
                var builderAttributes = SymbolProcessors.GetBuilderAttributes(builderMethod);

                var numBuilderAttributes = builderAttributes.Count;
                if (numBuilderAttributes == 0) {
                    // This is not a builder method.
                    return null;
                }

                var builderLocation = builderMethod.Locations.First();

                if (numBuilderAttributes > 1) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            "Method can only have a single builder attribute.",
                            builderLocation);
                }

                var methodParameterTypes = SymbolProcessors.GetMethodParametersQualifiedTypes(builderMethod);
                if (methodParameterTypes.Count == 0) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            "Builder method must have at least one parameter.",
                            builderLocation);
                }

                var qualifier = SymbolProcessors.GetQualifier(builderMethod);
                // Use the qualifier from the method, not the parameter.
                var builtType = methodParameterTypes[0] with { Qualifier = qualifier };
                var builderArguments = methodParameterTypes.Count > 1
                        ? methodParameterTypes.GetRange(index: 1, methodParameterTypes.Count - 1)
                        : ImmutableList.Create<QualifiedTypeModel>();

                return new SpecBuilderDescriptor(
                        builtType,
                        builderMethod.Name,
                        builderArguments,
                        builderLocation);
            }
        }
    }
}
