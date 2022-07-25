// -----------------------------------------------------------------------------
//  <copyright file="BuilderMethodContainerTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Render.Templates {
    using System.Collections.Generic;
    using System.Linq;
    using static Phx.Inject.Generator.Render.RenderConstants;

    internal record BuilderMethodContainerTemplate(
        string BuiltTypeQualifiedName,
        string BuilderMethodName,
        string SpecContainerCollectionQualifiedName,
        string SpecificationQualifiedName,
        IEnumerable<FactoryMethodContainerInvocationTemplate> Arguments
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append($"internal void {BuilderMethodName}({BuiltTypeQualifiedName} {BuilderMethodTargetName}, ")
                    .AppendLine($"{SpecContainerCollectionQualifiedName} {SpecContainersArgumentName}) {{").IncreaseIndent(1);
            writer.Append($"{SpecificationQualifiedName}.{BuilderMethodName}");
            var numArguments = Arguments.Count();
            writer.AppendLine("(").IncreaseIndent(1);
            writer.Append(BuilderMethodTargetName);
            if (numArguments == 0) {
                writer.AppendLine(");").DecreaseIndent(1);
            } else {
                foreach (var (argument, index) in Arguments.Select((a, i) => (a, i))) {
                    writer.AppendLine(",");
                    argument.Render(writer);
                    if (index >= numArguments - 1) {
                        writer.AppendLine(");").DecreaseIndent(1);
                    }
                }
            }
            writer.DecreaseIndent(1).AppendLine("}");
        }
    }
}
