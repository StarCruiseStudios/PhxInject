// -----------------------------------------------------------------------------
// <copyright file="InjectorChildProviderMetadata.cs" company="Star Cruise Studios LLC">
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

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;

/// <summary>
///     Metadata representing an analyzed child injector provider method.
/// </summary>
/// <param name="ChildProviderMethodName"> The name of the child provider method. </param>
/// <param name="ChildInjectorType"> The type metadata of the child injector. </param>
/// <param name="Parameters"> The list of parameters required to create the child injector. </param>
/// <param name="ChildInjectorAttribute"> The [ChildInjector] attribute metadata. </param>
/// <param name="Location"> The source location of the provider definition. </param>
internal record InjectorChildProviderMetadata(
    string ChildProviderMethodName,
    TypeMetadata ChildInjectorType,
    EquatableList<TypeMetadata> Parameters,
    ChildInjectorAttributeMetadata ChildInjectorAttribute,
    GeneratorIgnored<LocationInfo?> Location
): ISourceCodeElement { }
