// -----------------------------------------------------------------------------
// <copyright file="SpecInterfaceMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata representing an analyzed specification interface.
///     <para>
///         <strong>Specification Pattern:</strong> Interface specifications define abstract construction
///         contracts, enabling polymorphic injection strategies where multiple implementations can provide
///         the same dependency construction logic.
///     </para>
///     <para>
///         Unlike classes, interface specifications are pure contracts and must be implemented by concrete
///         specification classes to provide actual factory/builder logic.
///     </para>
/// </summary>
/// <param name="SpecInterfaceType"> The type metadata of the specification interface. </param>
/// <param name="FactoryMethods"> Factory method contracts that create and return new instances. </param>
/// <param name="FactoryProperties"> Factory property contracts that expose instance creation. </param>
/// <param name="FactoryReferences"> Factory reference contracts wrapped as Func delegates. </param>
/// <param name="BuilderMethods"> Builder method contracts that configure existing instances. </param>
/// <param name="BuilderReferences"> Builder reference contracts wrapped as Action delegates. </param>
/// <param name="Links"> Link attributes connecting this specification to injectors. </param>
/// <param name="SpecAttributeMetadata"> The [Specification] attribute metadata. </param>
/// <param name="Location"> The source location of the interface definition. </param>
internal record SpecInterfaceMetadata(
    TypeMetadata SpecInterfaceType,
    EquatableList<SpecFactoryMethodMetadata> FactoryMethods,
    EquatableList<SpecFactoryPropertyMetadata> FactoryProperties,
    EquatableList<SpecFactoryReferenceMetadata> FactoryReferences,
    EquatableList<SpecBuilderMethodMetadata> BuilderMethods,
    EquatableList<SpecBuilderReferenceMetadata> BuilderReferences,
    EquatableList<LinkAttributeMetadata> Links,
    SpecificationAttributeMetadata SpecAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
