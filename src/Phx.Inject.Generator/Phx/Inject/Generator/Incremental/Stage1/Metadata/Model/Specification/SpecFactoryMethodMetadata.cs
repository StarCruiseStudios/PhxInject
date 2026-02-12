// -----------------------------------------------------------------------------
// <copyright file="SpecFactoryMethodMetadata.cs" company="Star Cruise Studios LLC">
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

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;

/// <summary>
///     Metadata representing an analyzed specification factory method.
///     <para>
///         <strong>Factory Methods:</strong> Create and return NEW instances of dependencies. Distinguished
///         from Builders by non-void return type. Factories form the core of instance creation in the
///         dependency graph.
///     </para>
///     <para>
///         <strong>Parameters:</strong> Runtime dependencies injected when this factory is invoked.
///         All parameters must be satisfiable by the dependency graph or provided externally.
///     </para>
/// </summary>
/// <param name="FactoryMethodName"> The name of the factory method. </param>
/// <param name="FactoryReturnType"> The qualified type returned by the factory (must be non-void). </param>
/// <param name="Parameters"> Runtime parameters required by the factory, resolved from the graph. </param>
/// <param name="FactoryAttributeMetadata"> The [Factory] attribute metadata controlling fabrication mode. </param>
/// <param name="PartialFactoryAttributeMetadata"> Optional [Partial] attribute for partial dependency satisfaction. </param>
/// <param name="Location"> The source location of the method definition. </param>
internal record SpecFactoryMethodMetadata(
    string FactoryMethodName,
    QualifiedTypeMetadata FactoryReturnType,
    EquatableList<QualifiedTypeMetadata> Parameters,
    FactoryAttributeMetadata FactoryAttributeMetadata,
    PartialAttributeMetadata? PartialFactoryAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
