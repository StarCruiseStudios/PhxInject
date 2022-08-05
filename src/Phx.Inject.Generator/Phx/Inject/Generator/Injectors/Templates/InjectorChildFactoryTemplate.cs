// -----------------------------------------------------------------------------
//  <copyright file="InjectorChildFactoryTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Injectors.Templates {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal record InjectorChildFactoryTemplate(
            string ChildInterfaceTypeQualifiedName,
            string MethodName,
            string ChildTypeQualifiedName,
            IEnumerable<string> ChildExternalDependencyImplementationTypeQualifiedNames,
            string SpecContainerCollectionReferenceName,
            Location Location) : IInjectorMemberTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"public {ChildInterfaceTypeQualifiedName} {MethodName}() {{")
                    .IncreaseIndent(1);

            using (var collectionWriter = writer.GetCollectionWriter(new CollectionWriterProperties(
                           OpeningString: $"return new {ChildTypeQualifiedName}(",
                           ClosingString:");",
                           CloseWithNewline: false))) {
                foreach (var externalDependency in ChildExternalDependencyImplementationTypeQualifiedNames) {
                    var elementWriter = collectionWriter.GetElementWriter();
                    elementWriter.Append($"new {externalDependency}({SpecContainerCollectionReferenceName})");
                }
            }

            writer.AppendLine()
                    .DecreaseIndent(1)
                    .AppendLine("}");
        }
    }
}
