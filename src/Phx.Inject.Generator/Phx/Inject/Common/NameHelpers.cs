// -----------------------------------------------------------------------------
//  <copyright file="NameHelpers.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Text;
using System.Text.RegularExpressions;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Common;

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

    public static string GetSpecContainerFactoryName(this SpecFactoryDesc factory, IGeneratorContext context) {
        var sb = factory.SpecFactoryMemberType switch {
            SpecFactoryMemberType.Method => new StringBuilder("Fac_"),
            SpecFactoryMemberType.Property => new StringBuilder("PropFac_"),
            SpecFactoryMemberType.Reference => new StringBuilder("RefFac_"),
            SpecFactoryMemberType.Constructor => new StringBuilder("CtorFac_"),
            _ => throw Diagnostics.InternalError.AsFatalException(
                $"Unhandled Spec Factory Member Type {factory.SpecFactoryMemberType}.",
                factory.Location,
                context)
        };

        sb.Append(factory.ReturnType.AsVariableName().StartUppercase())
            .Append("_")
            .Append(factory.FactoryMemberName.AsValidIdentifier().StartUppercase());

        return sb.ToString();
    }

    public static string GetSpecContainerBuilderName(this SpecBuilderDesc builder, IGeneratorContext context) {
        var sb = builder.SpecBuilderMemberType switch {
            SpecBuilderMemberType.Method => new StringBuilder("Bld_"),
            SpecBuilderMemberType.Reference => new StringBuilder("RefBld_"),
            SpecBuilderMemberType.Direct => new StringBuilder("DirBld_"),
            _ => throw Diagnostics.InternalError.AsFatalException(
                $"Unhandled Spec Builder Member Type {builder.SpecBuilderMemberType}.",
                builder.Location,
                context)
        };

        sb.Append(builder.BuiltType.AsVariableName().StartUppercase())
            .Append("_")
            .Append(builder.BuilderMemberName.AsValidIdentifier().StartUppercase());

        return sb.ToString();
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
        return string.IsNullOrEmpty(input)
            ? input
            : char.ToLower(input[0]) + input.Substring(1);
    }

    public static string StartUppercase(this string input) {
        return string.IsNullOrEmpty(input)
            ? input
            : char.ToUpper(input[0]) + input.Substring(1);
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
        var referenceName = type.Qualifier is NoQualifier
            ? type.TypeModel.BaseTypeName
            : $"{type.Qualifier.Identifier}_{type.TypeModel.BaseTypeName}";
        return referenceName.AsValidIdentifier();
    }
}
