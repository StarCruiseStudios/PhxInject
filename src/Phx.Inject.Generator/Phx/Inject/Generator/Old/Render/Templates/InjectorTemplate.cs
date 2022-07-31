// -----------------------------------------------------------------------------
//  <copyright file="InjectorTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Render.Templates {
    using System.Collections.Generic;
    using static Construct.GenerationConstants;
    using static RenderConstants;

    internal record InjectorTemplate(
            string InjectorClassName,
            string InjectorInterfaceQualifiedName,
            SpecContainerCollectionInterfaceTemplate SpecContainerCollectionInterfaceTemplate,
            SpecContainerCollectionImplementationTemplate SpecContainerCollectionImplementationTemplate,
            IEnumerable<InjectorMethodTemplate> InjectorMethods,
            IEnumerable<InjectorBuilderMethodTemplate> InjectorBuilderMethods
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"internal partial class {InjectorClassName} : {InjectorInterfaceQualifiedName} {{")
                    .IncreaseIndent(1);
            SpecContainerCollectionInterfaceTemplate.Render(writer);
            writer.AppendBlankLine();
            SpecContainerCollectionImplementationTemplate.Render(writer);

            writer
                    .AppendBlankLine()
                    .AppendLine(
                            $"private readonly {SpecContainerCollectionInterfaceName} {SpecContainersMemberName} = new {SpecContainerCollectionClassName}();");
            foreach (var injectorMethod in InjectorMethods) {
                writer.AppendBlankLine();
                injectorMethod.Render(writer);
            }

            foreach (var injectorBuilderMethod in InjectorBuilderMethods) {
                writer.AppendBlankLine();
                injectorBuilderMethod.Render(writer);
            }

            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }
    }
}
