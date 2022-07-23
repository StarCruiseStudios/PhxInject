// -----------------------------------------------------------------------------
//  <copyright file="FactoryMethodContainerTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Render.Templates {
    using System.Collections.Generic;
    using System.Linq;
    using static Phx.Inject.Generator.Render.RenderConstants;

    internal record FactoryMethodContainerTemplate(
        string ReturnTypeQualifiedName,
        string FactoryMethodName,
        string SpecContainerCollectionQualifiedName,
        string? InstanceHolderReference,
        string SpecificationQualifiedName,
        IEnumerable<FactoryMethodContainerInvocationTemplate> Arguments
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append($"internal {ReturnTypeQualifiedName} {FactoryMethodName}(")
                    .AppendLine($"{SpecContainerCollectionQualifiedName} {SpecContainersArgumentName}) {{").IncreaseIndent(1)
                .Append("return ");
            if (InstanceHolderReference is string instanceHolderReference) {
                writer.Append($"{instanceHolderReference} ??= ");
            }
            writer.Append($"{SpecificationQualifiedName}.{FactoryMethodName}");
            var numArguments = Arguments.Count();
            if (numArguments == 0) {
                writer.AppendLine("();");
            } else {
                writer.AppendLine("(").IncreaseIndent(1);
                foreach (var (argument, index) in Arguments.Select((a, i) => (a, i))) {
                    argument.Render(writer);
                    if (index < numArguments - 1) {
                        writer.AppendLine(",");
                    } else {
                        writer.DecreaseIndent(1).AppendLine(");");
                    }
                }
            }
            writer.DecreaseIndent(1).AppendLine("}");
        }
    }
}
