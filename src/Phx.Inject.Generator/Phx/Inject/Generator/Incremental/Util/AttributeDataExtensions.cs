// -----------------------------------------------------------------------------
// <copyright file="AttributeDataExtensions.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;

namespace Phx.Inject.Generator.Incremental.Util;

internal static class AttributeDataExtensions {
    public static INamedTypeSymbol GetNamedTypeSymbol(this AttributeData attributeData) {
        return attributeData.AttributeClass
            ?? throw new InvalidOperationException("AttributeData does not have a valid AttributeClass.");
    }

    public static string GetFullyQualifiedName(this AttributeData attributeData) {
        return attributeData.AttributeClass?.ToString()
            ?? throw new InvalidOperationException("AttributeData does not have a valid AttributeClass.");
    }

    public static Location GetAttributeLocation(this AttributeData attributeData, ISymbol attributedSymbol) {
        return attributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation()
            ?? attributedSymbol.GetLocationOrDefault();
    }

    public static int? GetNamedIntArgument(this AttributeData attributeData, string argumentName) {
        return attributeData.NamedArguments
            .FirstOrDefault(arg => arg.Key == argumentName)
            .Value.Value as int?;
    }
    
    public static bool? GetNamedBoolArgument(this AttributeData attributeData, string argumentName) {
        return attributeData.NamedArguments
            .FirstOrDefault(arg => arg.Key == argumentName)
            .Value.Value as bool?;
    }
    
    public static T? GetNamedArgument<T>(this AttributeData attributeData, string argumentName) {
        return attributeData.NamedArguments
            .FirstOrDefault(arg => arg.Key == argumentName)
            .Value.Value is T t ? t : default; 
    }
    
    public static T? GetConstructorArgument<T>(this AttributeData attributeData, Func<TypedConstant, bool> predicate) where T : class {
        return attributeData.ConstructorArguments.FirstOrDefault(predicate).Value as T;
    }
    
    public static T GetConstructorArgument<T>(this AttributeData attributeData, Func<TypedConstant, bool> predicate, T defaultValue) {
        return attributeData.ConstructorArguments.FirstOrDefault(predicate).Value is T t ? t : defaultValue;
    }
    
    public static IEnumerable<T> GetConstructorArguments<T>(this AttributeData attributeData, Func<TypedConstant, bool> predicate) {
        return attributeData.ConstructorArguments.Where(predicate)
                .SelectMany(argument => argument.Values)
                .OfType<T>();
    }
}
