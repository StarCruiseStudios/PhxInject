// -----------------------------------------------------------------------------
// <copyright file="PhxInjectNames.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Common;

internal static class PhxInjectNames {
    internal static class Attributes {
        public const string PhxInjectNamespace = "Phx.Inject";
        public const string InjectorAttributeShortName = "Injector";
        public const string InjectorAttributeBaseName = nameof(InjectorAttribute);
        public const string SpecificationAttributeShortName = "Specification";
        public const string SpecificationAttributeBaseName = nameof(SpecificationAttribute);

        public const string BuilderAttributeClassName = $"{PhxInjectNamespace}.{nameof(BuilderAttribute)}";
        public const string BuilderReferenceAttributeClassName = $"{PhxInjectNamespace}.{nameof(BuilderReferenceAttribute)}";
        public const string ChildInjectorAttributeClassName = $"{PhxInjectNamespace}.{nameof(ChildInjectorAttribute)}";
        public const string DependencyAttributeClassName = $"{PhxInjectNamespace}.{nameof(DependencyAttribute)}";
        public const string FactoryAttributeClassName = $"{PhxInjectNamespace}.{nameof(FactoryAttribute)}";
        public const string FactoryReferenceAttributeClassName = $"{PhxInjectNamespace}.{nameof(FactoryReferenceAttribute)}";
        public const string InjectorAttributeClassName = $"{PhxInjectNamespace}.{nameof(InjectorAttribute)}";
        public const string LabelAttributeClassName = $"{PhxInjectNamespace}.{nameof(LabelAttribute)}";
        public const string LinkAttributeClassName = $"{PhxInjectNamespace}.{nameof(LinkAttribute)}";
        public const string PartialAttributeClassName = $"{PhxInjectNamespace}.{nameof(PartialAttribute)}";
        public const string QualifierAttributeClassName = $"{PhxInjectNamespace}.{nameof(QualifierAttribute)}";
        public const string SpecificationAttributeClassName = $"{PhxInjectNamespace}.{nameof(SpecificationAttribute)}";
        
    }
}
