// -----------------------------------------------------------------------------
//  <copyright file="IRenderWriter.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Templates {

    internal delegate IRenderWriter CreateRenderWriter();

    internal interface IRenderWriter {
        RenderSettings Settings { get; }
        IRenderWriter IncreaseIndent(int tabs);
        IRenderWriter DecreaseIndent(int tabs);
        IRenderWriter Append(string str, bool autoIndent = true);
        IRenderWriter AppendLine(string str = "", bool autoIndent = true);
        IRenderWriter AppendBlankLine();
        string GetRenderedString();
    }
}
