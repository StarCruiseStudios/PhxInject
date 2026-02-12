// -----------------------------------------------------------------------------
// <copyright file="SpecFactoryPropertyMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata representing an analyzed specification factory property.
///     <para>
///         <strong>Factory Properties:</strong> Expose instance creation through property getters rather
///         than methods. Useful for parameterless factories or when exposing creation logic as a field.
///     </para>
///     <para>
///         <strong>Use Case:</strong> Simpler syntax for dependencies with no runtime parameters,
///         or when a factory needs to be exposed as a first-class member rather than a method.
///     </para>
/// </summary>
/// <param name="FactoryPropertyName"> The name of the factory property. </param>
/// <param name="FactoryReturnType"> The qualified type returned by the factory getter. </param>
/// <param name="FactoryAttributeMetadata"> The [Factory] attribute metadata controlling fabrication mode. </param>
/// <param name="PartialFactoryAttributeMetadata"> Optional [Partial] attribute for partial dependency satisfaction. </param>
/// <param name="Location"> The source location of the property definition. </param>
internal record SpecFactoryPropertyMetadata(
    string FactoryPropertyName,
    QualifiedTypeMetadata FactoryReturnType,
    FactoryAttributeMetadata FactoryAttributeMetadata,
    PartialAttributeMetadata? PartialFactoryAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
