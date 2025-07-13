// -----------------------------------------------------------------------------
//  <copyright file="AttributeHelpers.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator;

namespace Phx.Inject.Common;

internal static class TypeNames {
    public const string FactoryClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(Factory<object>)}";
    public const string InjectionUtilClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(InjectionUtil)}";
    
    public const string ActionClassName = $"System.{nameof(Action)}";
    public const string AttributeClassName = $"System.{nameof(Attribute)}";
    public const string FuncClassName = $"System.{nameof(Func<object>)}";
    public const string IReadOnlyDictionaryClassName = "System.Collections.Generic.IReadOnlyDictionary";
    public const string IReadOnlyListClassName = "System.Collections.Generic.IReadOnlyList";
    public const string ISetClassName = "System.Collections.Generic.ISet";
    public const string StringPrimitiveTypeName = "string";
}
