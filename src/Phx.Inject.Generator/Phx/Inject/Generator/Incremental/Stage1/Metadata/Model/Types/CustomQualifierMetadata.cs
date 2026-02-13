// -----------------------------------------------------------------------------
// <copyright file="CustomQualifierMetadata.cs" company="Star Cruise Studios LLC">
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
///     Qualifier metadata representing a user-defined attribute type marked with [Qualifier].
/// </summary>
/// <param name="QualifierAttributeMetadata">
///     The metadata describing the custom qualifier attribute type.
/// </param>
/// <remarks>
///     <para>Purpose:</para>
///     <para>
///     Custom qualifiers provide type-safe, IDE-supported disambiguation of bindings. Instead of
///     string labels prone to typos, users define attributes like [Production] and [Development]
///     marked with [Qualifier], gaining compile-time safety and refactoring support.
///     </para>
///     
///     <para>Example Usage:</para>
///     <code>
///     [Qualifier]
///     public class ProductionAttribute : Attribute { }
///     
///     [Factory]
///     [Production]
///     public static IDatabase CreateProductionDb() { ... }
///     
///     [Injector]
///     public partial class MyInjector {
///         [Dependency]
///         [Production]
///         public partial IDatabase Database { get; }
///     }
///     </code>
///     
///     <para>Design Benefits:</para>
///     <list type="bullet">
///         <item>
///             <description>
///             Type safety: Compiler catches misspellings and missing qualifier declarations
///             </description>
///         </item>
///         <item>
///             <description>
///             Refactoring: Rename refactoring updates all usages consistently
///             </description>
///         </item>
///         <item>
///             <description>
///             Documentation: Qualifier attributes can carry XML docs explaining their purpose
///             </description>
///         </item>
///         <item>
///             <description>
///             Extensibility: Can add properties to qualifier attributes for metadata (though not currently supported)
///             </description>
///         </item>
///     </list>
///     
///     <para>Equality Semantics:</para>
///     <para>
///     Two custom qualifiers are equal if they reference the same attribute type (by fully-qualified
///     name). Location is included in hash code but not equality (similar inconsistency to LabelQualifierMetadata).
///     </para>
///     
///     <para>Limitations:</para>
///     <para>
///     Current implementation treats the qualifier attribute as a marker only - properties on the
///     attribute are not captured or considered during matching. This may be enhanced in the future.
///     </para>
/// </remarks>
internal record CustomQualifierMetadata(
    QualifierAttributeMetadata QualifierAttributeMetadata
) : IQualifierMetadata {
    /// <summary>
    ///     Gets the source location where the custom qualifier attribute was applied.
    /// </summary>
    public GeneratorIgnored<LocationInfo?> Location { get; } = QualifierAttributeMetadata.Location;
    
    /// <summary>
    ///     Returns a diagnostic representation showing the qualifier attribute type.
    /// </summary>
    /// <returns>
    ///     String in the format "[@QualifierType]" for error messages and logs.
    /// </returns>
    public override string ToString() {
        return $"[@{QualifierAttributeMetadata.QualifierType}]";        
    }

    /// <summary>
    ///     Determines whether another qualifier is a custom qualifier of the same attribute type.
    /// </summary>
    /// <param name="other">The qualifier to compare against, or null.</param>
    /// <returns>
    ///     True if other is a CustomQualifierMetadata referencing the same qualifier attribute type.
    ///     False for non-custom qualifiers, null, or differing attribute types.
    /// </returns>
    /// <remarks>
    ///     Compares both Location and QualifierAttributeMetadata. Including Location in equality
    ///     is inconsistent with NoQualifierMetadata and should be removed for consistency.
    /// </remarks>
    public virtual bool Equals(IQualifierMetadata? other) {
        if (other is not CustomQualifierMetadata o) return false;
        if (ReferenceEquals(this, o)) return true;
        return Location.Equals(o.Location) && QualifierAttributeMetadata.Equals(o.QualifierAttributeMetadata);
    }

    /// <summary>
    ///     Computes hash code from location and qualifier type for dictionary/hash set usage.
    /// </summary>
    /// <returns>Combined hash of Location and QualifierAttributeMetadata.</returns>
    public override int GetHashCode() {
        unchecked {
            return (Location.GetHashCode() * 397) ^ QualifierAttributeMetadata.GetHashCode();
        }
    }
}
