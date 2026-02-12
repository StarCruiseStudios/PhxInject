// -----------------------------------------------------------------------------
// <copyright file="TypeNames.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental;

#endregion

namespace Phx.Inject.Common;

/// <summary>
/// Contains constant strings for fully-qualified type names used throughout the generator.
/// 
/// PURPOSE:
/// - Provides a single source of truth for type name strings
/// - Enables compile-time verification of type name references
/// - Simplifies refactoring if type names change
/// 
/// WHY THIS EXISTS:
/// Source generators work with type names as strings in several contexts:
/// 1. Comparing AttributeData.GetFullyQualifiedName() to known attribute types
/// 2. Generating code that references framework types (System.Func, IReadOnlyList, etc.)
/// 3. Pattern matching on specific types during semantic analysis
/// 
/// String literals scattered throughout the codebase are:
/// - Error-prone (typos won't be caught until runtime)
/// - Hard to refactor (must find all occurrences manually)
/// - Inconsistent (different parts may use different conventions)
/// 
/// This constants class provides:
/// - Single point of maintenance for all type name strings
/// - IntelliSense support when referencing known types
/// - Compile-time errors if the referenced types don't exist (via nameof())
/// - Consistent formatting (always fully-qualified)
/// 
/// DESIGN DECISIONS:
/// 
/// 1. Why use nameof() instead of string literals?
///    - Compile-time validation that the type exists
///    - Refactoring tools can rename types safely
///    - Intent is clear (referencing a real type, not just a string)
/// 
/// 2. Why fully-qualified names?
///    - Roslyn's ToString() on ITypeSymbol produces fully-qualified names
///    - Avoids ambiguity (System.Action vs some other Action)
///    - Works correctly even when comparing types from different namespaces
/// 
/// 3. Why IReadOnlySet is special?
///    - IReadOnlySet doesn't exist in .NET Standard 2.0
///    - Can't use nameof() because it won't compile
///    - String literal is necessary for netstandard2.0 compatibility
/// 
/// USAGE PATTERNS:
/// 
/// Finding attributes:
///   if (attr.GetFullyQualifiedName().StartsWith(PhxInject.NamespaceName))
/// 
/// Type comparison:
///   if (typeSymbol.GetFullyQualifiedBaseName() == TypeNames.FuncClassName)
/// 
/// Code generation:
///   builder.AppendLine($"{TypeNames.IReadOnlyListClassName}&lt;{itemType}&gt; items");
/// </summary>
internal static class TypeNames {
    public const string FactoryClassName = $"{PhxInject.NamespaceName}.{nameof(Factory<object>)}";
    
    public const string InjectionUtilClassName = $"{PhxInject.NamespaceName}.{nameof(InjectionUtil)}";

    public const string ActionClassName = $"System.{nameof(Action)}";
    
    public const string AttributeClassName = $"System.{nameof(Attribute)}";
    
    public const string FuncClassName = $"System.{nameof(Func<object>)}";

    public const string IReadOnlyDictionaryClassName =
        $"System.Collections.Generic.{nameof(IReadOnlyDictionary<object, object>)}";

    public const string IReadOnlyListClassName = $"System.Collections.Generic.{nameof(IReadOnlyList<object>)}";
    
    public const string IReadOnlySetClassName = "System.Collections.Generic.IReadOnlySet"; // Not in netstandard2.0
    
    public const string ISetClassName = $"System.Collections.Generic.{nameof(ISet<object>)}";
    
    public const string StringPrimitiveTypeName = "string";
}
