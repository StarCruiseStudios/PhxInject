// -----------------------------------------------------------------------------
//  <copyright file="SpecBuilderDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Descriptors {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Input;

    internal delegate SpecBuilderDescriptor? CreateSpecBuilderDescriptor(IMethodSymbol builderMethod);

    internal record SpecBuilderDescriptor(
            QualifiedTypeDescriptor BuiltType,
            string BuilderMethodName,
            IEnumerable<QualifiedTypeDescriptor> Arguments,
            Location Location) : IDescriptor {
        public class Builder {
            public SpecBuilderDescriptor? Build(IMethodSymbol builderMethod) {
                var builderAttributes = SymbolProcessors.GetBuilderAttributes(builderMethod);

                var numBuilderAttributes = builderAttributes.Count;
                if (numBuilderAttributes == 0) {
                    // This is not a builder method.
                    return null;
                }

                if (numBuilderAttributes > 1) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            "Method can only have a single builder attribute.",
                            builderMethod.Locations.First());
                }

                var methodParameterTypes = SymbolProcessors.GetMethodParametersQualifiedTypes(builderMethod);
                if (methodParameterTypes.Count == 0) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            "Builder method must have at least one parameter.",
                            builderMethod.Locations.First());
                }

                var qualifier = SymbolProcessors.GetQualifier(builderMethod);
                // Use the qualifier from the method, not the parameter.
                var builtType = methodParameterTypes[0] with { Qualifier = qualifier };
                var builderArguments = methodParameterTypes.Count > 1
                        ? methodParameterTypes.GetRange(1, methodParameterTypes.Count - 1)
                        : ImmutableList.Create<QualifiedTypeDescriptor>();

                return new SpecBuilderDescriptor(
                        builtType,
                        builderMethod.Name,
                        builderArguments,
                        builderMethod.Locations.First());
            }
        }
    }
}
