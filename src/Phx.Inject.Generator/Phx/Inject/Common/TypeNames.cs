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
