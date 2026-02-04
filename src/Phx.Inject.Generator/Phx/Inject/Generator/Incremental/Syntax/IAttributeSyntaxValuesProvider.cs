// -----------------------------------------------------------------------------
// <copyright file="IAttributeSyntaxValuesProvider.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Syntax;

internal interface IAttributeSyntaxValuesProvider<out T> where T : ISourceCodeElement {
    string AttributeClassName { get; }
    bool CanProvide(SyntaxNode syntaxNode, CancellationToken cancellationToken);
    T Transform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken);
}

internal static class AttributeSyntaxProviderExtensions {
    public static IncrementalValuesProvider<T> ForAttribute<T>(
        this SyntaxValueProvider syntaxProvider,
        IAttributeSyntaxValuesProvider<T> syntaxValuesProvider
    ) where T : ISourceCodeElement {
        return syntaxProvider.ForAttributeWithMetadataName(
            syntaxValuesProvider.AttributeClassName,
            syntaxValuesProvider.CanProvide,
            syntaxValuesProvider.Transform);
    }
}
