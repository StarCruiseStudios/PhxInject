// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryRequiredPropertyMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Auto;

/// <summary>
///     Metadata representing a required property for an auto-generated factory.
///     <para>
///         <strong>C# Required Properties:</strong> Properties marked with the 'required' modifier in C# 11+
///         that MUST be initialized during object construction. The compiler enforces this at call sites.
///     </para>
///     <para>
///         <strong>Initialization Strategy:</strong> The generator must ensure these properties are satisfied
///         in the generated factory method, either by:
///         <list type="number">
///             <item>Resolving the property type from the dependency graph and setting it post-construction</item>
///             <item>Using object initializer syntax: <c>new MyClass { RequiredProp = value }</c></item>
///         </list>
///     </para>
///     <para>
///         <strong>Design Rationale:</strong> Enables constructor injection for dependencies while using
///         required properties for secondary configuration, maintaining clean separation of concerns.
///     </para>
/// </summary>
/// <param name="RequiredPropertyName"> The name of the required property from the class definition. </param>
/// <param name="RequiredPropertyType"> The property type that must be resolved from the dependency graph. </param>
/// <param name="Location"> The source location of the property definition. </param>
internal record AutoFactoryRequiredPropertyMetadata(
    string RequiredPropertyName,
    QualifiedTypeMetadata RequiredPropertyType,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }