// -----------------------------------------------------------------------------
// <copyright file="IAttributeListTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms multiple attributes from a symbol into a list of attribute metadata.
/// </summary>
/// <typeparam name="TAttributeMetadata">The type of attribute metadata to produce.</typeparam>
/// <remarks>
///     <para><b>Multi-Attribute Support Pattern:</b></para>
///     <para>
///     Handles attributes that can appear multiple times on a single symbol. C# allows repeatable
///     attributes (marked with [AttributeUsage(AllowMultiple = true)]), which are common in DI
///     scenarios:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Multiple [Qualifier] attributes defining different qualification dimensions
///             </description>
///         </item>
///         <item>
///             <description>
///             Multiple [Link] attributes connecting to different dependency sources
///             </description>
///         </item>
///         <item>
///             <description>
///             Multiple [Label] attributes for multi-dimensional categorization
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Why Not IResult Return Type:</b></para>
///     <para>
///     Unlike single-attribute transformers, list transformers return EquatableList directly without
///     wrapping in IResult. Rationale:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             List transformers handle absence by returning empty list (count == 0), making null
///             semantically unnecessary
///             </description>
///         </item>
///         <item>
///             <description>
///             If any individual attribute transformation fails, the transformer can either skip that
///             attribute (partial success) or throw (all-or-nothing), depending on semantics
///             </description>
///         </item>
///         <item>
///             <description>
///             Simplifies pipeline code by avoiding nested Result unwrapping for collections
///             </description>
///         </item>
///     </list>
///     <para>
///     In practice, most list transformers report individual transformation failures via diagnostics
///     and continue processing remaining attributes, maximizing user feedback in one compilation.
///     </para>
///     
///     <para><b>Equatable List - Incremental Caching:</b></para>
///     <para>
///     Returns EquatableList rather than IEnumerable or array to support incremental generator
///     caching. EquatableList implements structural equality (contents and order matter), allowing
///     Roslyn to detect when the attribute list hasn't changed and skip downstream transformation.
///     Standard collections use reference equality, which breaks caching.
///     </para>
///     
///     <para><b>Roslyn Attribute Ordering Guarantee:</b></para>
///     <para>
///     ISymbol.GetAttributes() returns attributes in source declaration order. This ordering is
///     preserved through transformation, which matters for attributes where order is semantically
///     significant (e.g., first [Qualifier] wins for conflict resolution).
///     </para>
///     
///     <para><b>Performance - Batch Processing:</b></para>
///     <para>
///     List transformers process all matching attributes in a single pass over GetAttributes(),
///     amortizing the enumeration cost. More efficient than calling single-attribute transformer
///     repeatedly, especially when the attribute count is typically small (1-3).
///     </para>
/// </remarks>
internal interface IAttributeListTransformer<TAttributeMetadata> : IAttributeChecker where TAttributeMetadata : IAttributeElement {
    /// <summary>
    ///     Transforms all matching attributes on the target symbol into a list.
    /// </summary>
    /// <param name="targetSymbol">The symbol with the attributes.</param>
    /// <returns>An equatable list of attribute metadata.</returns>
    /// <remarks>
    ///     <para>
    ///     Returns empty list if no matching attributes found. Never returns null. List preserves
    ///     attribute declaration order from source code.
    ///     </para>
    /// </remarks>
    EquatableList<TAttributeMetadata> Transform(ISymbol targetSymbol);
}