// -----------------------------------------------------------------------------
// <copyright file="PhxInjectAttribute.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

[AttributeUsage(AttributeTargets.Class)]
public class PhxInjectAttribute : Attribute {
    public const int DefaultTabSize = 4;
    public const string DefaultGeneratedFileExtension = "generated.cs";
    public const bool DefaultNullableEnabled = true;
    public const bool DefaultAllowConstructorFactories = true;

    public int TabSize { get; set; } = DefaultTabSize;
    public string GeneratedFileExtension { get; set; } = DefaultGeneratedFileExtension;
    public bool NullableEnabled { get; set; } = DefaultNullableEnabled;
    public bool AllowConstructorFactories { get; set; } = DefaultAllowConstructorFactories;
}
