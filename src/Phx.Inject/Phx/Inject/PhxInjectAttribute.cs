// -----------------------------------------------------------------------------
// <copyright file="PhxInjectAttribute.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary>
///     Configures global code generation settings for the PhxInject source generator.
/// </summary>
/// <remarks>
/// This attribute is applied to a class in the consuming project to configure how the
/// PhxInject source generator produces code. It controls formatting, file naming, and
/// nullability settings for all generated injector implementations.
///
/// ## Usage
///
/// Apply this attribute to any class in your project to configure generation settings:
///
/// - Only one <see cref="PhxInjectAttribute"/> should exist per project
/// - Settings apply globally to all generated code in the assembly
/// - If not specified, default values are used for all settings
///
/// ## Example
///
/// <code>
/// [PhxInject(TabSize = 2, NullableEnabled = true)]
/// public class InjectorConfiguration { }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class PhxInjectAttribute : Attribute {
    /// <summary>
    ///     The default number of spaces used for indentation in generated code.
    /// </summary>
    public const int DefaultTabSize = 4;

    /// <summary>
    ///     The default file extension suffix for generated source files.
    /// </summary>
    public const string DefaultGeneratedFileExtension = "generated.cs";

    /// <summary>
    ///     The default setting for nullable reference type annotations in generated code.
    /// </summary>
    public const bool DefaultNullableEnabled = true;

    /// <summary>
    ///     Gets or sets the number of spaces used for indentation in generated code.
    /// </summary>
    /// <value>
    ///     The number of spaces per indentation level. The default is <c>4</c>.
    /// </value>
    public int TabSize { get; set; } = DefaultTabSize;

    /// <summary>
    ///     Gets or sets the file extension suffix for generated source files.
    /// </summary>
    /// <value>
    ///     The file extension suffix (e.g., "generated.cs", "g.cs"). 
    ///     The default is <c>"generated.cs"</c>.
    /// </value>
    public string GeneratedFileExtension { get; set; } = DefaultGeneratedFileExtension;

    /// <summary>
    ///     Gets or sets a value that indicates whether nullable reference type annotations 
    ///     are included in generated code.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if generated code includes nullable annotations 
    ///     (<c>#nullable enable</c>); otherwise, <see langword="false" />.
    ///     The default is <see langword="true" />.
    /// </value>
    public bool NullableEnabled { get; set; } = DefaultNullableEnabled;
}
