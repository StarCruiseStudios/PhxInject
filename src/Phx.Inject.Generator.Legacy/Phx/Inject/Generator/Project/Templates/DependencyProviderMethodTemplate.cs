// -----------------------------------------------------------------------------
// <copyright file="DependencyProviderMethodTemplate.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;

namespace Phx.Inject.Generator.Project.Templates;

internal record DependencyProviderMethodTemplate(
    string ReturnTypeQualifiedName,
    string ProviderMethodName,
    DependencyProviderMemberType ProviderMemberType,
    SpecContainerFactoryInvocationTemplate FactoryInvocationTemplate,
    Location Location
) : IRenderTemplate {
    public void Render(IRenderWriter writer, RenderContext renderCtx) {
        writer.Append($"public {ReturnTypeQualifiedName} {ProviderMethodName}");
        switch (ProviderMemberType) {
            case DependencyProviderMemberType.Method:
                writer.AppendLine("() {")
                    .IncreaseIndent(1)
                    .Append("return ");
                FactoryInvocationTemplate.Render(writer, renderCtx);
                writer.AppendLine(";")
                    .DecreaseIndent(1)
                    .AppendLine("}");
                break;
            case DependencyProviderMemberType.Property:
                writer.Append("{")
                    .IncreaseIndent(1)
                    .AppendLine()
                    .Append("get => ")
                    .IncreaseIndent(1);
                FactoryInvocationTemplate.Render(writer, renderCtx);
                writer.AppendLine(";")
                    .DecreaseIndent(2)
                    .AppendLine("}");
                break;
            default:
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Unhandled Dependency Provider Member Type {ProviderMemberType}.",
                    Location,
                    renderCtx);
        }
    }
}
