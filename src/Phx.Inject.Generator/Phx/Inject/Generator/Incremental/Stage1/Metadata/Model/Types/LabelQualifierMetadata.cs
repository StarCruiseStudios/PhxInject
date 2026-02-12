// -----------------------------------------------------------------------------
// <copyright file="LabelQualifierMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;

/// <summary>
///     Qualifier metadata representing a string-based label annotation (e.g., @Named("primary")).
/// </summary>
/// <param name="LabelAttributeMetadata">
///     The metadata extracted from the [Label] attribute, containing the label string value.
/// </param>
/// <remarks>
///     <para>Purpose:</para>
///     <para>
///     Label qualifiers provide a lightweight way to distinguish bindings using simple string
///     identifiers. Common patterns include named instances (@Named("production") vs @Named("test")),
///     environment labels, or logical groupings like "primary" and "secondary."
///     </para>
///     
///     <para>Design Considerations:</para>
///     <list type="bullet">
///         <item>
///             <description>
///             String matching is case-sensitive - "Primary" and "primary" are distinct qualifiers
///             </description>
///         </item>
///         <item>
///             <description>
///             Empty strings are valid but discouraged (use NoQualifierMetadata instead)
///             </description>
///         </item>
///         <item>
///             <description>
///             Label values cannot be null - this is enforced by the attribute transformer
///             </description>
///         </item>
///     </list>
///     
///     <para>Equality Semantics:</para>
///     <para>
///     Two label qualifiers are equal if their label strings match exactly. Location is included
///     in the hash code but not in equality comparison (BUG: should exclude Location from hash
///     to match other qualifier types - this is a technical debt item).
///     </para>
///     
///     <para>vs. Custom Qualifiers:</para>
///     <para>
///     Use label qualifiers for simple string-based disambiguation. Use custom qualifier attributes
///     when you want type safety, IDE support, or need to associate additional data with the qualifier.
///     </para>
/// </remarks>
internal record LabelQualifierMetadata(
    LabelAttributeMetadata LabelAttributeMetadata
) : IQualifierMetadata {
    /// <summary>
    ///     Gets the source location where the [Label] attribute was declared.
    /// </summary>
    public GeneratorIgnored<LocationInfo?> Location { get; } = LabelAttributeMetadata.Location;
    
    /// <summary>
    ///     Returns a diagnostic-friendly representation showing the label value.
    /// </summary>
    /// <returns>String in the format "[Label(labelValue)]" for error messages and logs.</returns>
    public override string ToString() {
        return $"[Label({LabelAttributeMetadata.Label})]";        
    }

    /// <summary>
    ///     Determines whether another qualifier is a label with the same string value.
    /// </summary>
    /// <param name="other">The qualifier to compare against, or null.</param>
    /// <returns>
    ///     True if other is a LabelQualifierMetadata with an equal label string.
    ///     False for non-label qualifiers, null, or differing label values.
    /// </returns>
    /// <remarks>
    ///     Compares both Location and LabelAttributeMetadata for equality. Including Location
    ///     in equality is inconsistent with other metadata types and should be removed.
    /// </remarks>
    public virtual bool Equals(IQualifierMetadata? other) {
        if (other is not LabelQualifierMetadata l) return false;
        if (ReferenceEquals(this, l)) return true;
        return Location.Equals(l.Location) && LabelAttributeMetadata.Equals(l.LabelAttributeMetadata);
    }

    /// <summary>
    ///     Computes hash code from location and label value for dictionary/hash set usage.
    /// </summary>
    /// <returns>Combined hash of Location and LabelAttributeMetadata.</returns>
    public override int GetHashCode() {
        unchecked {
            return (Location.GetHashCode() * 397) ^ LabelAttributeMetadata.GetHashCode();
        }
    }
}
