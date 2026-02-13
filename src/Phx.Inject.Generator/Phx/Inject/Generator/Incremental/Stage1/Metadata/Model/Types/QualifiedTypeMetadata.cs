// -----------------------------------------------------------------------------
// <copyright file="QualifiedTypeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;

/// <summary>
///     Immutable pairing of a type identity with a qualifier that distinguishes it from other
///     instances of the same type in dependency injection scenarios.
/// </summary>
/// <param name="TypeMetadata">
///     The underlying type identity (e.g., "IDatabase").
/// </param>
/// <param name="QualifierMetadata">
///     The qualifier that disambiguates this binding (e.g., @Named("primary"), @Production,
///     or NoQualifierMetadata for unqualified bindings).
/// </param>
/// <remarks>
///     <para>Design Purpose:</para>
///     <para>
///     Enables multiple bindings of the same type to coexist by associating each with a
///     distinct qualifier. For example, you might have @Primary ILogger and @Fallback ILogger
///     bindings. The qualifier becomes part of the binding's identity for dependency resolution.
///     </para>
///     
///     <para>Qualifier Semantics:</para>
///     <list type="bullet">
///         <item>
///             <description>
///             NoQualifierMetadata: Standard unqualified binding, matches injection sites without qualifiers
///             </description>
///         </item>
///         <item>
///             <description>
///             LabelQualifierMetadata: String-based qualifier like @Named("production")
///             </description>
///         </item>
///         <item>
///             <description>
///             CustomQualifierMetadata: User-defined qualifier attribute type
///             </description>
///         </item>
///     </list>
///     
///     <para>Equality Semantics:</para>
///     <para>
///     Two qualified types are equal if both their type and qualifier match. This ensures
///     that @Primary ILogger and @Secondary ILogger are treated as distinct binding keys.
///     The Location property is inherited from TypeMetadata and excluded from equality.
///     </para>
///     
///     <para>When to Use:</para>
///     <para>
///     Use QualifiedTypeMetadata whenever representing a dependency binding, injection site,
///     or provided value. Always use this instead of bare TypeMetadata in the DI domain model
///     to maintain consistent qualifier semantics throughout the pipeline.
///     </para>
/// </remarks>
internal record QualifiedTypeMetadata(
    TypeMetadata TypeMetadata,
    IQualifierMetadata QualifierMetadata
) : ISourceCodeElement {
    /// <summary>
    ///     Gets the source location from the underlying type, for diagnostic reporting.
    /// </summary>
    /// <remarks>
    ///     Delegates to TypeMetadata's location since the qualifier's location is less relevant
    ///     for error messages (users think in terms of types, not qualifiers).
    /// </remarks>
    public GeneratorIgnored<LocationInfo?> Location => TypeMetadata.Location;

    /// <summary>
    ///     Returns a human-readable representation combining qualifier and type for diagnostics.
    /// </summary>
    /// <returns>
    ///     For qualified types: "&lt;Qualifier&gt; TypeName". For unqualified: "TypeName" only.
    /// </returns>
    /// <remarks>
    ///     Used primarily in diagnostic messages and debugging output to help users identify
    ///     which specific binding is being referenced when multiple bindings of the same type exist.
    /// </remarks>
    public override string ToString() {
        return (QualifierMetadata is not NoQualifierMetadata)
            ? $"{QualifierMetadata} {TypeMetadata}"
            : TypeMetadata.ToString();        
    }
}
