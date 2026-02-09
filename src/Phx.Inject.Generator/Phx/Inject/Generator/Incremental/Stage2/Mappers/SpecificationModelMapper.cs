// -----------------------------------------------------------------------------
// <copyright file="SpecificationModelMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Auto;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Stage2.Model;

namespace Phx.Inject.Generator.Incremental.Stage2.Mappers;

internal static class SpecificationModelMapper {
    /// <summary>
    /// Map a SpecClassMetadata to a unified SpecificationModel.
    /// </summary>
    public static SpecificationModel MapSpecClass(SpecClassMetadata metadata) {
        var instantiationMode = metadata.SpecAttributeMetadata?.InstantiationMode switch {
            Stage1.Metadata.Attributes.SpecInstantiationMode.Static => SpecInstantiationMode.Static,
            Stage1.Metadata.Attributes.SpecInstantiationMode.Instantiated => SpecInstantiationMode.Instantiated,
            _ => SpecInstantiationMode.Static
        };

        return new SpecificationModel(
            SpecType: metadata.SpecType,
            InstantiationMode: instantiationMode,
            Factories: MapFactories(metadata.FactoryMethods, metadata.FactoryProperties, metadata.FactoryReferences),
            Builders: MapBuilders(metadata.BuilderMethods, metadata.BuilderReferences),
            Links: MapLinks(metadata.Links)
        );
    }

    /// <summary>
    /// Map a SpecInterfaceMetadata to a unified SpecificationModel.
    /// </summary>
    public static SpecificationModel MapSpecInterface(SpecInterfaceMetadata metadata) {
        return new SpecificationModel(
            SpecType: metadata.SpecInterfaceType,
            InstantiationMode: SpecInstantiationMode.Instantiated,
            Factories: MapFactories(metadata.FactoryMethods, metadata.FactoryProperties, metadata.FactoryReferences),
            Builders: MapBuilders(metadata.BuilderMethods, metadata.BuilderReferences),
            Links: MapLinks(metadata.Links)
        );
    }

    /// <summary>
    /// Map an InjectorDependencyInterfaceMetadata to a unified SpecificationModel.
    /// </summary>
    public static SpecificationModel MapInjectorDependency(InjectorDependencyInterfaceMetadata metadata) {
        return new SpecificationModel(
            SpecType: metadata.InjectorDependencyInterfaceType,
            InstantiationMode: SpecInstantiationMode.Dependency,
            Factories: MapFactories(metadata.FactoryMethods, metadata.FactoryProperties, Enumerable.Empty<SpecFactoryReferenceMetadata>()),
            Builders: Enumerable.Empty<BuilderModel>(), // Dependencies cannot have builders
            Links: Enumerable.Empty<LinkModel>()
        );
    }

    /// <summary>
    /// Map AutoFactoryMetadata to a unified SpecificationModel (as auto-generated spec).
    /// </summary>
    public static SpecificationModel MapAutoFactory(AutoFactoryMetadata metadata) {
        var factory = new FactoryModel(
            FactoryMemberName: $"Create{metadata.AutoFactoryType.TypeMetadata.BaseTypeName}",
            ReturnType: metadata.AutoFactoryType,
            Parameters: metadata.Parameters,
            MemberType: FactoryMemberType.Method,
            FabricationMode: metadata.AutoFactoryAttributeMetadata.FabricationMode switch {
                Stage1.Metadata.Attributes.FabricationMode.Recurrent => FabricationMode.Recurrent,
                Stage1.Metadata.Attributes.FabricationMode.Scoped => FabricationMode.Scoped,
                Stage1.Metadata.Attributes.FabricationMode.Container => FabricationMode.Container,
                Stage1.Metadata.Attributes.FabricationMode.ContainerScoped => FabricationMode.ContainerScoped,
                _ => FabricationMode.Recurrent
            },
            RequiredProperties: metadata.RequiredProperties.Select(p => new RequiredPropertyModel(
                PropertyName: p.RequiredPropertyName,
                PropertyType: p.RequiredPropertyType
            )),
            IsPartial: false
        );

        return new SpecificationModel(
            SpecType: metadata.AutoFactoryType.TypeMetadata,
            InstantiationMode: SpecInstantiationMode.Static,
            Factories: new[] { factory },
            Builders: Enumerable.Empty<BuilderModel>(),
            Links: Enumerable.Empty<LinkModel>()
        );
    }

    /// <summary>
    /// Map AutoBuilderMetadata to a unified SpecificationModel (as auto-generated spec).
    /// </summary>
    public static SpecificationModel MapAutoBuilder(AutoBuilderMetadata metadata) {
        var builder = new BuilderModel(
            BuilderMemberName: metadata.AutoBuilderMethodName,
            BuiltType: metadata.BuiltType,
            Parameters: metadata.Parameters,
            MemberType: BuilderMemberType.Method
        );

        return new SpecificationModel(
            SpecType: metadata.BuiltType.TypeMetadata,
            InstantiationMode: SpecInstantiationMode.Static,
            Factories: Enumerable.Empty<FactoryModel>(),
            Builders: new[] { builder },
            Links: Enumerable.Empty<LinkModel>()
        );
    }

