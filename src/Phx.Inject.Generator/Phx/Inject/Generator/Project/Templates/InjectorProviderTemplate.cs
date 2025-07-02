// -----------------------------------------------------------------------------
//  <copyright file="InjectorProviderTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Generator.Project.Templates;

internal record InjectorProviderTemplate(
    string ReturnTypeQualifiedName,
    string MethodName,
    SpecContainerFactoryInvocationTemplate FactoryInvocationTemplate,
    Location Location
) : IInjectorMemberTemplate {
    public void Render(IRenderWriter writer, RenderContext context) {
        writer.AppendLine($"public {ReturnTypeQualifiedName} {MethodName}() {{")
            .IncreaseIndent(1)
            .Append("return ");
        FactoryInvocationTemplate.Render(writer, context);
        writer.AppendLine(";")
            .DecreaseIndent(1)
            .AppendLine("}");
    }
}
