// -----------------------------------------------------------------------------
//  <copyright file="InjectorProviderMethodDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Descriptors {
    using Microsoft.CodeAnalysis;

    internal delegate InjectorProviderMethodDescriptor? CreateInjectorProviderMethodDescriptor(
            IMethodSymbol providerMethod
    );

    internal record InjectorProviderMethodDescriptor(
            QualifiedTypeDescriptor ProvidedType,
            string ProviderMethodName,
            Location Location
    ) : IDescriptor {
        public class Builder {
            public InjectorProviderMethodDescriptor? Build(IMethodSymbol providerMethod) {
                return null!;
            }
        }
    }
}
