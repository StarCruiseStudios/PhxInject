// -----------------------------------------------------------------------------
//  <copyright file="IRenderWriter.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Templates {
    using Phx.Inject.Generator.Render;

    internal interface IRenderWriterFactory {
        IRenderWriter Build();
    }
    
    internal interface IRenderWriter {
        public GeneratorSettings Settings { get; }
        public IRenderWriter IncreaseIndent(int tabs);
        public IRenderWriter DecreaseIndent(int tabs);
        public IRenderWriter Append(string str, bool autoIndent = true);
        public IRenderWriter AppendLine(string str = "", bool autoIndent = true);
        public IRenderWriter AppendBlankLine();
        public string GetRenderedString();

        public ICollectionWriter GetCollectionWriter(CollectionWriterProperties properties);

        public interface ICollectionWriter : IDisposable {
            public IRenderWriter GetElementWriter();
        }
    }

    internal record CollectionWriterProperties(
        int Indent = 2,
        string OpeningString = "",
        bool OpenWithNewline = true,
        string ClosingString = "",
        bool CloseWithNewline = true,
        string Delimiter = ",",
        bool DelimitWithNewline = true
    ) {
        public static CollectionWriterProperties Default = new();
    }
}
