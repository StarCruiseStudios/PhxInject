// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerFactoryTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Model;

namespace Phx.Inject.Generator.Project.Templates;

internal record SpecContainerFactoryTemplate(
    string ReturnTypeQualifiedName,
    string SpecContainerFactoryMethodName,
    string SpecFactoryMemberName,
    SpecFactoryMemberType SpecFactoryMemberType,
    string SpecContainerCollectionQualifiedType,
    string SpecContainerCollectionReferenceName,
    string? InstanceHolderReference,
    bool startNewContainer,
    string? ConstructedSpecificationReference,
    string SpecificationQualifiedType,
    IEnumerable<SpecContainerFactoryInvocationTemplate> Arguments,
    IEnumerable<RequiredPropertyTemplate> RequiredProperties,
    Location Location
) : ISpecContainerMemberTemplate {
    public void Render(IRenderWriter writer) {
        var specContainerCollectionArgName = startNewContainer
            ? "parentSpecContainer"
            : SpecContainerCollectionReferenceName;

        writer.AppendLine($"internal {ReturnTypeQualifiedName} {SpecContainerFactoryMethodName}(")
            .IncreaseIndent(2)
            .AppendLine($"{SpecContainerCollectionQualifiedType} {specContainerCollectionArgName}")
            .DecreaseIndent(2)
            .AppendLine(") {")
            .IncreaseIndent(1);

        if (startNewContainer) {
            writer.AppendLine(
                $"var {SpecContainerCollectionReferenceName} = parentSpecContainer.CreateNewFrame();");
        }

        writer.Append("return ");
        if (!string.IsNullOrEmpty(InstanceHolderReference)) {
            writer.Append($"{InstanceHolderReference} ??= ");
        }

        var referenceName = ConstructedSpecificationReference ?? SpecificationQualifiedType;
        var numArguments = Arguments.Count();
        switch (SpecFactoryMemberType) {
            case SpecFactoryMemberType.Method:
            case SpecFactoryMemberType.Reference:
                writer.Append($"{referenceName}.{SpecFactoryMemberName}");
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

            case SpecFactoryMemberType.Constructor:
                writer.Append($"new {ReturnTypeQualifiedName}");
                var numRequiredProperties = RequiredProperties.Count();

                if (numArguments == 0) {
                    writer.Append("()");
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

                    if (numRequiredProperties > 0) {
                        writer.DecreaseIndent(1)
                            .AppendLine()
                            .Append(")");
                    } else {
                        writer.Append(")")
                            .DecreaseIndent(1);
                    }
                }

                if (numRequiredProperties > 0) {
                    writer.Append(" {")
                        .IncreaseIndent(1);
                    var isFirst = true;
                    foreach (var property in RequiredProperties.OrderBy(p => p.PropertyName)) {
                        if (isFirst) {
                            isFirst = false;
                        } else {
                            writer.Append(",");
                        }

                        writer.AppendLine();

                        writer.Append(property.PropertyName)
                            .Append(" = ");
                        property.PropertyValue.Render(writer);
                    }

                    writer.DecreaseIndent(1)
                        .AppendLine()
                        .Append("}");
                }

                writer.AppendLine(";");

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
