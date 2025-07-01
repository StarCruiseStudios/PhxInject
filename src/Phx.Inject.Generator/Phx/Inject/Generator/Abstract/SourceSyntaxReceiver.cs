// -----------------------------------------------------------------------------
//  <copyright file="InjectorSyntaxReceiver.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Phx.Inject.Common;

namespace Phx.Inject.Generator.Abstract;

internal class SourceSyntaxReceiver : ISyntaxReceiver {
    public List<TypeDeclarationSyntax> InjectorCandidates { get; } = new();
    public List<TypeDeclarationSyntax> SpecificationCandidates { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
        switch (syntaxNode) {
            case InterfaceDeclarationSyntax interfaceDeclaration:
                if (AttributeHelpers.HasInjectorAttribute(interfaceDeclaration)) {
                    InjectorCandidates.Add(interfaceDeclaration);
                }

                if (AttributeHelpers.HasSpecificationAttribute(interfaceDeclaration)) {
                    SpecificationCandidates.Add(interfaceDeclaration);
                }

                break;
            case ClassDeclarationSyntax classDeclaration:
                if (AttributeHelpers.HasSpecificationAttribute(classDeclaration)) {
                    SpecificationCandidates.Add(classDeclaration);
                }

                break;
        }
    }
}
