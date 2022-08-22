﻿// -----------------------------------------------------------------------------
//  <copyright file="InjectorChildFactoryDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Injectors.Descriptors {
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Descriptors;

    internal delegate InjectorChildFactoryDescriptor? CreateInjectorChildFactoryDescriptor(
            IMethodSymbol childInjectorMethod,
            DescriptorGenerationContext context
    );

    internal record InjectorChildFactoryDescriptor(
            TypeModel ChildInjectorType,
            string InjectorChildFactoryMethodName,
            Location Location
    ) : IDescriptor {
        public class Builder {
            public InjectorChildFactoryDescriptor? Build(
                    IMethodSymbol childInjectorMethod,
                    DescriptorGenerationContext context
            ) {
                var childInjectorLocation = childInjectorMethod.Locations.First();

                if (!childInjectorMethod.GetChildInjectorAttributes().Any()) {
                    // This is not an injector child factory.
                    return null;
                }

                if (childInjectorMethod.ReturnsVoid) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            $"Injector child factory {childInjectorMethod.Name} must return a type.",
                            childInjectorLocation);
                }

                var returnType = TypeModel.FromTypeSymbol(childInjectorMethod.ReturnType);
                return new InjectorChildFactoryDescriptor(
                        returnType,
                        childInjectorMethod.Name,
                        childInjectorLocation);
            }
        }
    }
}
