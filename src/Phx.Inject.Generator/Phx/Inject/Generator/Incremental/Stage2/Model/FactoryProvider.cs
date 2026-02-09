// -----------------------------------------------------------------------------
// <copyright file="FactoryProvider.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Auto;

namespace Phx.Inject.Generator.Incremental.Stage2.Model;

/// <summary>
/// Represents a factory method or property that provides a qualified type.
/// </summary>
internal abstract record FactoryProvider : IProvider {
    public abstract QualifiedTypeMetadata ProvidedType { get; }
    public abstract IReadOnlyList<QualifiedTypeMetadata> Dependencies { get; }
}

/// <summary>
/// Represents a factory method from a specification.
/// </summary>
internal record SpecFactoryMethodProvider(
    SpecFactoryMethodMetadata Metadata
) : FactoryProvider {
    public override QualifiedTypeMetadata ProvidedType => Metadata.FactoryReturnType;
    public override IReadOnlyList<QualifiedTypeMetadata> Dependencies => Metadata.Parameters.ToList();
}

/// <summary>
/// Represents a factory property from a specification.
/// </summary>
internal record SpecFactoryPropertyProvider(
    SpecFactoryPropertyMetadata Metadata
) : FactoryProvider {
    public override QualifiedTypeMetadata ProvidedType => Metadata.FactoryReturnType;
    public override IReadOnlyList<QualifiedTypeMetadata> Dependencies => Array.Empty<QualifiedTypeMetadata>();
}

/// <summary>
/// Represents a factory reference from a specification.
/// </summary>
internal record SpecFactoryReferenceProvider(
    SpecFactoryReferenceMetadata Metadata
) : FactoryProvider {
    public override QualifiedTypeMetadata ProvidedType => Metadata.FactoryReturnType;
    public override IReadOnlyList<QualifiedTypeMetadata> Dependencies => Metadata.Parameters.ToList();
}

/// <summary>
/// Represents an auto factory.
/// </summary>
internal record AutoFactoryProvider(
    AutoFactoryMetadata Metadata
) : FactoryProvider {
    public override QualifiedTypeMetadata ProvidedType => Metadata.AutoFactoryType;
    public override IReadOnlyList<QualifiedTypeMetadata> Dependencies {
        get {
            var deps = new List<QualifiedTypeMetadata>();
            deps.AddRange(Metadata.Parameters);
            deps.AddRange(Metadata.RequiredProperties.Select(p => p.RequiredPropertyType));
            return deps;
        }
    }
}
