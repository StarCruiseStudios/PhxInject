// -----------------------------------------------------------------------------
// <copyright file="InjectorDependencyFactoryMethodMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata representing an analyzed injector dependency factory method.
///     <para>
///         Dependency Factory Method: Defines a parameterless factory in an
///         InjectorDependency interface, representing a dependency that the parent injector must provide
///         to child injectors.
///     </para>
///     <para>
///         Parameterless Requirement: Must have no parameters because the parent provides
///         these dependencies independently of child context. The parent's dependency graph satisfies all
///         requirements internally.
///     </para>
/// </summary>
/// <param name="FactoryMethodName"> The name of the dependency factory method. </param>
/// <param name="FactoryReturnType"> The type that the parent must be able to provide. </param>
/// <param name="FactoryAttributeMetadata"> The [Factory] attribute metadata. </param>
/// <param name="Location"> The source location of the method definition. </param>
internal record InjectorDependencyFactoryMethodMetadata(
    string FactoryMethodName,
    QualifiedTypeMetadata FactoryReturnType,
    FactoryAttributeMetadata FactoryAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
