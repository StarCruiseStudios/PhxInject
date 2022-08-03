// -----------------------------------------------------------------------------
//  <copyright file="InjectorChildFactoryTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Injectors.Templates {
    using Microsoft.CodeAnalysis;

    // internal delegate InjectorChildMethodTemplate CreateInjectorChildMethodTemplate();

    internal record InjectorChildFactoryTemplate(
            string ChildInterfaceTypeQualifiedName,
            string MethodName,
            string ChildTypeQualifiedName,
            string ChildExternalDependencyImplementationTypeQualifiedName,
            string SpecContainerCollectionReferenceName,
            Location Location) : IInjectorMemberTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"public {ChildInterfaceTypeQualifiedName} {MethodName}() {{")
                    .IncreaseIndent(1)
                    .AppendLine($"return new {ChildTypeQualifiedName}(")
                    .IncreaseIndent(2)
                    .AppendLine($"new {ChildExternalDependencyImplementationTypeQualifiedName}({SpecContainerCollectionReferenceName}));")
                    .DecreaseIndent(3)
                    .AppendLine("}");
        }
    }
}
