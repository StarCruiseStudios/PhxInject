// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Render.Templates {
    using System.Collections.Generic;

    internal record SpecContainerTemplate(
        string SpecContainerClassName,
        IEnumerable<InstanceHolderDeclarationTemplate> InstanceHolderDeclarations,
        IEnumerable<FactoryMethodContainerTemplate> FactoryMethodContainers
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer
                .AppendLine($"internal class {SpecContainerClassName} {{").IncreaseIndent(1);
            foreach (var instanceHolder in InstanceHolderDeclarations) {
                instanceHolder.Render(writer);
            }
            writer.AppendBlankLine();
            foreach (var factoryMethod in FactoryMethodContainers) {
                factoryMethod.Render(writer);
                writer.AppendBlankLine();
            }
            writer
                .DecreaseIndent(1).AppendLine("}");
        }
    }
}