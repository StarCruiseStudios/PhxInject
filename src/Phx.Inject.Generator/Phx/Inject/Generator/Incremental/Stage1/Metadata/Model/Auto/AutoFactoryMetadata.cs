// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Auto;

/// <summary>
///     Metadata representing an analyzed auto-generated factory.
///     <para>
///         Auto-Generation Convention: Classes meeting specific criteria are automatically
///         treated as factories without explicit [Factory] attributes. Criteria include: public/internal
///         accessibility, non-abstract, single public constructor, and all constructor dependencies
///         satisfiable by the injection graph.
///     </para>
///     <para>
///         Required Properties: Properties marked with 'required' keyword that must be
///         initialized during object construction. The generator ensures these are satisfied either through
///         constructor parameters or post-construction initialization.
///     </para>
///     <para>
///         Scoping: Apply [Factory(FabricationMode.Scoped)] to the class to control
///         instance lifetime (transient vs scoped).
///     </para>
/// </summary>
/// <param name="AutoFactoryType"> The type being auto-generated as a factory. </param>
/// <param name="Parameters"> Constructor parameters that must be resolved from the dependency graph. </param>
/// <param name="RequiredProperties"> C# 'required' properties that must be initialized post-construction. </param>
/// <param name="AutoFactoryAttributeMetadata"> The [AutoFactory] attribute metadata controlling generation. </param>
/// <param name="Location"> The source location of the class definition. </param>
internal record AutoFactoryMetadata( 
    QualifiedTypeMetadata AutoFactoryType,
    EquatableList<QualifiedTypeMetadata> Parameters,
    EquatableList<AutoFactoryRequiredPropertyMetadata> RequiredProperties,
    AutoFactoryAttributeMetadata AutoFactoryAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }