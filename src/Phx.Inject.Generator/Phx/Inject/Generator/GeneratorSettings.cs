// -----------------------------------------------------------------------------
//  <copyright file="GeneratorSettings.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator;

internal class GeneratorSettings {
    public string Name { get; }
    public Location Location { get; }
    public int TabSize { get; }
    public string GeneratedFileExtension { get; }
    public bool NullableEnabled { get; }
    public bool AllowConstructorFactories { get; }
    public PhxInjectAttributeMetadata? Metadata { get; }

    public GeneratorSettings(
        int? tabSize = null,
        string? generatedFileExtension = null,
        bool? nullableEnabled = null,
        bool? allowConstructorFactories = null,
        PhxInjectAttributeMetadata? metadata = null
    ) {
        Name = metadata?.AttributedSymbol.ToString() ?? "Default";
        Location = metadata?.Location ?? Location.None;
        TabSize = tabSize ?? PhxInjectAttribute.DefaultTabSize;
        GeneratedFileExtension = generatedFileExtension ?? PhxInjectAttribute.DefaultGeneratedFileExtension;
        NullableEnabled = nullableEnabled ?? PhxInjectAttribute.DefaultNullableEnabled;
        AllowConstructorFactories = allowConstructorFactories ?? PhxInjectAttribute.DefaultAllowConstructorFactories;
        Metadata = metadata;
    }
}
