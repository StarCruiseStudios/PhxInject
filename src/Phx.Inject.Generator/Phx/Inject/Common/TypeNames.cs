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
///     Contains constant strings for fully-qualified type names used throughout the generator.
/// </summary>
internal static class TypeNames {
    /// <summary> The fully-qualified name of the Phx.Inject.Factory type. </summary>
    public const string FactoryClassName = $"{PhxInject.NamespaceName}.{nameof(Factory<object>)}";
    
    /// <summary> The fully-qualified name of the Phx.Inject.InjectionUtil type. </summary>
    public const string InjectionUtilClassName = $"{PhxInject.NamespaceName}.{nameof(InjectionUtil)}";

    /// <summary> The fully-qualified name of the System.Action type. </summary>
    public const string ActionClassName = $"System.{nameof(Action)}";
    
    /// <summary> The fully-qualified name of the System.Attribute type. </summary>
    public const string AttributeClassName = $"System.{nameof(Attribute)}";
    
    /// <summary> The fully-qualified name of the System.Func type. </summary>
    public const string FuncClassName = $"System.{nameof(Func<object>)}";

    /// <summary> The fully-qualified name of the System.Collections.Generic.IReadOnlyDictionary type. </summary>
    public const string IReadOnlyDictionaryClassName =
        $"System.Collections.Generic.{nameof(IReadOnlyDictionary<object, object>)}";

    /// <summary> The fully-qualified name of the System.Collections.Generic.IReadOnlyList type. </summary>
    public const string IReadOnlyListClassName = $"System.Collections.Generic.{nameof(IReadOnlyList<object>)}";
    
    /// <summary> The fully-qualified name of the System.Collections.Generic.IReadOnlySet type. </summary>
    public const string IReadOnlySetClassName = "System.Collections.Generic.IReadOnlySet"; // Not in netstandard2.0
    
    /// <summary> The fully-qualified name of the System.Collections.Generic.ISet type. </summary>
    public const string ISetClassName = $"System.Collections.Generic.{nameof(ISet<object>)}";
    
    /// <summary> The primitive type name for string. </summary>
    public const string StringPrimitiveTypeName = "string";
}
