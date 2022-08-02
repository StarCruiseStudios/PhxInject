// -----------------------------------------------------------------------------
//  <copyright file="ChildInjectorDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Descriptors {
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal delegate ChildInjectorDescriptor CreateChildInjectorDescriptor(IMethodSymbol childInjectorMethod);

    internal record ChildInjectorDescriptor(
        TypeModel ChildInjectorType,
        string ChildInjectorMethodName,
        Location Location
    ) : IDescriptor {
        public class Builder {
            public ChildInjectorDescriptor Build(IMethodSymbol childInjectorMethod) {
                var childInjectorLocation = childInjectorMethod.Locations.First();

                if (childInjectorMethod.ReturnsVoid) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            $"Child Injector factory {childInjectorMethod.Name} must return a type.",
                            childInjectorLocation);
                }

                if (childInjectorMethod.Parameters.Length > 0) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            $"Child injector factory {childInjectorMethod.Name} must not have any parameters.",
                            childInjectorLocation);
                }

                var returnType = TypeModel.FromTypeSymbol(childInjectorMethod.ReturnType);
                return new ChildInjectorDescriptor(
                        returnType,
                        childInjectorMethod.Name,
                        childInjectorLocation);
            }
        }
    }
}
