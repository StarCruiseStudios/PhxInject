// -----------------------------------------------------------------------------
// <copyright file="InjectorDependencyFactoryPropertyMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata representing an analyzed injector dependency factory property.
///     <para>
///         Dependency Factory Property: Defines a parameterless factory property in an
///         InjectorDependency interface, representing a dependency exposed by the parent injector through
///         a property getter rather than a method.
///     </para>
///     <para>
///         Composition Contract: The parent must implement this property getter to provide
///         the dependency on-demand. Properties offer cleaner syntax for frequently-accessed dependencies.
///     </para>
/// </summary>
/// <param name="FactoryPropertyName"> The name of the dependency factory property. </param>
/// <param name="FactoryReturnType"> The type that the parent must provide via the property. </param>
/// <param name="FactoryAttributeMetadata"> The [Factory] attribute metadata. </param>
/// <param name="Location"> The source location of the property definition. </param>
internal record InjectorDependencyFactoryPropertyMetadata(
    string FactoryPropertyName,
    QualifiedTypeMetadata FactoryReturnType,
    FactoryAttributeMetadata FactoryAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
