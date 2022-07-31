// -----------------------------------------------------------------------------
//  <copyright file="InjectorBuilderDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Descriptors {
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Input;

    internal delegate InjectorBuilderDescriptor? CreateInjectorBuilderDescriptor(
            IMethodSymbol builderMethod
    );

    internal record InjectorBuilderDescriptor(
            QualifiedTypeDescriptor BuiltType,
            string BuilderMethodName,
            Location Location
    ) : IDescriptor {
        public class Builder {
            public InjectorBuilderDescriptor? Build(IMethodSymbol builderMethod) {
                if (!builderMethod.ReturnsVoid) {
                    // This is a provider not a builder.
                    return null;
                }

                if (builderMethod.Parameters.Length != 1) {
                    // I don't know what this is, but it's not a builder.
                    return null;
                }

                var builtType = TypeModel.FromTypeSymbol(builderMethod.Parameters[0].Type);
                var qualifier = SymbolProcessors.GetQualifier(builderMethod);
                return new InjectorBuilderDescriptor(
                        new QualifiedTypeDescriptor(
                                builtType,
                                qualifier,
                                builderMethod.Parameters[0].Locations.Single()),
                        builderMethod.Name,
                        builderMethod.Locations.Single());
            }
        }
    }
}
