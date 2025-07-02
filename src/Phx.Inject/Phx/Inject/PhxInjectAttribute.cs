// -----------------------------------------------------------------------------
// <copyright file="PhxInjectAttribute.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

[AttributeUsage(AttributeTargets.Class)]
public class PhxInjectAttribute : Attribute {
    public int TabSize { get; set; } = 4;
    public string GeneratedFileExtension { get; set; } = "generated.cs";
    public bool NullableEnabled { get; set; } = true;
    public bool AllowConstructorFactories { get; set; } = true;
}
