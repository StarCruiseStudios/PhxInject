// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerCollectionTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Templates {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Input;
    using Phx.Inject.Generator.Model.Definitions;

    internal delegate SpecContainerCollectionTemplate CreateSpecContainerCollectionTemplate(
            SpecContainerCollectionDefinition specContainerCollectionDefinition,
            string specContainerCollectionReferenceName
    );

    internal record SpecContainerCollectionTemplate(
            string InjectorClassName,
            string SpecContainerCollectionClassName,
            string SpecContainerCollectionReferenceName,
            IEnumerable<SpecContainerCollectionPropertyDefinitionTemplate> SpecContainerProperties,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            //
            // SpecContainerCollection record definition
            //
            writer.AppendLine($"internal record {SpecContainerCollectionClassName} (")
                    .IncreaseIndent(1);
            var isFirst = true;
            foreach (var specContainerProperty in SpecContainerProperties) {
                if (isFirst) {
                    isFirst = false;
                } else {
                    writer.AppendLine(",");
                }

                specContainerProperty.Render(writer);
            }

            writer.AppendLine(");")
                    .DecreaseIndent(1)
                    .AppendBlankLine();

            //
            // SpecContainerCollection property declaration.
            //
            writer.Append(
                    $"private readonly {SpecContainerCollectionClassName} {SpecContainerCollectionReferenceName};");

            //
            // Injector constructor signature
            //

            writer.AppendBlankLine()
                    .Append($"public {InjectorClassName}(");

            var specContainersWithNonDefaultConstructors = SpecContainerProperties
                    .Where(specContainerProperty => specContainerProperty.HasDefaultConstructor == false)
                    .ToImmutableList();

            if (specContainersWithNonDefaultConstructors.Any()) {
                writer.AppendBlankLine()
                        .IncreaseIndent(1);

                var isFirstParameter = true;
                foreach (var specContainer in specContainersWithNonDefaultConstructors) {
                    if (isFirstParameter) {
                        isFirstParameter = false;
                    } else {
                        writer.AppendLine(",");
                    }

                    var specParameterName = SymbolProcessors.StartLowercase(specContainer.PropertyName);
                    writer.AppendLine($"{specContainer.QualifiedSpecificationTypeName} {specParameterName}");
                }

                writer.DecreaseIndent(1);
            }

            //
            // Injector constructor body.
            //

            writer.AppendLine(") {")
                    .IncreaseIndent(1)
                    .AppendLine($"{SpecContainerCollectionReferenceName} = new {SpecContainerCollectionClassName}(")
                    .IncreaseIndent(1);

            var isFirstArgument = true;
            foreach (var specContainer in SpecContainerProperties) {
                if (isFirstArgument) {
                    isFirstArgument = false;
                } else {
                    writer.AppendLine(",");
                }

                if (specContainer.HasDefaultConstructor) {
                    writer.Append(
                            $"{specContainer.PropertyName}: new {specContainer.QualifiedSpecContainerTypeName}()");
                } else {
                    var specParameterName = SymbolProcessors.StartLowercase(specContainer.PropertyName);
                    writer.Append(
                            $"{specContainer.PropertyName}: new {specContainer.QualifiedSpecContainerTypeName}({specParameterName})");
                }
            }

            writer.AppendLine(");")
                    .DecreaseIndent(2)
                    .AppendLine("}");
        }

        public class Builder {
            private readonly CreateSpecContainerCollectionPropertyDefinitionTemplate
                    createSpecContainerCollectionPropertyDefinition;

            public Builder(CreateSpecContainerCollectionPropertyDefinitionTemplate createSpecContainerCollectionPropertyDefinition) {
                this.createSpecContainerCollectionPropertyDefinition = createSpecContainerCollectionPropertyDefinition;
            }

            public SpecContainerCollectionTemplate Build(
                    SpecContainerCollectionDefinition specContainerCollectionDefinition,
                    string specContainerCollectionReferenceName
            ) {
                var specContainerProperties = specContainerCollectionDefinition.SpecContainerReferences.Select(
                                specContainerReference => createSpecContainerCollectionPropertyDefinition(specContainerReference))
                        .ToImmutableList();

                return new SpecContainerCollectionTemplate(
                        specContainerCollectionDefinition.InjectorType.TypeName,
                        specContainerCollectionDefinition.SpecContainerCollectionType.TypeName,
                        specContainerCollectionReferenceName,
                        specContainerProperties,
                        specContainerCollectionDefinition.Location);
            }
        }
    }
}
