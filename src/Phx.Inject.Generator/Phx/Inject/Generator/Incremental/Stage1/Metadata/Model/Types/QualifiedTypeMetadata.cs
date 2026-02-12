// -----------------------------------------------------------------------------
// <copyright file="QualifiedTypeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;

internal record QualifiedTypeMetadata(
    TypeMetadata TypeMetadata,
    IQualifierMetadata QualifierMetadata
) : ISourceCodeElement {
    public GeneratorIgnored<LocationInfo?> Location => TypeMetadata.Location;

    public override string ToString() {
        return (QualifierMetadata is not NoQualifierMetadata)
            ? $"{QualifierMetadata} {TypeMetadata}"
            : TypeMetadata.ToString();        
    }
}
