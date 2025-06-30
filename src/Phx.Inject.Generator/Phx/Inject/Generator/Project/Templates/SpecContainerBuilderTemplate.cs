// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerBuilderTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;

namespace Phx.Inject.Generator.Project.Templates;

internal record SpecContainerBuilderTemplate(
    string BuiltTypeQualifiedName,
    string SpecContainerBuilderMethodName,
    string SpecBuilderMemberName,
    string BuiltInstanceReferenceName,
    string SpecContainerCollectionQualifiedType,
    string SpecContainerCollectionReferenceName,
    string SpecificationQualifiedType,
    IEnumerable<SpecContainerFactoryInvocationTemplate> Arguments,
    Location Location
) : ISpecContainerMemberTemplate {
    public void Render(IRenderWriter writer) {
        writer.AppendLine($"internal void {SpecContainerBuilderMethodName}(")
            .IncreaseIndent(2)
            .AppendLine(
                $"{BuiltTypeQualifiedName} {BuiltInstanceReferenceName},")
            .AppendLine(
                $"{SpecContainerCollectionQualifiedType} {SpecContainerCollectionReferenceName}")
            .DecreaseIndent(2)
            .AppendLine(") {")
            .IncreaseIndent(1);

        var referenceName = SpecificationQualifiedType;
        writer.AppendLine($"{referenceName}.{SpecBuilderMemberName}(")
            .IncreaseIndent(1)
            .Append($"{BuiltInstanceReferenceName}");

        var numArguments = Arguments.Count();
        if (numArguments > 0) {
            foreach (var argument in Arguments) {
                writer.AppendLine(",");
                argument.Render(writer);
            }
        }

        writer.AppendLine(");")
            .DecreaseIndent(1);

        writer.DecreaseIndent(1)
            .AppendLine("}");
    }
}
