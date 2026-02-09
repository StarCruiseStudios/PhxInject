// -----------------------------------------------------------------------------
// <copyright file="SpecificationModelMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Stage2.Model.Specification;

namespace Phx.Inject.Generator.Incremental.Stage2.Mappers;

internal static class SpecificationModelMapper {
    public static SpecificationModel MapToModel(SpecClassMetadata metadata) {
        return new SpecificationModel(
            SpecType: metadata.SpecType,
            FactoryMethods: metadata.FactoryMethods.Select(f => new FactoryMethodModel(
                FactoryMethodName: f.FactoryMethodName,
                FactoryReturnType: f.FactoryReturnType,
                Parameters: f.Parameters
            )),
            FactoryProperties: metadata.FactoryProperties.Select(f => new FactoryPropertyModel(
                FactoryPropertyName: f.FactoryPropertyName,
                FactoryReturnType: f.FactoryReturnType
            )),
            FactoryReferences: metadata.FactoryReferences.Select(f => new FactoryReferenceModel(
                FactoryReferenceName: f.FactoryReferenceName,
                FactoryReturnType: f.FactoryReturnType,
                Parameters: f.Parameters
            )),
            BuilderMethods: metadata.BuilderMethods.Select(b => new BuilderMethodModel(
                BuilderMethodName: b.BuilderMethodName,
                BuiltType: b.BuiltType,
                Parameters: b.Parameters
            )),
            BuilderReferences: metadata.BuilderReferences.Select(b => new BuilderReferenceModel(
                BuilderReferenceName: b.BuilderReferenceName,
                BuiltType: b.BuiltType,
                Parameters: b.Parameters
            )),
            Links: metadata.Links.Select(l => new LinkModel(
                Input: l.Input,
                Output: l.Output,
                InputLabel: l.InputLabel,
                OutputLabel: l.OutputLabel,
                InputQualifier: l.InputQualifier,
                OutputQualifier: l.OutputQualifier
            ))
        );
    }
    
    public static SpecificationModel MapToModel(SpecInterfaceMetadata metadata) {
        return new SpecificationModel(
            SpecType: metadata.SpecInterfaceType,
            FactoryMethods: metadata.FactoryMethods.Select(f => new FactoryMethodModel(
                FactoryMethodName: f.FactoryMethodName,
                FactoryReturnType: f.FactoryReturnType,
                Parameters: f.Parameters
            )),
            FactoryProperties: metadata.FactoryProperties.Select(f => new FactoryPropertyModel(
                FactoryPropertyName: f.FactoryPropertyName,
                FactoryReturnType: f.FactoryReturnType
            )),
            FactoryReferences: metadata.FactoryReferences.Select(f => new FactoryReferenceModel(
                FactoryReferenceName: f.FactoryReferenceName,
                FactoryReturnType: f.FactoryReturnType,
                Parameters: f.Parameters
            )),
            BuilderMethods: metadata.BuilderMethods.Select(b => new BuilderMethodModel(
                BuilderMethodName: b.BuilderMethodName,
                BuiltType: b.BuiltType,
                Parameters: b.Parameters
            )),
            BuilderReferences: metadata.BuilderReferences.Select(b => new BuilderReferenceModel(
                BuilderReferenceName: b.BuilderReferenceName,
                BuiltType: b.BuiltType,
                Parameters: b.Parameters
            )),
            Links: metadata.Links.Select(l => new LinkModel(
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
