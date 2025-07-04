// -----------------------------------------------------------------------------
//  <copyright file="InjectorChildFactoryDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record InjectorChildFactoryDesc(
    TypeModel ChildInjectorType,
    string InjectorChildFactoryMethodName,
    IReadOnlyList<TypeModel> Parameters,
    Location Location
) : IDescriptor {
    public interface IExtractor {
        InjectorChildFactoryDesc? Extract(
            IMethodSymbol childInjectorMethod,
            ExtractorContext context
        );
    }

    public class Extractor : IExtractor {
        public InjectorChildFactoryDesc? Extract(
            IMethodSymbol childInjectorMethod,
            ExtractorContext context
        ) {
            var childInjectorLocation = childInjectorMethod.Locations.First();

            if (childInjectorMethod.TryGetChildInjectorAttribute().GetOrThrow(context.GenerationContext) == null) {
                // This is not an injector child factory.
                return null;
            }

            if (childInjectorMethod.ReturnsVoid) {
                throw new InjectionException($"Injector child factory {childInjectorMethod.Name} must return a type.",
                    Diagnostics.InvalidSpecification,
                    childInjectorLocation,
                    context.GenerationContext);
            }

            IReadOnlyList<TypeModel> parameters = childInjectorMethod.Parameters
                .Select(parameter => TypeModel.FromTypeSymbol(parameter.Type))
                .ToImmutableList();

            var returnType = TypeModel.FromTypeSymbol(childInjectorMethod.ReturnType);
            return new InjectorChildFactoryDesc(
                returnType,
                childInjectorMethod.Name,
                parameters,
                childInjectorLocation);
        }
    }
}
