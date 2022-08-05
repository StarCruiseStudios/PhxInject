// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerBuilderTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Specifications.Templates {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common.Templates;

    internal record SpecContainerBuilderTemplate(
            string BuiltTypeQualifiedName,
            string MethodName,
            string BuiltInstanceReferenceName,
            string SpecContainerCollectionQualifiedType,
            string SpecContainerCollectionReferenceName,
            string? ConstructedSpecificationReference,
            string SpecificationQualifiedType,
            IEnumerable<SpecContainerFactoryInvocationTemplate> Arguments,
            Location Location
    ) : ISpecContainerMemberTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"internal void {MethodName}(")
                    .AppendLine(
                            $"{BuiltTypeQualifiedName} {BuiltInstanceReferenceName}, {SpecContainerCollectionQualifiedType} {SpecContainerCollectionReferenceName}) {{")
                    .IncreaseIndent(1);

            var referenceName = ConstructedSpecificationReference ?? SpecificationQualifiedType;
            writer.AppendLine($"{referenceName}.{MethodName}(")
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
                    .DecreaseIndent(2)
                    .AppendLine("}");
        }
    }
}
