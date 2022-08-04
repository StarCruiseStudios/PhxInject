﻿// -----------------------------------------------------------------------------
//  <copyright file="IRenderWriter.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Render {
    using System.Text;
    using Phx.Inject.Generator.Model;

    internal class RenderWriter : IRenderWriter {
        private readonly StringBuilder sourceBuilder = new();
        private int currentIndent;
        private string indentString = "";
        private bool isBeginningOfLine = true;

        public RenderSettings Settings { get; }

        public RenderWriter(RenderSettings settings) {
            Settings = settings;
        }

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

        private class CollectionWriter : IRenderWriter.ICollectionWriter {
            private bool isFirst = true;
            private readonly IRenderWriter renderWriter;
            private readonly CollectionWriterProperties properties;

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
}
