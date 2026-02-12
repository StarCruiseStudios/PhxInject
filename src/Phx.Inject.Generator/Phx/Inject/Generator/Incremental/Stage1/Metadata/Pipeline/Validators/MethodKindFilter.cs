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
///     <para><b>Design Purpose - Distinguishing Method Varieties:</b></para>
///     <para>
///     Roslyn's IMethodSymbol represents many constructs beyond ordinary methods: property getters/setters,
///     event add/remove handlers, operator overloads, constructors, finalizers, etc. In DI contexts,
///     most patterns want only ordinary methods. This enum provides precise filtering.
///     </para>
///     
///     <para><b>WHY This Filter Exists:</b></para>
///     <para>
///     Consider a user writing a specification class with factory methods. They mark methods with
///     @Factory attributes. If validation accepts all IMethodSymbol instances, we'd incorrectly
///     process property getters, operators, etc. as factory methods, producing nonsensical generated
///     code. Method kind filtering prevents this category error.
///     </para>
///     
///     <para><b>DI Patterns by Method Kind:</b></para>
///     <list type="bullet">
///         <item>
///             <term>Ordinary Methods (MethodKind.Ordinary):</term>
///             <description>
///             Factory methods, provider methods, builder methods. The primary target for most DI
///             validation. Clear intent and semantics.
///             </description>
///         </item>
///         <item>
///             <term>Constructors (MethodKind.Constructor):</term>
///             <description>
///             Constructor injection pattern. Parameters become required dependencies. Validation
///             ensures constructors are accessible and parameters can be satisfied by dependency graph.
///             </description>
///         </item>
///         <item>
///             <term>Property Getters (MethodKind.PropertyGet):</term>
///             <description>
///             Rarely used directly in DI, but may appear in advanced scenarios like lazy property
///             initialization or computed dependencies. Usually validated as properties, not methods.
///             </description>
///         </item>
///         <item>
///             <term>Property Setters (MethodKind.PropertySet):</term>
///             <description>
///             Property injection targets. Validated as setters to enable post-construction injection.
///             Usually validated as properties, not methods.
///             </description>
///         </item>
///     </list>
///     
///     <para><b>What Malformed Code Gets Caught:</b></para>
///     <list type="bullet">
///         <item>
///             <description>
///             @Factory attribute on property getters (user probably meant a method, got auto-property syntax)
///             </description>
///         </item>
///         <item>
///             <description>
///             @Inject attribute on event handlers (events are not dependency injection targets)
///             </description>
///         </item>
///         <item>
///             <description>
///             Attempting to use operator overloads as factory methods (operators have fixed signatures, incompatible with DI)
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Why Not Just Check MethodKind.Ordinary Everywhere:</b></para>
///     <para>
///     Different DI patterns have different method kind requirements. Constructor injection specifically
///     needs constructors. Some advanced patterns might want to intercept property access via getters.
///     This enum provides flexibility while maintaining type safety. The 'Any' option allows validators
///     to explicitly opt out of filtering when appropriate.
///     </para>
///     
///     <para><b>Syntax vs Symbol Distinction:</b></para>
///     <para>
///     MethodKind is semantic information unavailable at syntax level. Syntax cannot distinguish
///     a MethodDeclarationSyntax from a property getter's method body. This is why method kind
///     filtering only happens in symbol validation, never syntax validation.
///     </para>
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
    ///     <para><b>Exact Matching Only:</b></para>
    ///     <para>
    ///     Each filter value matches exactly one MethodKind, except 'Any' which matches all.
    ///     We don't group related kinds (e.g., treating constructors and static constructors
    ///     as equivalent) because DI patterns typically care about the distinction.
    ///     </para>
    ///     
    ///     <para><b>Unhandled Method Kinds:</b></para>
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
