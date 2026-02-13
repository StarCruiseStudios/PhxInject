// -----------------------------------------------------------------------------
// <copyright file="SpecContainerFactoryInvocationModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Model.SpecContainer;

/// <summary>
///     Model representing an invocation of a specification container factory method.
/// </summary>
/// <remarks>
///     <para>Dependency Resolution Pattern:</para>
///     <para>
///     Represents a resolved dependency that must be created by calling one or more factory methods.
///     Each invocation captures the chain of factory calls needed to satisfy a parameter or property
///     requirement in the dependency graph. This enables transitive dependency resolution across
///     multiple specification containers.
///     </para>
///     
///     <para>Factory Invocation Chains:</para>
///     <para>
///     FactoryInvocationDefs contains an ordered list of factory calls that must execute to produce
///     the final value. For simple dependencies, this is a single call. For cross-spec dependencies,
///     this chains multiple calls: `containerA.GetSpec() → spec.CreateFoo()`.
///     </para>
///     
///     <para>Runtime Type Support:</para>
///     <para>
///     RuntimeFactoryProvidedType enables dependency injection containers to provide concrete
///     implementations at runtime (e.g., IService → ServiceImpl). When non-null, the generated
///     code accepts this type as a parameter rather than resolving it from the factory graph.
///     </para>
///     
///     <para>Generated Code Example:</para>
///     <code>
///     // Single invocation:
///     var logger = this.CreateLogger();
///     
///     // Chained invocation (cross-spec):
///     var service = otherContainer.CreateService();
///     </code>
/// </remarks>
/// <param name="FactoryInvocationDefs"> The factory invocations to chain together. </param>
/// <param name="FactoryReturnType"> The return type of the factory invocation. </param>
/// <param name="RuntimeFactoryProvidedType"> The optional runtime-provided type for the factory. </param>
/// <param name="Location"> The source location where this invocation is defined. </param>
internal record SpecContainerFactoryInvocationModel(
    EquatableList<SpecContainerFactorySingleInvocationModel> FactoryInvocationDefs,
    QualifiedTypeMetadata FactoryReturnType,
    TypeMetadata? RuntimeFactoryProvidedType,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
