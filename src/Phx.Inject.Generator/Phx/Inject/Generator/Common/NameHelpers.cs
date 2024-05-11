// -----------------------------------------------------------------------------
//  <copyright file="NameHelpers.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Common {
    using System.Text.RegularExpressions;
    using Phx.Inject.Generator.Specifications;
    using Phx.Inject.Generator.Specifications.Descriptors;

    internal static class NameHelpers {
        private const string GeneratedInjectorClassPrefix = "Generated";
        public const string SpecContainerCollectionTypeName = "SpecContainerCollection";

        private static readonly Regex ValidCharsRegex = new(@"[^a-zA-Z0-9_]");

        public static string GetInjectorClassName(this TypeModel injectorInterfaceType) {
            var baseName = injectorInterfaceType.AsVariableName()
                    .RemoveLeadingI()
                    .StartUppercase();
            return $"{GeneratedInjectorClassPrefix}{baseName}";
        }

        public static string GetSpecContainerCollectionTypeName(this TypeModel injectorType) {
            return $"{injectorType.BaseTypeName}.{SpecContainerCollectionTypeName}";
        }

        public static string GetSpecContainerFactoryName(this SpecFactoryDescriptor factory) {
            return factory.SpecFactoryMemberType switch {
                SpecFactoryMemberType.Method => factory.FactoryMemberName,
                SpecFactoryMemberType.Property => $"GetProperty{factory.FactoryMemberName}",
                SpecFactoryMemberType.Reference => $"GetReference{factory.FactoryMemberName}",
                SpecFactoryMemberType.Constructor => $"GetConstructor{factory.FactoryMemberName}",
                _ => throw new InjectionException(
                        Diagnostics.InternalError,
                        $"Unhandled SpecFactoryMemberType {factory.SpecFactoryMemberType}.",
                        factory.Location)
            };
        }

        public static string GetSpecContainerBuilderName(this SpecBuilderDescriptor builder) {
            return builder.SpecBuilderMemberType switch {
                SpecBuilderMemberType.Method => builder.BuilderMemberName,
                SpecBuilderMemberType.Reference => $"GetReference{builder.BuilderMemberName}",
                _ => throw new InjectionException(
                        Diagnostics.InternalError,
                        $"Unhandled SpecBuilderMemberType {builder.SpecBuilderMemberType}.",
                        builder.Location)
            };
        }

        public static string GetCombinedClassName(TypeModel prefixType, TypeModel suffixType) {
            return GetAppendedClassName(prefixType, suffixType.BaseTypeName);

        }

        public static string GetAppendedClassName(TypeModel prefixType, string suffix) {
            return $"{prefixType.BaseTypeName}_{suffix}"
                    .AsValidIdentifier()
                    .StartUppercase();
        }

        public static string GetVariableName(this TypeModel type) {
            return type.AsVariableName().StartLowercase();
        }

        public static string GetVariableName(this QualifiedTypeModel type) {
            return type.AsVariableName().StartLowercase();
        }

        public static string GetPropertyName(this TypeModel type) {
            return type.AsVariableName().StartUppercase();
        }

        public static string AsValidIdentifier(this string baseName) {
            var referenceName = baseName.Replace(".", "_");
            return ValidCharsRegex.Replace(referenceName, "");
        }

        public static string StartLowercase(this string input) {
            return char.ToLower(input[0]) + input.Substring(1);
        }

        public static string StartUppercase(this string input) {
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        public static string RemoveLeadingI(this string input) {
            return input.StartsWith("I")
                    ? input.Substring(1)
                    : input;
        }

        private static string AsVariableName(this TypeModel type) {
            return type.BaseTypeName.AsValidIdentifier();
        }

        private static string AsVariableName(this QualifiedTypeModel type) {
            var referenceName = string.IsNullOrEmpty(type.Qualifier)
                    ? type.TypeModel.BaseTypeName
                    : $"{type.Qualifier}_{type.TypeModel.BaseTypeName}";
            return referenceName.AsValidIdentifier();
        }
    }
}
