// -----------------------------------------------------------------------------
// <copyright file="IQualifierMetadata.cs" company="Star Cruise Studios LLC">
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
///     Marker interface for metadata representing dependency injection qualifiers that
///     distinguish multiple bindings of the same type.
/// </summary>
/// <remarks>
///     <para><b>Design Role:</b></para>
///     <para>
///     Qualifiers solve the "multiple bindings" problem in dependency injection. When you need
///     several implementations of the same interface (e.g., @Primary and @Backup database connections),
///     qualifiers tag each binding to make it uniquely identifiable during resolution.
///     </para>
///     
///     <para><b>Implementations:</b></para>
///     <list type="bullet">
///         <item>
///             <term>NoQualifierMetadata:</term>
///             <description>Singleton representing the absence of a qualifier (default case)</description>
///         </item>
///         <item>
///             <term>LabelQualifierMetadata:</term>
///             <description>String-based qualifier like @Named("production") or @Labeled("cache")</description>
///         </item>
///         <item>
///             <term>CustomQualifierMetadata:</term>
///             <description>User-defined attribute type marked with @Qualifier</description>
///         </item>
///     </list>
///     
///     <para><b>Equality Contract:</b></para>
///     <para>
///     Implementers must provide value-based equality that considers only semantic content,
///     excluding Location. Two qualifiers are equal if they represent the same disambiguation
///     constraint (same label string, same custom attribute type, or both being "no qualifier").
///     </para>
///     
///     <para><b>Immutability Requirement:</b></para>
///     <para>
///     All implementations must be immutable records to serve as safe cache keys in Roslyn's
///     incremental compilation pipeline. Mutating a qualifier would break incremental caching.
///     </para>
/// </remarks>
internal interface IQualifierMetadata : ISourceCodeElement, IEquatable<IQualifierMetadata> { }