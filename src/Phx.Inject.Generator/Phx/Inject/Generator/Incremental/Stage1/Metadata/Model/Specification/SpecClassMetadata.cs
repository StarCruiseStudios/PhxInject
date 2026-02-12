// -----------------------------------------------------------------------------
// <copyright file="SpecClassMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata representing an analyzed specification class.
///     <para>
///         <strong>Specification Pattern:</strong> Defines HOW dependencies are constructed, separate from
///         the injector which defines WHAT is exposed. This separation enables reusability, composability,
///         and clear separation of concerns between construction logic and API surface.
///     </para>
///     <para>
///         <strong>Factory vs Builder:</strong>
///         <list type="bullet">
///             <item>Factories create new instances and return non-void types</item>
///             <item>Builders configure existing instances and return void</item>
///             <item>References wrap existing methods as delegates for reuse</item>
///         </list>
///     </para>
/// </summary>
/// <param name="SpecType"> The type metadata of the specification class. </param>
/// <param name="FactoryMethods"> Factory methods that create and return new instances. </param>
/// <param name="FactoryProperties"> Factory properties that expose instance creation via getters. </param>
/// <param name="FactoryReferences"> Wrapped factory methods exposed as Func delegates. </param>
/// <param name="BuilderMethods"> Builder methods that configure existing instances (void return). </param>
/// <param name="BuilderReferences"> Wrapped builder methods exposed as Action delegates. </param>
/// <param name="Links"> Link attributes that connect this specification to injectors. </param>
/// <param name="SpecAttributeMetadata"> The [Specification] attribute metadata. </param>
/// <param name="Location"> The source location of the class definition. </param>
internal record SpecClassMetadata(
    TypeMetadata SpecType,
    EquatableList<SpecFactoryMethodMetadata> FactoryMethods,
    EquatableList<SpecFactoryPropertyMetadata> FactoryProperties,
    EquatableList<SpecFactoryReferenceMetadata> FactoryReferences,
    EquatableList<SpecBuilderMethodMetadata> BuilderMethods,
    EquatableList<SpecBuilderReferenceMetadata> BuilderReferences,
    EquatableList<LinkAttributeMetadata> Links,
    SpecificationAttributeMetadata SpecAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
