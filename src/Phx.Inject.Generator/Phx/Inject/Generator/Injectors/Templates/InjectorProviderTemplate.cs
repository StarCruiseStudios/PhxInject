// -----------------------------------------------------------------------------
//  <copyright file="InjectorProviderTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Injectors.Templates {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common.Templates;
    using Phx.Inject.Generator.Specifications.Templates;

    internal record InjectorProviderTemplate(
            string ReturnTypeQualifiedName,
            string MethodName,
            SpecContainerFactoryInvocationTemplate FactoryInvocationTemplate,
            Location Location
    ) : IInjectorMemberTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"public {ReturnTypeQualifiedName} {MethodName}() {{")
                    .IncreaseIndent(1)
                    .Append("return ");
            FactoryInvocationTemplate.Render(writer);
            writer.AppendLine(";")
                    .DecreaseIndent(1)
                    .AppendLine("}");
        }
    }
}
