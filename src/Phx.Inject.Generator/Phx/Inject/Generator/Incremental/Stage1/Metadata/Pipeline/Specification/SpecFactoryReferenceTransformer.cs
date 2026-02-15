// -----------------------------------------------------------------------------
// <copyright file="SpecFactoryReferenceTransformer.cs" company="Star Cruise Studios LLC">
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

/// <summary>
/// Transforms specification factory references into metadata.
/// </summary>
internal sealed class SpecFactoryReferenceTransformer(
    ICodeElementValidator elementValidator,
    ITransformer<ISymbol, IQualifierMetadata> qualifierTransformer,
    IAttributeTransformer<FactoryReferenceAttributeMetadata> factoryReferenceAttributeTransformer,
    IAttributeTransformer<PartialAttributeMetadata> partialAttributeTransformer
) : ITransformer<ISymbol, SpecFactoryReferenceMetadata> {
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly SpecFactoryReferenceTransformer Instance = new(
            CodeElementValidator.Of(
            new FieldElementValidator(
                CodeElementAccessibility.PublicOrInternal,
                requiredAttributes: ImmutableList.Create<IAttributeChecker>(FactoryReferenceAttributeTransformer.Instance),
                prohibitedAttributes: ImmutableList.Create<IAttributeChecker>(
                    FactoryAttributeTransformer.Instance,
                    BuilderAttributeTransformer.Instance,
                    BuilderReferenceAttributeTransformer.Instance
                )
            ),
            new PropertyElementValidator(
                CodeElementAccessibility.PublicOrInternal,
                requiredAttributes: ImmutableList.Create<IAttributeChecker>(FactoryReferenceAttributeTransformer.Instance),
                prohibitedAttributes: ImmutableList.Create<IAttributeChecker>(
                    FactoryAttributeTransformer.Instance,
                    BuilderAttributeTransformer.Instance,
                    BuilderReferenceAttributeTransformer.Instance
                )
            )
        ),
        QualifierTransformer.Instance,
        FactoryReferenceAttributeTransformer.Instance,
        PartialAttributeTransformer.Instance
    );

    /// <inheritdoc />
    public bool CanTransform(ISymbol symbol) {
        return elementValidator.IsValidSymbol(symbol);
    }

    /// <inheritdoc />
    public IResult<SpecFactoryReferenceMetadata> Transform(ISymbol symbol) {
        return DiagnosticsRecorder.Capture(diagnostics => {
            var (name, type) = symbol switch {
                IFieldSymbol field => (field.Name, field.Type),
                IPropertySymbol property => (property.Name, property.Type),
                _ => throw new InvalidOperationException($"Expected field or property symbol, got {symbol.GetType()}")
            };

            // Factory reference type is IFactory<TParam1, ..., TReturn>
            // Extract type arguments to get parameters and return type
            var typeArguments = type is INamedTypeSymbol namedType
                ? namedType.TypeArguments
                : ImmutableArray<ITypeSymbol>.Empty;

            if (typeArguments.Length == 0) {
                throw new InvalidOperationException($"Factory reference {name} must have type arguments");
            }

            // Last type argument is return type, others are parameters
            var returnTypeSymbol = typeArguments[typeArguments.Length - 1];
            var returnTypeQualifier = qualifierTransformer.Transform(symbol).OrThrow(diagnostics);
            var factoryReturnType = returnTypeSymbol.ToQualifiedTypeModel(returnTypeQualifier);

            var parameters = typeArguments
                .Take(typeArguments.Length - 1)
                .Select(paramType => {
                    var paramQualifier = qualifierTransformer.Transform(paramType).OrThrow(diagnostics);
                    return paramType.ToQualifiedTypeModel(paramQualifier);
                })
                .ToEquatableList();

            var factoryReferenceAttribute =
                factoryReferenceAttributeTransformer.Transform(symbol).OrThrow(diagnostics);
            var partialAttribute = partialAttributeTransformer
                .TransformOrNull(symbol)?
                .OrThrow(diagnostics);

            return new SpecFactoryReferenceMetadata(
                name,
                factoryReturnType,
                parameters,
                factoryReferenceAttribute,
                partialAttribute,
                symbol.GetLocationOrDefault().GeneratorIgnored()
            );
        });
    }
}
