// -----------------------------------------------------------------------------
// <copyright file="SpecContainerFactoryModel.cs" company="Star Cruise Studios LLC">
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
///     Model representing a factory method in a specification container.
/// </summary>
/// <remarks>
///     <para>Factory Method Purpose:</para>
///     <para>
///     Represents a single factory method in the generated SpecContainer that creates instances
///     of a specific type. Each factory method resolves its dependencies recursively by invoking
///     other factories in the dependency graph, passing constructor arguments and setting required
///     properties as specified by the [AutoFactory] or [Factory] pattern.
///     </para>
///     
///     <para>Relationship to Stage 1 Metadata:</para>
///     <para>
///     Derived from Stage 1's SpecFactoryMethodMetadata, SpecFactoryPropertyMetadata, or
///     SpecFactoryReferenceMetadata. Stage 2 transforms these into invocable factory methods:
///     - Method: Calls user's factory method in the spec
///     - Property: Reads user's factory property in the spec
///     - Reference: Resolves dependency from another container
///     - Constructor: Directly invokes type constructor (AutoFactory pattern)
///     </para>
///     
///     <para>AutoFactory Required Properties Pattern:</para>
///     <para>
///     For [AutoFactory] patterns, the generator analyzes the target type's constructor and
///     required properties. RequiredProperties contains initialization for properties marked
///     as `required` in C# or annotated with initialization attributes, enabling object
///     initializer syntax in generated code: `new T(args) { Prop = value }`.
///     </para>
///     
///     <para>Generated Code Example:</para>
///     <code>
///     // From: [Factory] public IService CreateService(ILogger logger) { ... }
///     // Generates in container:
///     public IService CreateService() {
///         var arg0 = spec.CreateLogger(); // Resolve dependency
///         return spec.CreateService(arg0); // Invoke user's factory
///     }
///     </code>
/// </remarks>
/// <param name="ReturnType"> The return type of the factory method. </param>
/// <param name="SpecContainerFactoryMethodName"> The name of the generated factory method. </param>
/// <param name="SpecFactoryMemberName"> The name of the specification factory member. </param>
/// <param name="SpecFactoryMemberType"> The type of the specification factory member. </param>
/// <param name="FabricationMode"> The fabrication mode for creating instances. </param>
/// <param name="Arguments"> The arguments to pass to the factory. </param>
/// <param name="RequiredProperties"> The required properties to set on the created instance. </param>
/// <param name="Location"> The source location where this factory is defined. </param>
internal record SpecContainerFactoryModel(
    QualifiedTypeMetadata ReturnType,
    string SpecContainerFactoryMethodName,
    string SpecFactoryMemberName,
    SpecFactoryMemberType SpecFactoryMemberType,
    FabricationMode FabricationMode,
    IEnumerable<SpecContainerFactoryInvocationModel> Arguments,
    IEnumerable<SpecContainerFactoryRequiredPropertyModel> RequiredProperties,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;

/// <summary>
///     Specifies the type of specification factory member.
/// </summary>
/// <remarks>
///     <para>
///     Determines how the container invokes the specification member to create instances:
///     - Method: Calls a user-defined factory method
///     - Property: Reads a user-defined factory property (lazy initialization pattern)
///     - Reference: Delegates to another container's factory (cross-spec dependencies)
///     - Constructor: Directly invokes type constructor (AutoFactory generates this)
///     </para>
/// </remarks>
internal enum SpecFactoryMemberType {
    Method,
    Property,
    Reference,
    Constructor
}
