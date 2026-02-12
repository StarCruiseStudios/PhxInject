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
    /// <summary> The builder is a method. </summary>
    Method,
    /// <summary> The builder is a reference. </summary>
    Reference,
    /// <summary> The builder is a direct invocation. </summary>
    Direct
}
