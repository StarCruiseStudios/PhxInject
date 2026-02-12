// -----------------------------------------------------------------------------
// <copyright file="AttributeDataExtensions.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Util;

/// <summary>
/// Extension methods for working with Roslyn <see cref="AttributeData"/>.
/// 
/// PURPOSE:
/// - Simplifies extraction of attribute metadata during code analysis
/// - Provides type-safe access to attribute arguments (constructor and named)
/// - Handles common edge cases (null classes, missing arguments) with clear semantics
/// 
/// WHY THIS EXISTS:
/// Roslyn's AttributeData API is verbose and error-prone to use directly:
/// 1. AttributeClass can be null (for malformed code or unresolved types)
/// 2. Named arguments require manual dictionary lookups with KeyValuePair iteration
/// 3. Constructor arguments require filtering and type casting
/// 4. No built-in null-safe or type-safe accessor methods
/// 
/// These extensions provide:
/// - Fail-fast behavior (throw) for unexpected nulls (development-time errors)
/// - Convenient typed accessors for common argument types (int, bool, generic T)
/// - Predicate-based filtering for complex constructor argument scenarios
/// - Location resolution that falls back appropriately
/// 
/// COMMON PATTERNS SUPPORTED:
/// 
/// 1. Getting attribute type information:
///    var typeName = attributeData.GetFullyQualifiedName();
///    
/// 2. Reading named properties:
///    var lifetime = attributeData.GetNamedArgument&lt;string&gt;("Lifetime");
///    var isOptional = attributeData.GetNamedBoolArgument("IsOptional") ?? false;
///    
/// 3. Reading constructor arguments by type:
///    var typeArg = attributeData.GetConstructorArgument&lt;ITypeSymbol&gt;(
///        tc =&gt; tc.Kind == TypedConstantKind.Type);
///    
/// 4. Reading multiple constructor arguments:
///    var types = attributeData.GetConstructorArguments&lt;ITypeSymbol&gt;(
///        tc =&gt; tc.Kind == TypedConstantKind.Type);
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - Uses LINQ for clarity; attribute processing is not performance-critical
/// - Occurs once per attribute during initial parse, results are cached
/// - Predicate-based methods support lazy evaluation and early exit
/// </summary>
internal static class AttributeDataExtensions {
    /// <summary>
    ///     Gets the named type symbol for the attribute.
    /// </summary>
    /// <remarks>
    /// FAIL-FAST DESIGN:
    /// Throws if AttributeClass is null rather than returning null. This is intentional because:
    /// - Null AttributeClass indicates a semantic model error (unresolved type)
    /// - Such errors should be surfaced immediately rather than propagated as null
    /// - Callers should not need to handle null; if it occurs, it's a bug or unresolved reference
    /// </remarks>
    /// <param name="attributeData"> The attribute data. </param>
    /// <returns> The named type symbol. </returns>
    /// <exception cref="InvalidOperationException"> Thrown if the attribute does not have a valid class. </exception>
    public static INamedTypeSymbol GetNamedTypeSymbol(this AttributeData attributeData) {
        return attributeData.AttributeClass
            ?? throw new InvalidOperationException("AttributeData does not have a valid AttributeClass.");
    }

    /// <summary>
    ///     Gets the fully-qualified name of the attribute type.
    /// </summary>
    /// <param name="attributeData"> The attribute data. </param>
    /// <returns> The fully-qualified attribute type name. </returns>
    /// <exception cref="InvalidOperationException"> Thrown if the attribute does not have a valid class. </exception>
    public static string GetFullyQualifiedName(this AttributeData attributeData) {
        return attributeData.AttributeClass?.ToString()
            ?? throw new InvalidOperationException("AttributeData does not have a valid AttributeClass.");
    }

    /// <summary>
    ///     Gets the source location where the attribute was applied.
    /// </summary>
    /// <remarks>
    /// FALLBACK CHAIN:
    /// 1. Try to get the location of the attribute syntax (e.g., [MyAttribute(...)])
    /// 2. If that's not available, fall back to the symbol's location
    /// 3. If that's not available, use Location.None
    /// 
    /// This ensures diagnostics can point to something meaningful even when:
    /// - Attributes are inherited from base classes
    /// - Attributes are from metadata (referenced assemblies)
    /// - Syntax references are unavailable
    /// </remarks>
    /// <param name="attributeData"> The attribute data. </param>
    /// <param name="attributedSymbol"> The symbol to which the attribute was applied. </param>
    /// <returns> The attribute application location. </returns>
    public static Location GetAttributeLocation(this AttributeData attributeData, ISymbol attributedSymbol) {
        return attributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation()
            ?? attributedSymbol.GetLocationOrDefault();
    }

