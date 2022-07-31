// -----------------------------------------------------------------------------
//  <copyright file="InjectorBuilderMethodTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Render.Templates {
    using static RenderConstants;

    internal record InjectorBuilderMethodTemplate(
            string BuilderTypeQualifiedName,
            string MethodName,
            IRenderTemplate BuilderMethodContainerInvocation
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"public void {MethodName}({BuilderTypeQualifiedName} {BuilderMethodTargetName}) {{")
                    .IncreaseIndent(1);
            BuilderMethodContainerInvocation.Render(writer);
            writer.AppendLine(";")
                    .DecreaseIndent(1)
                    .AppendLine("}");
        }
    }
}
