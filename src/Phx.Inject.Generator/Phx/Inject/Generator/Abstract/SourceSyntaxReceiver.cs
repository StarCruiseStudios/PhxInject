// -----------------------------------------------------------------------------
//  <copyright file="InjectorSyntaxReceiver.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Phx.Inject.Generator.Abstract;

internal class SourceSyntaxReceiver : ISyntaxReceiver {
    public List<TypeDeclarationSyntax> PhxInjectSettingsCandidates { get; } = new();
    public List<TypeDeclarationSyntax> InjectorCandidates { get; } = new();
    public List<TypeDeclarationSyntax> SpecificationCandidates { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
        switch (syntaxNode) {
            case InterfaceDeclarationSyntax interfaceDeclaration:
                // Track all interfaces with the injector attribute as injector candidates.
                if (interfaceDeclaration.HasInjectorAttribute()) {
                    InjectorCandidates.Add(interfaceDeclaration);
                }

                // Track all interfaces with the specification attribute as specification candidates.
                if (interfaceDeclaration.HasSpecificationAttribute()) {
                    SpecificationCandidates.Add(interfaceDeclaration);
                }

                break;
            case ClassDeclarationSyntax classDeclaration:
                // Track all classes with the specification attribute as specification candidates.
                if (classDeclaration.HasSpecificationAttribute()) {
                    SpecificationCandidates.Add(classDeclaration);
                }

                // Track all classes with the phx inject attribute as settings candidates.
                if (classDeclaration.HasPhxInjectAttribute()) {
                    PhxInjectSettingsCandidates.Add(classDeclaration);
                }

                break;
        }
    }
}

internal static class MemberDeclarationSyntaxExtensions {
    public const string InjectorAttributeShortName = "Injector";
    public const string InjectorAttributeBaseName = nameof(InjectorAttribute);
    public const string PhxInjectAttributeShortName = "PhxInject";
    public const string PhxInjectAttributeBaseName = nameof(PhxInjectAttribute);
    public const string SpecificationAttributeShortName = "Specification";
    public const string SpecificationAttributeBaseName = nameof(SpecificationAttribute);

    public static bool HasInjectorAttribute(this MemberDeclarationSyntax memberDeclaration) {
        return HasAttribute(memberDeclaration,
            it =>
                it is InjectorAttributeShortName
                    or InjectorAttributeBaseName);
    }

    public static bool HasPhxInjectAttribute(this MemberDeclarationSyntax memberDeclaration) {
        return HasAttribute(memberDeclaration,
            it =>
                it is PhxInjectAttributeShortName
                    or PhxInjectAttributeBaseName);
    }

    public static bool HasSpecificationAttribute(this MemberDeclarationSyntax memberDeclaration) {
        return HasAttribute(memberDeclaration,
            it =>
                it is SpecificationAttributeShortName
                    or SpecificationAttributeBaseName);
    }

    private static bool HasAttribute(MemberDeclarationSyntax memberDeclaration, Func<string, bool> predicate) {
        return memberDeclaration.AttributeLists
            .Any(attributeList => attributeList.Attributes
                .Any(attribute => {
                    var name = attribute.Name.ToString();
                    return predicate(name);
                }));
    }
}
