// -----------------------------------------------------------------------------
// <copyright file="SpecContainerBuilderModel.cs" company="Star Cruise Studios LLC">
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
///     Model representing a builder method in a specification container.
/// </summary>
/// <remarks>
///     <para>Builder vs Factory Pattern Differences:</para>
///     <para>
///     Builders differ from factories in return type and lifecycle semantics:
///     - Factory: Returns concrete type T (transient or singleton creation)
///     - Builder: Returns IBuilder&lt;T&gt; (deferred creation with configuration chain)
///     
///     Builders enable fluent configuration before final instantiation, supporting scenarios
///     like: `builder.WithOption(x).WithOption(y).Build()` where configuration logic is
///     user-defined but dependency resolution is framework-managed.
///     </para>
///     
///     <para>Relationship to Stage 1 Metadata:</para>
///     <para>
///     Derived from Stage 1's SpecBuilderMethodMetadata or SpecBuilderReferenceMetadata.
///     Unlike factories which may be auto-generated, builders are always user-defined:
///     - Method: User provides builder factory method that returns IBuilder&lt;T&gt;
///     - Reference: Delegates to another container's builder
///     - Direct: Builder is directly constructed (rare, usually wrapped in method)
///     </para>
///     
///     <para>Dependency Resolution Pattern:</para>
///     <para>
///     Builder arguments are resolved identically to factory arguments - each argument in
///     the Arguments collection represents a dependency that must be satisfied by invoking
///     factories in the dependency graph. The builder receives fully-resolved dependencies
///     but defers final object construction to the caller via `.Build()`.
///     </para>
///     
///     <para>Generated Code Example:</para>
///     <code>
///     // From: [Builder] public IBuilder&lt;Service&gt; BuildService(ILogger logger) { ... }
///     // Generates in container:
///     public IBuilder&lt;Service&gt; BuildService() {
///         var arg0 = this.CreateLogger(); // Resolve dependency
///         return spec.BuildService(arg0); // Return builder for caller configuration
///     }
///     </code>
/// </remarks>
/// <param name="BuiltType"> The type constructed by the builder. </param>
/// <param name="SpecContainerBuilderMethodName"> The name of the generated builder method. </param>
/// <param name="SpecBuilderMemberName"> The name of the specification builder member. </param>
/// <param name="SpecBuilderMemberType"> The type of the specification builder member. </param>
/// <param name="Arguments"> The arguments to pass to the builder. </param>
/// <param name="Location"> The source location where this builder is defined. </param>
internal record SpecContainerBuilderModel(
    TypeMetadata BuiltType,
    string SpecContainerBuilderMethodName,
    string SpecBuilderMemberName,
    SpecBuilderMemberType SpecBuilderMemberType,
    IEnumerable<SpecContainerFactoryInvocationModel> Arguments,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;

/// <summary>
///     Specifies the type of specification builder member.
/// </summary>
internal enum SpecBuilderMemberType {
    Method,
    Reference,
    Direct
}
