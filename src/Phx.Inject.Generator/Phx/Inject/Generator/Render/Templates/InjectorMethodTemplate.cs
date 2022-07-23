// -----------------------------------------------------------------------------
//  <copyright file="InjectorMethodTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Render.Templates {
    internal record InjectorMethodTemplate(
        string ReturnTypeQualifiedName,
        string MethodName,
        IRenderTemplate FactoryMethodContainerInvocation
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"public {ReturnTypeQualifiedName} {MethodName}() {{").IncreaseIndent(1)
                .Append("return ");
            FactoryMethodContainerInvocation.Render(writer);
            writer.AppendLine(";")
                .DecreaseIndent(1).AppendLine("}");
        }
    }
}