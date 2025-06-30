// -----------------------------------------------------------------------------
//  <copyright file="RenderWriter.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Text;
using Phx.Inject.Generator.Templates;

namespace Phx.Inject.Generator.Render;

internal class RenderWriter : IRenderWriter {
    private readonly StringBuilder sourceBuilder = new();
    private int currentIndent;
    private string indentString = "";
    private bool isBeginningOfLine = true;

    public RenderWriter(GeneratorSettings settings) {
        Settings = settings;
    }

    public GeneratorSettings Settings { get; }

    public IRenderWriter IncreaseIndent(int tabs) {
        currentIndent += tabs * Settings.TabSize;
        indentString = "".PadRight(currentIndent);
        return this;
    }

    public IRenderWriter DecreaseIndent(int tabs) {
        currentIndent -= tabs * Settings.TabSize;
        if (currentIndent < 0) {
            currentIndent = 0;
        }

        indentString = "".PadRight(currentIndent);
        return this;
    }

    public IRenderWriter Append(string str, bool autoIndent = true) {
        if (isBeginningOfLine && autoIndent) {
            sourceBuilder.Append(indentString);
        }

        var stringToAppend = autoIndent
            ? str.Replace("\n", $"\n{indentString}")
            : str;
        sourceBuilder.Append(stringToAppend);
        isBeginningOfLine = false;
        return this;
    }

    public IRenderWriter AppendLine(string str = "", bool autoIndent = true) {
        if (isBeginningOfLine && autoIndent) {
            sourceBuilder.Append(indentString);
        }

        var stringToAppend = autoIndent
            ? str.Replace("\n", $"\n{indentString}")
            : str;
        sourceBuilder.AppendLine(stringToAppend);
        isBeginningOfLine = true;
        return this;
    }

    public IRenderWriter AppendBlankLine() {
        if (!isBeginningOfLine) {
            sourceBuilder.AppendLine();
            isBeginningOfLine = true;
        }

        sourceBuilder.AppendLine();
        return this;
    }

    public string GetRenderedString() {
        return sourceBuilder.ToString();
    }

    public IRenderWriter.ICollectionWriter GetCollectionWriter(CollectionWriterProperties properties) {
        return new CollectionWriter(this, properties);
    }

    public class Factory : IRenderWriterFactory {
        public GeneratorSettings settings { get; init; }
        public Factory(GeneratorSettings Settings) {
            settings = Settings;
        }
        public IRenderWriter Build() {
            return new RenderWriter(settings);
        }
    }

    private class CollectionWriter : IRenderWriter.ICollectionWriter {
        private readonly CollectionWriterProperties properties;
        private readonly IRenderWriter renderWriter;
        private bool isFirst = true;

        public CollectionWriter(IRenderWriter renderWriter, CollectionWriterProperties properties) {
            this.renderWriter = renderWriter;
            this.properties = properties;

            if (properties.OpenWithNewline) {
                renderWriter.AppendLine(properties.OpeningString);
            } else {
                renderWriter.Append(properties.OpeningString);
            }

            renderWriter.IncreaseIndent(properties.Indent);
        }

        public IRenderWriter GetElementWriter() {
            if (isFirst) {
                isFirst = false;
            } else {
                if (properties.DelimitWithNewline) {
                    renderWriter.AppendLine(properties.Delimiter);
                } else {
                    renderWriter.Append(properties.Delimiter);
                }
            }

            return renderWriter;
        }

        public void Dispose() {
            renderWriter.DecreaseIndent(properties.Indent);
            if (properties.CloseWithNewline) {
                renderWriter.AppendLine(properties.ClosingString);
            } else {
                renderWriter.Append(properties.ClosingString);
            }
        }
    }
}
