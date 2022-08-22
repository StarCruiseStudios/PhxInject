// -----------------------------------------------------------------------------
//  <copyright file="InjectorChildFactoryTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Injectors.Templates {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common.Templates;

    internal record InjectorChildFactoryTemplate(
            string ChildInterfaceTypeQualifiedName,
            string MethodName,
            string ChildTypeQualifiedName,
            IEnumerable<InjectorConstructorParameter> ConstructorParameters,
            IEnumerable<string> ChildExternalDependencyImplementationTypeQualifiedNames,
            string SpecContainerCollectionReferenceName,
            Location Location
    ) : IInjectorMemberTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append($"public {ChildInterfaceTypeQualifiedName} {MethodName}(");
            
            if (ConstructorParameters.Any()) {
                using (var collectionWriter = writer.GetCollectionWriter(CollectionWriterProperties.Default)) {
                    foreach (var parameter in ConstructorParameters) {
                        var elementWriter = collectionWriter.GetElementWriter();
                        elementWriter.Append($"{parameter.ParameterTypeQualifiedName} {parameter.ParameterName}");
                    }
                }
            }
            
            writer.AppendLine(") {")
                    .IncreaseIndent(1);
            
            using (var collectionWriter = writer.GetCollectionWriter(
                           new CollectionWriterProperties(
                                   OpeningString: $"return new {ChildTypeQualifiedName}(",
                                   ClosingString: ");",
                                   CloseWithNewline: false))) {
                foreach (var parameter in ConstructorParameters) {
                    var elementWriter = collectionWriter.GetElementWriter();
                    elementWriter.Append(parameter.ParameterName);
                }
                
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
