// -----------------------------------------------------------------------------
//  <copyright file="InjectorTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Templates {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Definitions;

    internal record InjectorTemplate(
            string InjectorClassName,
            string InjectorInterfaceQualifiedName,
            SpecContainerCollectionTemplate SpecContainerCollectionTemplate,
            string SpecContainerCollectionClassName,
            string SpecContainerCollectionReferenceName,
            IEnumerable<IInjectorMemberTemplate> InjectorMemberTemplates,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"internal partial class {InjectorClassName} : {InjectorInterfaceQualifiedName} {{")
                    .IncreaseIndent(1);
            SpecContainerCollectionTemplate.Render(writer);

            writer.AppendBlankLine()
                    .AppendLine($"private readonly {SpecContainerCollectionClassName} {SpecContainerCollectionReferenceName} = new {SpecContainerCollectionClassName}();");

            foreach (var memberTemplate in InjectorMemberTemplates) {
                writer.AppendBlankLine();
                memberTemplate.Render(writer);
            }

            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }

        public class Builder {
            private CreateSpecContainerCollectionTemplate createSpecContainerCollectionTemplate;
            private CreateInjectorProviderMethodTemplate createInjectorProviderMethodTemplate;
            private CreateInjectorBuilderMethodTemplate createInjectorBuilderMethodTemplate;

            public Builder(
                    CreateSpecContainerCollectionTemplate createSpecContainerCollectionTemplate,
                    CreateInjectorProviderMethodTemplate createInjectorProviderMethodTemplate,
                    CreateInjectorBuilderMethodTemplate createInjectorBuilderMethodTemplate
            ) {
                this.createSpecContainerCollectionTemplate = createSpecContainerCollectionTemplate;
                this.createInjectorProviderMethodTemplate = createInjectorProviderMethodTemplate;
                this.createInjectorBuilderMethodTemplate = createInjectorBuilderMethodTemplate;
            }

            public InjectorTemplate Build(InjectorDefinition injectorDefinition) {
                var specContainerCollectionReferenceName = "specContainers";
                var injectorMemberTemplates = new List<IInjectorMemberTemplate>();
                foreach (var provider in injectorDefinition.ProviderMethods) {
                    injectorMemberTemplates.Add(createInjectorProviderMethodTemplate(provider, specContainerCollectionReferenceName));
                }

                foreach (var builder in injectorDefinition.BuilderMethods) {
                    injectorMemberTemplates.Add(createInjectorBuilderMethodTemplate(builder, specContainerCollectionReferenceName));
                }

                return new InjectorTemplate(
                        injectorDefinition.InjectorType.TypeName,
                        injectorDefinition.InjectorInterfaceType.QualifiedName,
                        createSpecContainerCollectionTemplate(injectorDefinition.SpecContainerCollection),
                        injectorDefinition.SpecContainerCollection.SpecContainerCollectionType.TypeName,
                        specContainerCollectionReferenceName,
                        injectorMemberTemplates,
                        injectorDefinition.Location);
            }
        }
    }
}
