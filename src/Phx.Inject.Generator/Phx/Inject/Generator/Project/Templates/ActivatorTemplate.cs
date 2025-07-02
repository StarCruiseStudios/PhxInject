// -----------------------------------------------------------------------------
//  <copyright file="ActivatorTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Generator.Project.Templates;

internal record ActivatorTemplate(
    string BuiltTypeQualifiedName,
    string MethodName,
    string BuilderTargetName,
    SpecContainerBuilderInvocationTemplate SpecContainerBuilderInvocation,
    Location Location
) : IInjectorMemberTemplate {
    public void Render(IRenderWriter writer, RenderContext context) {
        writer.AppendLine($"public void {MethodName}({BuiltTypeQualifiedName} {BuilderTargetName}) {{")
            .IncreaseIndent(1);
        SpecContainerBuilderInvocation.Render(writer, context);
        writer.AppendLine(";")
            .DecreaseIndent(1)
            .AppendLine("}");
    }
}
