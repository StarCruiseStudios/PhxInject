// -----------------------------------------------------------------------------
// <copyright file="InjectionFrame.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Text;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;

namespace Phx.Inject.Common.Exceptions;

internal interface IInjectionFrame {
    ISymbol? Symbol { get; }
    IInjectionFrame? Parent { get; }
}

internal static class IInjectionFrameExtensions {
    public static string GetInjectionFrameStack(this IInjectionFrame? frame) {
        var sb = new StringBuilder();
        var injectionFrame = frame;
        while (injectionFrame is { Symbol: not null }) {
            sb.AppendLine()
                .Append($"|   at {injectionFrame}");
            injectionFrame = injectionFrame.Parent;
        }

        return sb.ToString();
    }
}

internal class InjectionFrame : IInjectionFrame {
    public InjectionFrame(ISymbol? symbol, IInjectionFrame? parent) {
        Symbol = symbol;
        Parent = parent;
    }
    public ISymbol? Symbol { get; }
    public IInjectionFrame? Parent { get; }

    public override string ToString() {
        return Symbol?.Let(it => {
                var location = it.Locations.First();
                var typeName = it.ContainingNamespace + "." + it.Name;

                return
                    $"{Path.GetFileName(location.GetLineSpan().Path)}({location.GetLineSpan().StartLinePosition}) [{typeName}]";
            })
            ?? "[Injection]";
    }
}
