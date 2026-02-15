// -----------------------------------------------------------------------------
// <copyright file="DiagnosticType.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;

#endregion

namespace Phx.Inject.Generator.Incremental.Diagnostics;

/// <summary>
///     Defines the type and metadata for a diagnostic that can be reported during code generation.
/// </summary>
/// <remarks>
///     <para>Immutable Singleton Pattern:</para>
///     <para>
///     All DiagnosticType instances are defined as static readonly fields with private constructors.
///     This ensures each diagnostic type exists exactly once in memory and cannot be modified,
///     making them safe for use in incremental compilation caching and concurrent scenarios.
///     </para>
///     
///     <para>Registry for Discovery:</para>
///     <para>
///     The <see cref="All"/> collection provides access to all diagnostic types for tooling that
///     needs to enumerate, validate, or document all diagnostics. This enables extensibility
///     without exposing individual fields.
///     </para>
/// </remarks>
internal sealed class DiagnosticType {
    private const string InjectionCategory = "Injection";
    private const string IdPrefix = "PHXINJECT";
    
    /// <summary> Debug information diagnostic. </summary>
    public static readonly DiagnosticType DebugMessage = new(
        IdPrefix + "9000",
        "Debug",
        InjectionCategory,
        DiagnosticSeverity.Info);
    
    /// <summary> Diagnostic for unexpected errors during generation. </summary>
    public static readonly DiagnosticType UnexpectedError = new(
        IdPrefix + "0001",
        "An unexpected error occurred.",
        InjectionCategory,
        DiagnosticSeverity.Error);

    /// <summary> Diagnostic for internal generator errors. </summary>
    public static readonly DiagnosticType InternalError = new(
        IdPrefix + "0002",
        "An internal error occurred while generating injection.",
        InjectionCategory,
        DiagnosticSeverity.Error);
    
    /// <summary>
    ///     Collection of all defined diagnostic types for enumeration and discovery.
    /// </summary>
    /// <remarks>
    ///     Use this to iterate over all diagnostics for validation, documentation generation,
    ///     or tooling that needs comprehensive diagnostic information.
    /// </remarks>
    public static IReadOnlyList<DiagnosticType> All { get; } = new[] {
        DebugMessage,
        UnexpectedError,
        InternalError
    };
    
    /// <summary> The unique diagnostic identifier. </summary>
    public string Id { get; }
    
    /// <summary> The diagnostic title. </summary>
    public string Title { get; }
    
    /// <summary> The diagnostic category. </summary>
    public string Category { get; }
    
    /// <summary> The diagnostic severity level. </summary>
    public DiagnosticSeverity Severity { get; }
    
    /// <summary> Indicates whether this diagnostic is enabled by default. </summary>
    public bool IsEnabledByDefault { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DiagnosticType"/> class.
    /// </summary>
    /// <param name="id"> The unique diagnostic identifier. </param>
    /// <param name="title"> The diagnostic title. </param>
    /// <param name="category"> The diagnostic category. </param>
    /// <param name="severity"> The diagnostic severity level. </param>
    /// <param name="isEnabledByDefault"> Indicates whether this diagnostic is enabled by default. </param>
    private DiagnosticType(
        string id,
        string title,
        string category,
        DiagnosticSeverity severity,
        bool isEnabledByDefault = true
    ) {
        Id = id;
        Title = title;
        Category = category;
        Severity = severity;
        IsEnabledByDefault = isEnabledByDefault;
    }
    
    /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
    public bool Equals(DiagnosticType? other) {
        return other is not null && Id == other.Id;
    }

    /// <inheritdoc cref="Object.GetHashCode"/>
    public override int GetHashCode() {
        return Id.GetHashCode();
    }
}