    /// <summary>
    ///     Gets a named integer argument from the attribute.
    /// </summary>
    /// <remarks>
    /// NULLABLE RETURN:
    /// Returns null if the argument isn't found or isn't an int. This allows callers to distinguish
    /// between "not specified" (null) and "explicitly set to 0" (0).
    /// </remarks>
    /// <param name="attributeData"> The attribute data. </param>
    /// <param name="argumentName"> The name of the argument. </param>
    /// <returns> The integer value, or null if not found. </returns>
    public static int? GetNamedIntArgument(this AttributeData attributeData, string argumentName) {
        return attributeData.NamedArguments
            .FirstOrDefault(arg => arg.Key == argumentName)
            .Value.Value as int?;
    }
    
    /// <summary>
    ///     Gets a named boolean argument from the attribute.
    /// </summary>
    /// <param name="attributeData"> The attribute data. </param>
    /// <param name="argumentName"> The name of the argument. </param>
    /// <returns> The boolean value, or null if not found. </returns>
    public static bool? GetNamedBoolArgument(this AttributeData attributeData, string argumentName) {
        return attributeData.NamedArguments
            .FirstOrDefault(arg => arg.Key == argumentName)
            .Value.Value as bool?;
    }
    
    /// <summary>
    ///     Gets a named argument of a specific type from the attribute.
    /// </summary>
    /// <remarks>
    /// GENERIC TYPE-SAFE ACCESSOR:
    /// Provides type safety and null handling for any argument type. Example:
    ///   var lifetime = attr.GetNamedArgument&lt;string&gt;("Lifetime");
    ///   var scopeType = attr.GetNamedArgument&lt;INamedTypeSymbol&gt;("Scope");
    /// </remarks>
    /// <typeparam name="T"> The type of the argument value. </typeparam>
    /// <param name="attributeData"> The attribute data. </param>
    /// <param name="argumentName"> The name of the argument. </param>
    /// <returns> The argument value, or the default value if not found. </returns>
    public static T? GetNamedArgument<T>(this AttributeData attributeData, string argumentName) {
        return attributeData.NamedArguments
            .FirstOrDefault(arg => arg.Key == argumentName)
            .Value.Value is T t ? t : default; 
    }
    
    /// <summary>
    ///     Gets a constructor argument matching a predicate.
    /// </summary>
    /// <remarks>
    /// PREDICATE-BASED FILTERING:
    /// Useful when constructor parameters are complex or positional. Example:
    ///   var typeArg = attr.GetConstructorArgument&lt;ITypeSymbol&gt;(
    ///       tc =&gt; tc.Kind == TypedConstantKind.Type);
    /// 
    /// This handles cases where you need to filter by TypedConstant.Kind, IsNull, etc.
    /// </remarks>
    /// <typeparam name="T"> The type of the argument value. </typeparam>
    /// <param name="attributeData"> The attribute data. </param>
    /// <param name="predicate"> The predicate to match. </param>
    /// <returns> The argument value, or null if not found. </returns>
    public static T? GetConstructorArgument<T>(this AttributeData attributeData, Func<TypedConstant, bool> predicate) where T : class {
        return attributeData.ConstructorArguments.FirstOrDefault(predicate).Value as T;
    }
    
    /// <summary>
    ///     Gets a constructor argument matching a predicate, or returns a default value.
    /// </summary>
    /// <remarks>
    /// DEFAULT VALUE VARIANT:
    /// Unlike the nullable variant, this allows specifying a non-null default when the argument
    /// is missing. Useful for value types or when you want to ensure a non-null return.
    /// </remarks>
    /// <typeparam name="T"> The type of the argument value. </typeparam>
    /// <param name="attributeData"> The attribute data. </param>
    /// <param name="predicate"> The predicate to match. </param>
    /// <param name="defaultValue"> The default value to return if not found. </param>
    /// <returns> The argument value, or the default value if not found. </returns>
    public static T GetConstructorArgument<T>(this AttributeData attributeData, Func<TypedConstant, bool> predicate, T defaultValue) {
        return attributeData.ConstructorArguments.FirstOrDefault(predicate).Value is T t ? t : defaultValue;
    }
    
    /// <summary>
    ///     Gets all constructor arguments matching a predicate.
    /// </summary>
    /// <remarks>
    /// MULTI-VALUE EXTRACTION:
    /// Supports params arrays or attributes with multiple constructor arguments of the same type.
    /// Example: [MyAttribute(typeof(IFoo), typeof(IBar), typeof(IBaz))]
    /// 
    /// Returns an empty sequence if no matches, allowing safe iteration with foreach/LINQ.
    /// </remarks>
    /// <typeparam name="T"> The type of the argument values. </typeparam>
    /// <param name="attributeData"> The attribute data. </param>
    /// <param name="predicate"> The predicate to match. </param>
    /// <returns> A sequence of matching argument values. </returns>
    public static IEnumerable<T> GetConstructorArguments<T>(this AttributeData attributeData, Func<TypedConstant, bool> predicate) {
        return attributeData.ConstructorArguments.Where(predicate)
                .Select(argument => argument.Value)
                .OfType<T>();
    }
}
