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
///     Extension methods for working with Roslyn <see cref="AttributeData"/>.
/// </summary>
internal static class AttributeDataExtensions {
    /// <summary>
    ///     Gets the named type symbol for the attribute.
    /// </summary>
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
