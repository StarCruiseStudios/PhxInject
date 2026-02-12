// -----------------------------------------------------------------------------
// <copyright file="SpecContainerFactoryRequiredPropertyModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Model.SpecContainer;

/// <summary>
///     Model representing a required property for a factory method.
/// </summary>
/// <remarks>
///     <para><b>AutoFactory Required Properties Pattern:</b></para>
///     <para>
///     Enables C# object initializer syntax for types with `required` properties or init-only
///     properties. When [AutoFactory] analyzes a target type, it identifies properties that must
///     be initialized at construction time and generates code to resolve their dependencies and
///     set them during object creation.
///     </para>
///     
///     <para><b>Dependency Resolution for Properties:</b></para>
///     <para>
///     The Value field contains a SpecContainerFactoryInvocationModel that resolves the property's
///     dependency the same way constructor parameters are resolved - by recursively invoking
///     factories in the dependency graph. This ensures consistency between constructor injection
///     and property injection patterns.
///     </para>
///     
///     <para><b>Generated Code Example:</b></para>
///     <code>
///     // For: required ILogger Logger { get; init; }
///     // Generates in factory:
///     return new Service(arg0, arg1) {
///         Logger = this.CreateLogger(),  // Required property initialization
///         Config = this.CreateConfig()   // Another required property
///     };
///     </code>
///     
///     <para><b>WHY Separate from Arguments:</b></para>
///     <para>
///     Properties are initialized after constructor execution, which matters for types with
///     initialization order dependencies or validation logic in property setters. Separating
///     Arguments from RequiredProperties preserves this semantic distinction in generated code.
///     </para>
/// </remarks>
/// <param name="PropertyName"> The name of the property to set. </param>
/// <param name="Value"> The factory invocation that provides the property value. </param>
/// <param name="Location"> The source location where this property is defined. </param>
internal record SpecContainerFactoryRequiredPropertyModel(
    string PropertyName,
    SpecContainerFactoryInvocationModel Value,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
