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
    public const string AttributeClassName = $"System.{nameof(Attribute)}";
    public const string BuilderAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(BuilderAttribute)}";

    public const string BuilderReferenceAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(BuilderReferenceAttribute)}";

    public const string ChildInjectorAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(ChildInjectorAttribute)}";

    public const string DependencyAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(DependencyAttribute)}";

    public const string FabricationModeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(FabricationMode)}";
    public const string FactoryAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(FactoryAttribute)}";
    public const string FactoryClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(Factory<object>)}";

    public const string FactoryReferenceAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(FactoryReferenceAttribute)}";

    public const string InjectionUtilClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(InjectionUtil)}";

    public const string InjectorAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(InjectorAttribute)}";

    public const string IReadOnlyDictionaryClassName = "System.Collections.Generic.IReadOnlyDictionary";
    public const string IReadOnlyListClassName = "System.Collections.Generic.IReadOnlyList";
    public const string ISetClassName = "System.Collections.Generic.ISet";
    public const string LabelAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(LabelAttribute)}";
    public const string LinkAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(LinkAttribute)}";
    public const string PartialAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(PartialAttribute)}";

    public const string QualifierAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(QualifierAttribute)}";

    public const string SpecificationAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(SpecificationAttribute)}";

    public const string StringClassName = "string";
}
