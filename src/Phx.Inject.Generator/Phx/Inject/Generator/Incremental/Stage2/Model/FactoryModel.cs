// -----------------------------------------------------------------------------
// <copyright file="FactoryModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;

namespace Phx.Inject.Generator.Incremental.Stage2.Model;

/// <summary>
/// Domain model representing a factory (method or property that produces a type).
/// </summary>
internal record FactoryModel(
    string FactoryMemberName,
    QualifiedTypeMetadata ReturnType,
    IEnumerable<QualifiedTypeMetadata> Parameters,
    FactoryMemberType MemberType,
    FabricationMode FabricationMode,
    IEnumerable<RequiredPropertyModel> RequiredProperties,
    bool IsPartial
);

/// <summary>
/// Type of factory member.
/// </summary>
internal enum FactoryMemberType {
    Method,
    Property,
    FieldReference
}

/// <summary>
/// Fabrication mode for factories.
/// </summary>
internal enum FabricationMode {
    Recurrent,
    Scoped,
    Container,
    ContainerScoped
}

/// <summary>
/// Required property for auto-factories.
/// </summary>
internal record RequiredPropertyModel(
    string PropertyName,
    QualifiedTypeMetadata PropertyType
);
