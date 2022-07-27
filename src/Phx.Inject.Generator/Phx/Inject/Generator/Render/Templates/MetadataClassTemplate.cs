// -----------------------------------------------------------------------------
//  <copyright file="MetadataClassTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Render.Templates {
    internal record MetadataClassTemplate(
            string MetadataClassName,
            string EscapedClassSource
    ) : IRenderTemplate {
        void IRenderTemplate.Render(IRenderWriter writer) {
            writer.AppendLine($"public class {MetadataClassName} {{")
                    .IncreaseIndent(1)
                    .AppendLine("public string GetRawSource() {{")
                    .IncreaseIndent(1)
                    .Append("return @\"")
                    .Append(EscapedClassSource, autoIndent: false)
                    .AppendLine("\";")
                    .DecreaseIndent(1)
                    .AppendLine("}")
                    .DecreaseIndent(1)
                    .AppendLine("}");
        }
    }
}
