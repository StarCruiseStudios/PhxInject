// -----------------------------------------------------------------------------
// <copyright file="AutoModelMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Auto;
using Phx.Inject.Generator.Incremental.Stage2.Model.Auto;

namespace Phx.Inject.Generator.Incremental.Stage2.Mappers;

internal static class AutoModelMapper {
    public static AutoFactoryModel MapToModel(AutoFactoryMetadata metadata) {
        return new AutoFactoryModel(
            AutoFactoryType: metadata.AutoFactoryType,
            Parameters: metadata.Parameters,
            RequiredProperties: metadata.RequiredProperties.Select(p => new AutoFactoryRequiredPropertyModel(
                RequiredPropertyName: p.RequiredPropertyName,
                RequiredPropertyType: p.RequiredPropertyType
            ))
        );
    }
    
    public static AutoBuilderModel MapToModel(AutoBuilderMetadata metadata) {
        return new AutoBuilderModel(
            AutoBuilderMethodName: metadata.AutoBuilderMethodName,
            BuiltType: metadata.BuiltType,
            Parameters: metadata.Parameters
        );
    }
}
