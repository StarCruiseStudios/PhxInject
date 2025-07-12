// -----------------------------------------------------------------------------
//  <copyright file="InjectorChildFactoryDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record InjectorChildFactoryDesc(
    TypeModel ChildInjectorType,
    string InjectorChildFactoryMethodName,
    IReadOnlyList<TypeModel> Parameters,
    Location Location
) : IDescriptor {
    public interface IExtractor {
        bool IsInjectorChildFactory(IMethodSymbol childInjectorMethod);
        InjectorChildFactoryDesc Extract(
            IMethodSymbol childInjectorMethod,
            ExtractorContext extractorCtx
        );
    }

    public class Extractor : IExtractor {
        private readonly ChildInjectorAttributeMetadata.IExtractor childInjectorAttributeExtractor;
        public Extractor(
            ChildInjectorAttributeMetadata.IExtractor childInjectorAttributeExtractor
        ) {
            this.childInjectorAttributeExtractor = childInjectorAttributeExtractor;
        }

        public Extractor() : this(
            ChildInjectorAttributeMetadata.Extractor.Instance
        ) { }

        public bool IsInjectorChildFactory(IMethodSymbol childInjectorMethod) {
            return childInjectorAttributeExtractor.CanExtract(childInjectorMethod);
        }

        public InjectorChildFactoryDesc Extract(
            IMethodSymbol childInjectorMethod,
            ExtractorContext extractorCtx
        ) {
            return extractorCtx.UseChildContext(childInjectorMethod,
                currentCtx => {
                    var childInjectorLocation = childInjectorMethod.Locations.First();

                    if (!childInjectorAttributeExtractor.CanExtract(childInjectorMethod)) {
                        throw Diagnostics.InvalidSpecification.AsException(
                            $"Type {childInjectorMethod} must declare an {ChildInjectorAttributeMetadata.ChildInjectorAttributeClassName}.",
                            childInjectorMethod.Locations.First(),
                            currentCtx);
                    }

                    var childInjectorAttribute =
                        childInjectorAttributeExtractor.Extract(childInjectorMethod, currentCtx);

                    IReadOnlyList<TypeModel> parameters = childInjectorMethod.Parameters
                        .Select(parameter => TypeModel.FromTypeSymbol(parameter.Type))
                        .ToImmutableList();

                    var returnType = TypeModel.FromTypeSymbol(childInjectorMethod.ReturnType);
                    return new InjectorChildFactoryDesc(
                        returnType,
                        childInjectorMethod.Name,
                        parameters,
                        childInjectorLocation);
                });
        }
    }
}