    private static IEnumerable<FactoryModel> MapFactories(
        IEnumerable<SpecFactoryMethodMetadata> methods,
        IEnumerable<SpecFactoryPropertyMetadata> properties,
        IEnumerable<SpecFactoryReferenceMetadata> references
    ) {
        var factoryMethods = methods.Select(m => new FactoryModel(
            FactoryMemberName: m.FactoryMethodName,
            ReturnType: m.FactoryReturnType,
            Parameters: m.Parameters,
            MemberType: FactoryMemberType.Method,
            FabricationMode: m.FactoryAttributeMetadata.FabricationMode switch {
                Stage1.Metadata.Attributes.FabricationMode.Recurrent => FabricationMode.Recurrent,
                Stage1.Metadata.Attributes.FabricationMode.Scoped => FabricationMode.Scoped,
                Stage1.Metadata.Attributes.FabricationMode.Container => FabricationMode.Container,
                Stage1.Metadata.Attributes.FabricationMode.ContainerScoped => FabricationMode.ContainerScoped,
                _ => FabricationMode.Recurrent
            },
            RequiredProperties: Enumerable.Empty<RequiredPropertyModel>(),
            IsPartial: m.PartialFactoryAttributeMetadata != null
        ));

        var factoryProperties = properties.Select(p => new FactoryModel(
            FactoryMemberName: p.FactoryPropertyName,
            ReturnType: p.FactoryReturnType,
            Parameters: Enumerable.Empty<Stage1.Metadata.Types.QualifiedTypeMetadata>(),
            MemberType: FactoryMemberType.Property,
            FabricationMode: p.FactoryAttributeMetadata.FabricationMode switch {
                Stage1.Metadata.Attributes.FabricationMode.Recurrent => FabricationMode.Recurrent,
                Stage1.Metadata.Attributes.FabricationMode.Scoped => FabricationMode.Scoped,
                Stage1.Metadata.Attributes.FabricationMode.Container => FabricationMode.Container,
                Stage1.Metadata.Attributes.FabricationMode.ContainerScoped => FabricationMode.ContainerScoped,
                _ => FabricationMode.Recurrent
            },
            RequiredProperties: Enumerable.Empty<RequiredPropertyModel>(),
            IsPartial: p.PartialFactoryAttributeMetadata != null
        ));

        var factoryReferences = references.Select(r => new FactoryModel(
            FactoryMemberName: r.FactoryReferenceName,
            ReturnType: r.FactoryReturnType,
            Parameters: r.Parameters,
            MemberType: FactoryMemberType.FieldReference,
            FabricationMode: r.FactoryAttributeMetadata.FabricationMode switch {
                Stage1.Metadata.Attributes.FabricationMode.Recurrent => FabricationMode.Recurrent,
                Stage1.Metadata.Attributes.FabricationMode.Scoped => FabricationMode.Scoped,
                Stage1.Metadata.Attributes.FabricationMode.Container => FabricationMode.Container,
                Stage1.Metadata.Attributes.FabricationMode.ContainerScoped => FabricationMode.ContainerScoped,
                _ => FabricationMode.Recurrent
            },
            RequiredProperties: Enumerable.Empty<RequiredPropertyModel>(),
            IsPartial: r.PartialFactoryAttributeMetadata != null
        ));

        return factoryMethods.Concat(factoryProperties).Concat(factoryReferences);
    }

    private static IEnumerable<BuilderModel> MapBuilders(
        IEnumerable<SpecBuilderMethodMetadata> methods,
        IEnumerable<SpecBuilderReferenceMetadata> references
    ) {
        var builderMethods = methods.Select(m => new BuilderModel(
            BuilderMemberName: m.BuilderMethodName,
            BuiltType: m.BuiltType,
            Parameters: m.Parameters,
            MemberType: BuilderMemberType.Method
        ));

        var builderReferences = references.Select(r => new BuilderModel(
            BuilderMemberName: r.BuilderReferenceName,
            BuiltType: r.BuiltType,
            Parameters: r.Parameters,
            MemberType: BuilderMemberType.FieldReference
        ));

        return builderMethods.Concat(builderReferences);
    }

    private static IEnumerable<LinkModel> MapLinks(
        IEnumerable<Stage1.Metadata.Attributes.LinkAttributeMetadata> links
    ) {
        return links.Select(l => new LinkModel(
            Input: l.Input,
            Output: l.Output,
            InputLabel: l.InputLabel,
            OutputLabel: l.OutputLabel,
            InputQualifier: l.InputQualifier,
            OutputQualifier: l.OutputQualifier
        ));
    }
}
