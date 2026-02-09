// -----------------------------------------------------------------------------
// <copyright file="BuilderProvider.cs" company="Star Cruise Studios LLC">
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
/// Represents a builder method that can build a qualified type.
/// </summary>
internal abstract record BuilderProvider : IProvider {
    public abstract QualifiedTypeMetadata ProvidedType { get; }
    public abstract IReadOnlyList<QualifiedTypeMetadata> Dependencies { get; }
}

/// <summary>
/// Represents a builder method from a specification.
/// </summary>
internal record SpecBuilderMethodProvider(
    SpecBuilderMethodMetadata Metadata
) : BuilderProvider {
    public override QualifiedTypeMetadata ProvidedType => Metadata.BuiltType;
    public override IReadOnlyList<QualifiedTypeMetadata> Dependencies => Metadata.Parameters.ToList();
}

/// <summary>
/// Represents a builder reference from a specification.
/// </summary>
internal record SpecBuilderReferenceProvider(
    SpecBuilderReferenceMetadata Metadata
) : BuilderProvider {
    public override QualifiedTypeMetadata ProvidedType => Metadata.BuiltType;
    public override IReadOnlyList<QualifiedTypeMetadata> Dependencies => Metadata.Parameters.ToList();
}

/// <summary>
/// Represents an auto builder.
/// </summary>
internal record AutoBuilderProvider(
    AutoBuilderMetadata Metadata
) : BuilderProvider {
    public override QualifiedTypeMetadata ProvidedType => Metadata.BuiltType;
    public override IReadOnlyList<QualifiedTypeMetadata> Dependencies => Metadata.Parameters.ToList();
}
