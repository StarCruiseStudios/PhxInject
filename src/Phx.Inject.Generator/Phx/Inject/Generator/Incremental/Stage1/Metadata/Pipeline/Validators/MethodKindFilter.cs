// -----------------------------------------------------------------------------
// <copyright file="MethodKindFilter.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;

/// <summary>
///     Specifies method kind filters for validation in dependency injection contexts.
/// </summary>
/// <remarks>
///     Filters <c>IMethodSymbol</c> instances by <c>MethodKind</c> to distinguish ordinary methods
///     from property accessors, operators, constructors, etc. Most DI patterns target
///     <c>MethodKind.Ordinary</c> (factory/provider methods). <c>Constructor</c> enables constructor
///     injection validation. Method kind is semantic info unavailable at syntax level (symbol-only).
/// </remarks>
internal enum MethodKindFilter {
    /// <summary>
    ///     Matches ordinary methods (user-defined methods, not generated accessors/operators).
    /// </summary>
    /// <remarks>
    ///     This is the default and most common filter for DI factory methods, providers, and builders.
    ///     Excludes constructors, property accessors, event handlers, operators, and other special methods.
    /// </remarks>
    Method,
    
    /// <summary>
    ///     Matches constructor methods (instance and static constructors).
    /// </summary>
    /// <remarks>
    ///     Used for constructor injection pattern validation. Constructor parameters become
    ///     required dependencies that must be satisfied by the dependency graph.
    /// </remarks>
    Constructor,
    
    /// <summary>
    ///     Matches property getter methods (IMethodSymbol with MethodKind.PropertyGet).
    /// </summary>
    /// <remarks>
    ///     Rarely used directly. Most property validation happens via PropertyElementValidator.
    ///     Included for completeness and advanced scenarios.
    /// </remarks>
    Getter,
    
    /// <summary>
    ///     Matches property setter methods (IMethodSymbol with MethodKind.PropertySet).
    /// </summary>
    /// <remarks>
    ///     Rarely used directly. Most property validation happens via PropertyElementValidator.
    ///     Included for completeness and advanced scenarios.
    /// </remarks>
    Setter,
    
    /// <summary>
    ///     Matches any method kind (no filtering applied).
    /// </summary>
    /// <remarks>
    ///     Used when method kind is irrelevant or validated elsewhere. For example, reading metadata
    ///     from all methods regardless of kind, or when other constraints (attributes, parameters)
    ///     provide sufficient discrimination.
    /// </remarks>
    Any
}

internal static class MethodKindFilterExtensions {
    /// <summary>
    ///     Checks if a method symbol's kind matches the filter requirement.
    /// </summary>
    /// <param name="filter">The method kind filter to apply.</param>
    /// <param name="methodSymbol">The method symbol being validated.</param>
    /// <returns>
    ///     True if the method's kind matches the filter, false otherwise.
    /// </returns>
    /// <remarks>
    ///     <para>Exact Matching Only:</para>
    ///     <para>
    ///     Each filter value matches exactly one MethodKind, except 'Any' which matches all.
    ///     We don't group related kinds (e.g., treating constructors and static constructors
    ///     as equivalent) because DI patterns typically care about the distinction.
    ///     </para>
    ///     
    ///     <para>Unhandled Method Kinds:</para>
    ///     <para>
    ///     Roslyn defines many MethodKind values not represented in this filter: Destructor,
    ///     EventAdd, EventRemove, UserDefinedOperator, Conversion, etc. These are intentionally
    ///     omitted because they're never valid DI targets. If validation encounters them with
    ///     a specific filter, this method returns false (which is correct - we don't want to
    ///     process operators as factories).
    ///     </para>
    /// </remarks>
    internal static bool MethodKindMatches(this MethodKindFilter filter, IMethodSymbol methodSymbol) {
        return filter switch {
            MethodKindFilter.Method => methodSymbol.MethodKind == MethodKind.Ordinary,
            MethodKindFilter.Constructor => methodSymbol.MethodKind == MethodKind.Constructor,
            MethodKindFilter.Getter => methodSymbol.MethodKind == MethodKind.PropertyGet,
            MethodKindFilter.Setter => methodSymbol.MethodKind == MethodKind.PropertySet,
            MethodKindFilter.Any => true,
            _ => false // Unknown method kind filter, treat as no match
        };
    }
}
