// -----------------------------------------------------------------------------
// <copyright file="SpecContainerBuilderInvocationModel.cs" company="Star Cruise Studios LLC">
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
///     Model representing an invocation of a specification container builder method.
/// </summary>
/// <remarks>
///     <para>Builder Invocation Pattern:</para>
///     <para>
///     Represents a resolved builder dependency in the dependency graph. While structurally
///     similar to factory invocations, builder invocations have different semantics:
///     - Factory invocation: Immediately creates and returns an instance
///     - Builder invocation: Returns an IBuilder&lt;T&gt; that defers creation until .Build()
///     </para>
///     
///     <para>Cross-Container Builder Dependencies:</para>
///     <para>
///     Builders can depend on other builders across specification containers, enabling
///     compositional builder patterns where one builder uses another builder's output
///     as input for its own construction logic. The invocation model captures the
///     container and method name to call.
///     </para>
///     
///     <para>Generated Code Example:</para>
///     <code>
///     // SpecContainerType = "ServiceSpec_Container"
///     // BuilderMethodName = "BuildLogger"
///     // Generates:
///     var loggerBuilder = serviceSpecContainer.BuildLogger();
///     // Caller can now configure: loggerBuilder.WithLevel(Debug).Build()
///     </code>
///     
///     <para>Usage in Factory Arguments:</para>
///     <para>
///     Though defined separately, builder invocations appear in factory Arguments when a
///     factory needs a builder as a dependency. This is less common than factory-to-factory
///     dependencies but supports patterns where the factory internally calls .Build().
///     </para>
/// </remarks>
/// <param name="SpecContainerType"> The specification container type. </param>
/// <param name="BuilderMethodName"> The name of the builder method to invoke. </param>
/// <param name="Location"> The source location where this invocation is defined. </param>
internal record SpecContainerBuilderInvocationModel(
    TypeMetadata SpecContainerType,
    string BuilderMethodName,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
