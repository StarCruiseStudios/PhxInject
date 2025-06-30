// -----------------------------------------------------------------------------
// <copyright file="SourceDescriptor.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record SourceDesc(
    IReadOnlyList<InjectorDesc> injectorDescs,
    IReadOnlyList<SpecDesc> specDescs,
    IReadOnlyList<DependencyDesc> dependencyDescs
) {
    public IReadOnlyList<SpecDesc> GetAllSpecDescs() {
        return dependencyDescs
            .Select(dep => dep.GetSpecDesc())
            .Concat(specDescs)
            .ToImmutableList();
    }
}
