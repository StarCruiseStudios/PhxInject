// -----------------------------------------------------------------------------
// <copyright file="SpecContainerBuilderModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage2.Model.SpecContainer;

internal record SpecContainerBuilderModel(
    TypeMetadata BuiltType,
    string SpecContainerBuilderMethodName,
    string SpecBuilderMemberName,
    SpecBuilderMemberType SpecBuilderMemberType,
    IEnumerable<SpecContainerFactoryInvocationModel> Arguments,
    GeneratorIgnored<Location> Location
) : ISourceCodeElement;

internal enum SpecBuilderMemberType {
    Method,
    Reference,
    Direct
}
