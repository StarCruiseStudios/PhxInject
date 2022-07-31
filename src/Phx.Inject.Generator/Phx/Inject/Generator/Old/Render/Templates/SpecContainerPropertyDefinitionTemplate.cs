// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerPropertyDefinitionTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Render.Templates {
    internal record SpecContainerPropertyDefinitionTemplate(string NamespaceName, string Name) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"public {NamespaceName}.{Name} {Name} {{ get; }} = new {NamespaceName}.{Name}();");
        }
    }
}
