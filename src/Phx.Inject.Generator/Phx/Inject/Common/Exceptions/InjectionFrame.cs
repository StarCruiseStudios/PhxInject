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
    string? Description { get; }
    ISymbol? Symbol { get; }
    IInjectionFrame? Parent { get; }
}

internal static class IInjectionFrameExtensions {
    public static string GetInjectionFrameStack(this IInjectionFrame? frame) {
        var sb = new StringBuilder();
        var injectionFrame = frame;
        while (injectionFrame != null) {
            if (injectionFrame.Description != null) {
                sb.AppendLine()
                    .Append($"|   while {injectionFrame}");
            }
            
            injectionFrame = injectionFrame.Parent;
        }

        return sb.ToString();
    }
}

internal record InjectionFrame(
    string? Description,
    ISymbol? Symbol,
    IInjectionFrame? Parent
) : IInjectionFrame {
    public override string ToString() {
        return Symbol?.Let(it => {
                var location = it.Locations.First();
                var typeName = it.ContainingNamespace + "." + it.Name;

                return
                    $"{Description} for {typeName} {Path.GetFileName(location.GetLineSpan().Path)}({location.GetLineSpan().StartLinePosition})";
            })
            ?? Description ?? "unknown injection frame";
    }
}
