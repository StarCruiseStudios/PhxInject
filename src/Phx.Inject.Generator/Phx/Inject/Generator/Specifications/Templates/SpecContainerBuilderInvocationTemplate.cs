﻿// -----------------------------------------------------------------------------
// <copyright file="SpecContainerBuilderInvocationTemplate.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Specifications.Templates {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common.Templates;

    internal record SpecContainerBuilderInvocationTemplate(
        string SpecContainerCollectionReferenceName,
        string SpecContainerReferenceName,
        string SpecContainerBuilderMethodName,
        string BuilderTargetReferenceName,
        Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append(
                $"{SpecContainerCollectionReferenceName}.{SpecContainerReferenceName}.{SpecContainerBuilderMethodName}({BuilderTargetReferenceName}, {SpecContainerCollectionReferenceName})");
        }
    }
}
