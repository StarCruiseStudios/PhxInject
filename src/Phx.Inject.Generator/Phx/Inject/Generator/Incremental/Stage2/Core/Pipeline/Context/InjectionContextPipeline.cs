// -----------------------------------------------------------------------------
// <copyright file="InjectionContextPipeline.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage2.Core.Model.Context;
using Phx.Inject.Generator.Incremental.Stage2.Core.Model.SpecContainer;
using Phx.Inject.Generator.Incremental.Stage2.Core.Pipeline.Injector;
using Phx.Inject.Generator.Incremental.Stage2.Core.Pipeline.SpecContainer;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Pipeline.Context;

internal class InjectionContextPipeline {
    public static readonly InjectionContextPipeline Instance = new();

    public IncrementalValuesProvider<InjectionContextModel> Select(
        IncrementalValuesProvider<InjectorInterfaceMetadata> injectorMetadataProvider,
        IncrementalValuesProvider<SpecClassMetadata> specClassMetadataProvider,
        IncrementalValuesProvider<SpecInterfaceMetadata> specInterfaceMetadataProvider,
        IncrementalValuesProvider<InjectorDependencyInterfaceMetadata> injectorDependencyMetadataProvider
    ) {
        var allSpecClasses = specClassMetadataProvider.Collect();
        var allSpecInterfaces = specInterfaceMetadataProvider.Collect();
        var allDependencies = injectorDependencyMetadataProvider.Collect();

        return injectorMetadataProvider
            .Combine(allSpecClasses)
            .Combine(allSpecInterfaces)
            .Combine(allDependencies)
            .Select((tuple, _) => {
                var (((injectorMetadata, specClasses), specInterfaces), dependencies) = tuple;
                var specClassByType = specClasses.ToDictionary(s => s.SpecType.NamespacedBaseTypeName);
                var specInterfaceByType = specInterfaces.ToDictionary(s => s.SpecInterfaceType.NamespacedBaseTypeName);
                var dependencyByType = dependencies.ToDictionary(d => d.InjectorDependencyInterfaceType.NamespacedBaseTypeName);

                var constructedSpecTypes = new List<TypeMetadata>();
                var specContainers = new List<SpecContainerModel>();
                var injectorType = injectorMetadata.InjectorInterfaceType
                    .CreateInjectorType(injectorMetadata.InjectorAttributeMetadata.GeneratedClassName);

                foreach (var specType in injectorMetadata.InjectorAttributeMetadata.Specifications) {
                    var key = specType.NamespacedBaseTypeName;
                    if (specClassByType.TryGetValue(key, out var specClass)) {
                        specContainers.Add(SpecContainerMapper.Map(injectorType, specClass));
                    } else if (specInterfaceByType.TryGetValue(key, out var specInterface)) {
                        constructedSpecTypes.Add(specType);
                        specContainers.Add(SpecContainerMapper.Map(injectorType, specInterface));
                    } else if (dependencyByType.TryGetValue(key, out var dependency)) {
                        constructedSpecTypes.Add(specType);
                        specContainers.Add(SpecContainerMapper.Map(injectorType, dependency));
                    }
                }

                var injectorModel = InjectorMapper.Instance.Map(injectorMetadata, constructedSpecTypes);

                return new InjectionContextModel(
                    injectorModel,
                    specContainers,
                    injectorMetadata.Location
                );
            });
    }
}
