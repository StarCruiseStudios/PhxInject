// -----------------------------------------------------------------------------
// <copyright file="SpecContainerFactoryModel.cs" company="Star Cruise Studios LLC">
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
///     Model representing a factory method in a specification container.
/// </summary>
/// <param name="ReturnType"> The return type of the factory method. </param>
/// <param name="SpecContainerFactoryMethodName"> The name of the generated factory method. </param>
/// <param name="SpecFactoryMemberName"> The name of the specification factory member. </param>
/// <param name="SpecFactoryMemberType"> The type of the specification factory member. </param>
/// <param name="FabricationMode"> The fabrication mode for creating instances. </param>
/// <param name="Arguments"> The arguments to pass to the factory. </param>
/// <param name="RequiredProperties"> The required properties to set on the created instance. </param>
/// <param name="Location"> The source location where this factory is defined. </param>
internal record SpecContainerFactoryModel(
    QualifiedTypeMetadata ReturnType,
    string SpecContainerFactoryMethodName,
    string SpecFactoryMemberName,
    SpecFactoryMemberType SpecFactoryMemberType,
    FabricationMode FabricationMode,
    IEnumerable<SpecContainerFactoryInvocationModel> Arguments,
    IEnumerable<SpecContainerFactoryRequiredPropertyModel> RequiredProperties,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;

/// <summary>
///     Specifies the type of specification factory member.
/// </summary>
internal enum SpecFactoryMemberType {
    /// <summary> The factory is a method. </summary>
    Method,
    /// <summary> The factory is a property. </summary>
    Property,
    /// <summary> The factory is a reference. </summary>
    Reference,
    /// <summary> The factory is a constructor. </summary>
    Constructor
}
