// -----------------------------------------------------------------------------
// <copyright file="SpecificationModelMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Stage2.Model.Specification;

namespace Phx.Inject.Generator.Incremental.Stage2.Mappers;

internal static class SpecificationModelMapper {
    public static SpecificationModel MapToModel(SpecClassMetadata metadata) {
        return CreateSpecificationModel(
            metadata.SpecType,
            metadata.FactoryMethods,
            metadata.FactoryProperties,
            metadata.FactoryReferences,
            metadata.BuilderMethods,
            metadata.BuilderReferences,
            metadata.Links
        );
    }
    
    public static SpecificationModel MapToModel(SpecInterfaceMetadata metadata) {
        return CreateSpecificationModel(
            metadata.SpecInterfaceType,
            metadata.FactoryMethods,
            metadata.FactoryProperties,
            metadata.FactoryReferences,
            metadata.BuilderMethods,
            metadata.BuilderReferences,
            metadata.Links
        );
    }

    private static SpecificationModel CreateSpecificationModel(
        TypeMetadata specType,
        IEnumerable<SpecFactoryMethodMetadata> factoryMethods,
        IEnumerable<SpecFactoryPropertyMetadata> factoryProperties,
        IEnumerable<SpecFactoryReferenceMetadata> factoryReferences,
        IEnumerable<SpecBuilderMethodMetadata> builderMethods,
        IEnumerable<SpecBuilderReferenceMetadata> builderReferences,
        IEnumerable<LinkAttributeMetadata> links
    ) {
        return new SpecificationModel(
            SpecType: specType,
            FactoryMethods: factoryMethods.Select(f => new FactoryMethodModel(
                FactoryMethodName: f.FactoryMethodName,
                FactoryReturnType: f.FactoryReturnType,
                Parameters: f.Parameters
            )),
            FactoryProperties: factoryProperties.Select(f => new FactoryPropertyModel(
                FactoryPropertyName: f.FactoryPropertyName,
                FactoryReturnType: f.FactoryReturnType
            )),
            FactoryReferences: factoryReferences.Select(f => new FactoryReferenceModel(
                FactoryReferenceName: f.FactoryReferenceName,
                FactoryReturnType: f.FactoryReturnType,
                Parameters: f.Parameters
            )),
            BuilderMethods: builderMethods.Select(b => new BuilderMethodModel(
                BuilderMethodName: b.BuilderMethodName,
                BuiltType: b.BuiltType,
                Parameters: b.Parameters
            )),
            BuilderReferences: builderReferences.Select(b => new BuilderReferenceModel(
                BuilderReferenceName: b.BuilderReferenceName,
                BuiltType: b.BuiltType,
                Parameters: b.Parameters
            )),
            Links: links.Select(l => new LinkModel(
                Input: l.Input,
                Output: l.Output,
                InputLabel: l.InputLabel,
                OutputLabel: l.OutputLabel,
                InputQualifier: l.InputQualifier,
                OutputQualifier: l.OutputQualifier
            ))
        );
    }
}
