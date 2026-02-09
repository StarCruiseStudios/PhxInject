// -----------------------------------------------------------------------------
// <copyright file="InjectorModelMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Stage2.Model;

namespace Phx.Inject.Generator.Incremental.Stage2.Mappers;

internal static class InjectorModelMapper {
    /// <summary>
    /// Map an InjectorInterfaceMetadata to an InjectorModel domain object.
    /// Combines data from the injector interface, attributes, and specifications.
    /// </summary>
    public static InjectorModel MapToModel(InjectorInterfaceMetadata metadata) {
        // Determine which specs need to be instantiated (non-static)
        var constructedSpecs = metadata.InjectorAttributeMetadata.Specifications
            .Where(spec => IsConstructedSpecification(spec))
            .ToImmutableList();

        // Generate the implementation type name
        var implTypeName = metadata.InjectorAttributeMetadata.GeneratedClassName ??
                          GetDefaultInjectorImplementationName(metadata.InjectorInterfaceType);
        
        var implType = metadata.InjectorInterfaceType with {
            BaseTypeName = implTypeName,
            TypeArguments = ImmutableList<TypeMetadata>.Empty
        };

        return new InjectorModel(
            InjectorInterfaceType: metadata.InjectorInterfaceType,
            InjectorImplementationType: implType,
            SpecificationTypes: metadata.InjectorAttributeMetadata.Specifications,
            ConstructedSpecificationTypes: constructedSpecs,
            DependencyInterfaceType: metadata.DependencyAttributeMetadata?.DependencyType,
            Providers: metadata.Providers.Select(p => new ProviderModel(
                ProviderMethodName: p.ProviderMethodName,
                ProvidedType: p.ProvidedType
            )),
            Activators: metadata.Activators.Select(a => new ActivatorModel(
                ActivatorMethodName: a.ActivatorMethodName,
                ActivatedType: a.ActivatedType
            )),
            ChildInjectors: metadata.ChildProviders.Select(c => new ChildInjectorModel(
                ChildInjectorMethodName: c.ChildProviderMethodName,
                ChildInjectorType: c.ChildInjectorType,
                ConstructedSpecificationParameters: c.Parameters
            ))
        );
    }

    private static string GetDefaultInjectorImplementationName(TypeMetadata interfaceType) {
        var baseName = interfaceType.BaseTypeName;
        // Remove leading 'I' if present
        if (baseName.StartsWith("I") && baseName.Length > 1 && char.IsUpper(baseName[1])) {
            baseName = baseName.Substring(1);
        }
        return $"Generated{baseName}";
    }

    private static bool IsConstructedSpecification(TypeMetadata specType) {
        // In Stage1, we don't have full information about whether a spec is constructed
        // This would typically be determined by checking if it's an interface
        // For now, we'll use a heuristic based on the type name
        // TODO: This should be improved with actual type information
        return false; // Simplified for now; would need symbol information
    }
}
