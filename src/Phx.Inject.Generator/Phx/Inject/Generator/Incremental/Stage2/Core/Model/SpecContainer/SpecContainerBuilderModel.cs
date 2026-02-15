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
///     Code generation model for a builder method in a specification container.
/// </summary>
/// <param name="BuiltType">The type constructed by the builder.</param>
/// <param name="SpecContainerBuilderMethodName">The name of the generated builder method.</param>
/// <param name="SpecBuilderMemberName">The name of the specification builder member.</param>
/// <param name="SpecBuilderMemberType">The type of the specification builder member (Method, Reference, Direct).</param>
/// <param name="Arguments">The arguments to pass to the builder.</param>
/// <param name="Location">The source location where this builder is defined.</param>
/// <remarks>
///     Represents a builder method that returns <c>IBuilder&lt;T&gt;</c> for deferred creation
///     with configuration chain. Arguments are resolved identically to factory arguments.
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
