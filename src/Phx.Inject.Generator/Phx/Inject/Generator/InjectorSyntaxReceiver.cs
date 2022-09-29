// -----------------------------------------------------------------------------
//  <copyright file="InjectorSyntaxReceiver.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class InjectorSyntaxReceiver : ISyntaxReceiver {
        public List<TypeDeclarationSyntax> InjectorCandidates { get; } = new();
        public List<TypeDeclarationSyntax> SpecificationCandidates { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
            switch (syntaxNode) {
                case InterfaceDeclarationSyntax interfaceDeclaration:
                    if (HasInjectorAttribute(interfaceDeclaration)) {
                        InjectorCandidates.Add(interfaceDeclaration);
                    }

                    if (HasSpecificationAttribute(interfaceDeclaration)) {
                        SpecificationCandidates.Add(interfaceDeclaration);
                    }

                    break;
                case ClassDeclarationSyntax classDeclaration:
                    if (HasSpecificationAttribute(classDeclaration)) {
                        SpecificationCandidates.Add(classDeclaration);
                    }

                    break;
            }
        }

        private static bool HasInjectorAttribute(MemberDeclarationSyntax memberDeclaration) {
            return memberDeclaration.AttributeLists
                    .Any(
                            attributeList => attributeList.Attributes
                                    .Any(
                                            attribute => {
                                                var name = attribute.Name.ToString();
                                                return name is "Injector" or "InjectorAttribute";
                                            }));
        }

        private static bool HasSpecificationAttribute(MemberDeclarationSyntax memberDeclaration) {
            return memberDeclaration.AttributeLists
                    .Any(
                            attributeList => attributeList.Attributes
                                    .Any(
                                            attribute => {
                                                var name = attribute.Name.ToString();
                                                return name is "Specification" or "SpecificationAttribute";
                                            }));
        }
    }
}
