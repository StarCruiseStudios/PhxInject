// -----------------------------------------------------------------------------
//  <copyright file="InjectorSyntaxReceiver.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class InjectorSyntaxReceiver : ISyntaxReceiver {
        public List<InterfaceDeclarationSyntax> InjectorCandidates { get; } = new();
        public List<ClassDeclarationSyntax> SpecificationCandidates { get; } = new();
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
            switch (syntaxNode) {
                case InterfaceDeclarationSyntax interfaceDeclaration:
                    if (HasInjectorAttribute(interfaceDeclaration)) {
                        InjectorCandidates.Add(interfaceDeclaration);
                    }
                    break;
                case ClassDeclarationSyntax classDeclaration:
                    if (HasSpecificationAttribute(classDeclaration)) {
                        SpecificationCandidates.Add(classDeclaration);
                    }
                    break;
            }
        }

        private bool HasInjectorAttribute(InterfaceDeclarationSyntax interfaceDeclaration) {
            return interfaceDeclaration.AttributeLists
                .Any(attributeList => attributeList.Attributes
                    .Any(attribute => {
                        var name = attribute.Name.ToString();
                        return name == "Injector" || name == "InjectorAttribute";
                    }));
        }

        private bool HasSpecificationAttribute(ClassDeclarationSyntax classDeclaration) {
            return classDeclaration.AttributeLists
                .Any(attributeList => attributeList.Attributes
                    .Any(attribute => {
                        var name = attribute.Name.ToString();
                        return name == "Specification" || name == "SpecificationAttribute";
                    }));
        }
    }
}