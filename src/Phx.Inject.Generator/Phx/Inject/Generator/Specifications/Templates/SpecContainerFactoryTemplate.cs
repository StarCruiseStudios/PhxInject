// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerFactoryTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Specifications.Templates {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Templates;

    internal record SpecContainerFactoryTemplate(
            string ReturnTypeQualifiedName,
            string SpecContainerFactoryMethodName,
            string SpecFactoryMemberName,
            SpecFactoryMemberType SpecFactoryMemberType,
            string SpecContainerCollectionQualifiedType,
            string SpecContainerCollectionReferenceName,
            string? InstanceHolderReference,
            string? ConstructedSpecificationReference,
            string SpecificationQualifiedType,
            IEnumerable<SpecContainerFactoryInvocationTemplate> Arguments,
            Location Location
    ) : ISpecContainerMemberTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"internal {ReturnTypeQualifiedName} {SpecContainerFactoryMethodName}(")
                    .IncreaseIndent(2)
                    .AppendLine($"{SpecContainerCollectionQualifiedType} {SpecContainerCollectionReferenceName}")
                    .DecreaseIndent(2)
                    .AppendLine(") {")
                    .IncreaseIndent(1)
                    .Append("return ");
            if (!string.IsNullOrEmpty(InstanceHolderReference)) {
                writer.Append($"{InstanceHolderReference} ??= ");
            }

            var referenceName = ConstructedSpecificationReference ?? SpecificationQualifiedType;

            switch (SpecFactoryMemberType) {
                case SpecFactoryMemberType.Method:
                case SpecFactoryMemberType.Reference:
                    writer.Append($"{referenceName}.{SpecFactoryMemberName}");
                    var numArguments = Arguments.Count();
                    if (numArguments == 0) {
                        writer.AppendLine("();");
                    } else {
                        writer.AppendLine("(")
                                .IncreaseIndent(1);
                        var isFirst = true;
                        foreach (var argument in Arguments) {
                            if (!isFirst) {
                                writer.AppendLine(",");
                            }

                            isFirst = false;
                            argument.Render(writer);
                        }

                        writer.AppendLine(");")
                                .DecreaseIndent(1);
                    }

                    break;

                case SpecFactoryMemberType.Property:
                    writer.AppendLine($"{referenceName}.{SpecFactoryMemberName};");
                    break;

                default:
                    throw new InjectionException(
                            Diagnostics.InternalError,
                            $"Unhandled Spec Factory Member Type {SpecFactoryMemberType}.",
                            Location);
            }

            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }
    }
}
