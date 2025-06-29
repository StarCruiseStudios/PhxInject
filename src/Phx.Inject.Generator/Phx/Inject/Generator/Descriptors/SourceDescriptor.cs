// -----------------------------------------------------------------------------
// <copyright file="SourceDescriptor.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;

namespace Phx.Inject.Generator.Descriptors;

internal record SourceDescriptor(
    IReadOnlyList<InjectorDescriptor> injectorDescriptors,
    IReadOnlyList<SpecDescriptor> specDescriptors,
    IReadOnlyList<ExternalDependencyDescriptor> externalDependencyDescriptors
) {
    public IReadOnlyList<SpecDescriptor> GetAllSpecDescriptors() {
        return externalDependencyDescriptors
            .Select(dep => dep.GetSpecDescriptor())
            .Concat(specDescriptors)
            .ToImmutableList();
    }
}