// -----------------------------------------------------------------------------
// <copyright file="InjectorChildFactoryMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record InjectorChildFactoryMetadata(
    TypeModel InjectorInterfaceType,
    TypeModel ChildInjectorType,
    string InjectorChildFactoryMethodName,
    IReadOnlyList<TypeModel> Parameters,
    ChildInjectorAttributeMetadata Attribute,
    IMethodSymbol InjectorChildFactoryMethodSymbol
) : IMetadata {
    public Location Location {
        get => InjectorChildFactoryMethodSymbol.GetLocationOrDefault();
    }

    public interface IExtractor {
        bool CanExtract(IMethodSymbol childInjectorMethod);
        InjectorChildFactoryMetadata Extract(
            TypeModel InjectorInterfaceType,
            IMethodSymbol childInjectorMethod,
            ExtractorContext parentCtx
        );
    }

    public class Extractor(
        ChildInjectorAttributeMetadata.IExtractor childInjectorAttributeExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(ChildInjectorAttributeMetadata.Extractor.Instance);

        public bool CanExtract(IMethodSymbol childInjectorMethod) {
            return VerifyExtract(childInjectorMethod, null);
        }

        public InjectorChildFactoryMetadata Extract(
            TypeModel InjectorInterfaceType,
            IMethodSymbol childInjectorMethod,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildExtractorContext(
                $"extracting injector child factory {childInjectorMethod}",
                childInjectorMethod,
                currentCtx => {
                    VerifyExtract(childInjectorMethod, currentCtx);

                    var childInjectorAttribute =
                        childInjectorAttributeExtractor.Extract(childInjectorMethod, currentCtx);
                    IReadOnlyList<TypeModel> parameters = childInjectorMethod.Parameters
                        .Select(parameter => parameter.Type.ToTypeModel())
                        .ToImmutableList();
                    var returnType = childInjectorMethod.ReturnType.ToTypeModel();
                    var childInjectorMethodName = childInjectorMethod.Name;

                    return new InjectorChildFactoryMetadata(
                        InjectorInterfaceType,
                        returnType,
                        childInjectorMethodName,
                        parameters,
                        childInjectorAttribute,
                        childInjectorMethod);
                });
        }

        private bool VerifyExtract(
            IMethodSymbol childInjectorMethod,
            IGeneratorContext? currentCtx
        ) {
            if (!childInjectorAttributeExtractor.CanExtract(childInjectorMethod)) {
                return currentCtx == null
                    ? false
                    : throw Diagnostics.InternalError.AsException(
                        $"Child injector factory must declare a {ChildInjectorAttributeMetadata.ChildInjectorAttributeClassName}.",
                        childInjectorMethod.GetLocationOrDefault(),
                        currentCtx);
            }

            if (currentCtx != null) {
                if (childInjectorMethod is not {
                        ReturnsVoid: false,
                        DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                    }
                ) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Child injector factory {childInjectorMethod.Name} must be a public or internal method with a return type.",
                        childInjectorMethod.GetLocationOrDefault(),
                        currentCtx);
                }
            }

            return true;
        }
    }
}
