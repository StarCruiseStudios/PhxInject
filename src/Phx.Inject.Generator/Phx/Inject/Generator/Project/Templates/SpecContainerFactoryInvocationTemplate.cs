// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerFactoryInvocationTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common;

namespace Phx.Inject.Generator.Project.Templates;

internal record SpecContainerFactoryInvocationTemplate(
    IReadOnlyList<SpecContainerFactorySingleInvocationTemplate> FactoryInvocationTemplates,
    string? multiBindQualifiedTypeArgs,
    string? runtimeFactoryProvidedTypeQualifiedName,
    Location Location
) : IRenderTemplate {
    public void Render(IRenderWriter writer) {
        if (runtimeFactoryProvidedTypeQualifiedName != null) {
            writer.Append($"new {TypeHelpers.FactoryTypeName}<{runtimeFactoryProvidedTypeQualifiedName}>(() => ");
        }

        if (FactoryInvocationTemplates.Count == 1) {
            FactoryInvocationTemplates[0].Render(writer);
        } else {
            writer.Append($"{TypeHelpers.InjectionUtilTypeName}.Combine<{multiBindQualifiedTypeArgs}> (");
            writer.IncreaseIndent(1);
            writer.AppendLine();
            var isFirst = true;
            foreach (var factoryInvocationTemplate in FactoryInvocationTemplates) {
                if (!isFirst) {
                    writer.Append(",");
                    writer.AppendLine();
                }

                isFirst = false;
                factoryInvocationTemplate.Render(writer);
            }

            writer.DecreaseIndent(1);
            writer.AppendLine();
            writer.Append(")");
        }

        if (runtimeFactoryProvidedTypeQualifiedName != null) {
            writer.Append(")");
        }
    }
}
