﻿// -----------------------------------------------------------------------------
//  <copyright file="DependencyImplementationDef.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Metadata;

namespace Phx.Inject.Generator.Map.Definitions;

internal record DependencyImplementationDef(
    TypeModel DependencyImplementationType,
    TypeModel DependencyInterfaceType,
    IEnumerable<DependencyProviderMethodDef> ProviderMethodDefs,
    Location Location
) : IDefinition {
    public interface IMapper {
        DependencyImplementationDef Map(
            InjectorMetadata injector,
            InjectorRegistrations injectorRegistrations,
            DependencyMetadata dependencyMetadata,
            DefGenerationContext currentCtx
        );
    }

    public class Mapper : IMapper {
        public DependencyImplementationDef Map(
            InjectorMetadata injector,
            InjectorRegistrations injectorRegistrations,
            DependencyMetadata dependencyMetadata,
            DefGenerationContext currentCtx
        ) {
            var implementationType = TypeHelpers.CreateDependencyImplementationType(
                injector.InjectorType,
                dependencyMetadata.DependencyInterfaceType);

            IReadOnlyList<DependencyProviderMethodDef> providers = dependencyMetadata.Providers.Select(provider => {
                    var specContainerFactoryInvocation = TypeHelpers.GetSpecContainerFactoryInvocation(
                        injector,
                        injectorRegistrations,
                        provider.ProvidedType,
                        provider.Location,
                        currentCtx);

                    return new DependencyProviderMethodDef(
                        provider.ProvidedType.TypeModel,
                        provider.ProviderName,
                        provider.ProviderMemberType,
                        specContainerFactoryInvocation,
                        provider.Location);
                })
                .ToImmutableList();

            return new DependencyImplementationDef(
                implementationType,
                dependencyMetadata.DependencyInterfaceType,
                providers,
                dependencyMetadata.Location);
        }
    }
}
