// -----------------------------------------------------------------------------
// <copyright file="SpecContainerFactoryModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Util;
using Phx.Inject;

namespace Phx.Inject.Generator.Incremental.Stage2.Model.SpecContainer;

internal record SpecContainerFactoryModel(
    QualifiedTypeMetadata ReturnType,
    string SpecContainerFactoryMethodName,
    string SpecFactoryMemberName,
    SpecFactoryMemberType SpecFactoryMemberType,
    FabricationMode FabricationMode,
    IEnumerable<SpecContainerFactoryInvocationModel> Arguments,
    IEnumerable<SpecContainerFactoryRequiredPropertyModel> RequiredProperties,
    GeneratorIgnored<Location> Location
) : ISourceCodeElement;

internal enum SpecFactoryMemberType {
    Method,
    Property,
    Reference,
    Constructor
}
