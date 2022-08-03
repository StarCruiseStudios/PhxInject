﻿// -----------------------------------------------------------------------------
//  <copyright file="InjectorSpecContainerCollectionReferenceDeclarationTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Injectors.Templates {
    using Microsoft.CodeAnalysis;

    internal record InjectorSpecContainerCollectionReferenceDeclarationTemplate(
            string SpecContainerCollectionTypeName,
            string SpecContainerCollectionReferenceName,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine(
                    $"private readonly {SpecContainerCollectionTypeName} {SpecContainerCollectionReferenceName};");
        }
    }
}
