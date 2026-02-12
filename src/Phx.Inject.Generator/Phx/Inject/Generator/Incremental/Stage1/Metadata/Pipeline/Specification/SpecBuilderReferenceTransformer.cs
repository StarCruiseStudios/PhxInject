// -----------------------------------------------------------------------------
// <copyright file="SpecBuilderReferenceTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Specification;

internal class SpecBuilderReferenceTransformer(
    ICodeElementValidator elementValidator,
    ITransformer<ISymbol, IQualifierMetadata> qualifierTransformer,
    IAttributeTransformer<BuilderReferenceAttributeMetadata> builderReferenceAttributeTransformer
) : ITransformer<ISymbol, SpecBuilderReferenceMetadata> {
    public static readonly SpecBuilderReferenceTransformer Instance = new(
        CodeElementValidator.Of(
            new FieldElementValidator(
                CodeElementAccessibility.PublicOrInternal,
                requiredAttributes: ImmutableList.Create<IAttributeChecker>(BuilderReferenceAttributeTransformer.Instance),
                prohibitedAttributes: ImmutableList.Create<IAttributeChecker>(
                    FactoryAttributeTransformer.Instance,
                    FactoryReferenceAttributeTransformer.Instance,
                    BuilderAttributeTransformer.Instance
                )
            ),
            new PropertyElementValidator(
                CodeElementAccessibility.PublicOrInternal,
                isStatic: false,
                requiredAttributes: ImmutableList.Create<IAttributeChecker>(BuilderReferenceAttributeTransformer.Instance),
                prohibitedAttributes: ImmutableList.Create<IAttributeChecker>(
                    FactoryAttributeTransformer.Instance,
                    FactoryReferenceAttributeTransformer.Instance,
                    BuilderAttributeTransformer.Instance
                )
            )
        ),
        QualifierTransformer.Instance,
        BuilderReferenceAttributeTransformer.Instance
    );

    public bool CanTransform(ISymbol symbol) {
        return elementValidator.IsValidSymbol(symbol);
    }

    public IResult<SpecBuilderReferenceMetadata> Transform(ISymbol symbol) {
        return DiagnosticsRecorder.Capture(diagnostics => {
            var (name, type) = symbol switch {
                IFieldSymbol field => (field.Name, field.Type),
                IPropertySymbol property => (property.Name, property.Type),
                _ => throw new InvalidOperationException($"Expected field or property symbol, got {symbol.GetType()}")
            };

            // Builder reference type is Action<TParam1, ..., TReturn>
            // Extract type arguments to get parameters and built type
            var typeArguments = type is INamedTypeSymbol namedType
                ? namedType.TypeArguments
                : ImmutableArray<ITypeSymbol>.Empty;

            if (typeArguments.Length == 0) {
                throw new InvalidOperationException($"Builder reference {name} must have type arguments");
            }

            // Last type argument is built type, others are parameters
            var builtTypeSymbol = typeArguments[0];
            var builtTypeQualifier = qualifierTransformer.Transform(symbol).OrThrow(diagnostics);
            var builtType = builtTypeSymbol.ToQualifiedTypeModel(builtTypeQualifier);

            var parameters = typeArguments.Skip(1)
                .Select(paramType => {
                    var paramQualifier = qualifierTransformer.Transform(paramType).OrThrow(diagnostics);
                    return paramType.ToQualifiedTypeModel(paramQualifier);
                })
                .ToEquatableList();

            var builderReferenceAttribute =
                builderReferenceAttributeTransformer.Transform(symbol).OrThrow(diagnostics);

            return new SpecBuilderReferenceMetadata(
                name,
                builtType,
                parameters,
                builderReferenceAttribute,
                symbol.GetLocationOrDefault().GeneratorIgnored()
            );
        });
    }
}
