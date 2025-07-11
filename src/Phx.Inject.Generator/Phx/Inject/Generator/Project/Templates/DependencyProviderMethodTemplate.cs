﻿// -----------------------------------------------------------------------------
//  <copyright file="DependencyProviderMethodTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Generator.Project.Templates;

internal record DependencyProviderMethodTemplate(
    string ReturnTypeQualifiedName,
    string ProviderMethodName,
    SpecContainerFactoryInvocationTemplate FactoryInvocationTemplate,
    Location Location
) : IRenderTemplate {
    public void Render(IRenderWriter writer, RenderContext renderCtx) {
        writer.AppendLine($"public {ReturnTypeQualifiedName} {ProviderMethodName}() {{")
            .IncreaseIndent(1)
            .Append("return ");
        FactoryInvocationTemplate.Render(writer, renderCtx);
        writer.AppendLine(";")
            .DecreaseIndent(1)
            .AppendLine("}");
    }
}
