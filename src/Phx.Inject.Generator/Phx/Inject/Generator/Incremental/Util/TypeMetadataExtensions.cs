// -----------------------------------------------------------------------------
// <copyright file="TypeMetadataExtensions.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;

#endregion

namespace Phx.Inject.Generator.Incremental.Util;

/// <summary>
/// Extension methods for working with TypeMetadata in the context of code generation.
/// 
/// PURPOSE:
/// - Encapsulates naming conventions for generated types
/// - Ensures consistent type name transformations across the generator
/// - Provides semantic clarity for type derivation operations
/// 
/// WHY THIS EXISTS:
/// The generator creates multiple derived types from user-defined types:
/// 1. Injector implementations from injector interfaces (IMyInjector → GeneratedMyInjector)
/// 2. Spec container types that combine injector and spec names
/// 
/// These extensions:
/// - Centralize the naming logic to prevent inconsistencies
/// - Make the intent explicit (CreateInjectorType vs manual string concatenation)
/// - Allow future evolution of naming conventions in one place
/// 
/// NAMING CONVENTIONS:
/// - Injector classes: "Generated" + InterfaceName (or custom name if provided)
/// - Spec containers: InjectorBaseName + "_" + SpecBaseName
/// 
/// DESIGN DECISIONS:
/// 1. Why prefix "Generated"?
///    - Clearly indicates the type is generated code
///    - Avoids naming conflicts with user code
///    - Follows common convention (e.g., EF Core's "Generated..." classes)
/// 
/// 2. Why underscore in spec containers?
///    - Prevents namespace collisions with nested types
///    - Makes parsing/debugging easier (clear visual separation)
///    - Unlikely to conflict with user naming conventions
/// 
/// 3. Why strip type arguments from spec containers?
///    - Spec containers are internal implementation details
///    - Generic parameters are resolved at the point of use
///    - Simplifies name generation logic
/// </summary>
internal static class TypeMetadataExtensions {
    private const string GeneratedInjectorClassPrefix = "Generated";

    /// <summary>
    ///     Creates the generated injector type from the interface type and optional generated class name.
    /// </summary>
    /// <remarks>
    /// NAMING LOGIC:
    /// - If generatedClassName is provided, use it as-is (allows custom names via attributes)
    /// - Otherwise, prefix the interface name with "Generated" (IMyInjector → GeneratedMyInjector)
    /// 
    /// METADATA PRESERVATION:
    /// - Namespace: Same as the interface (generated code lives alongside user code)
    /// - Type arguments: Empty (concrete class, not generic)
    /// - Location: Same as interface (for diagnostics that reference the generated type)
    /// 
    /// EXAMPLE:
    /// Interface: MyApp.DI.IAppInjector
    /// Generated: MyApp.DI.GeneratedAppInjector (or custom name if specified)
    /// </remarks>
    public static TypeMetadata CreateInjectorType(
        this TypeMetadata injectorInterfaceType,
        string? generatedClassName
    ) {
        var baseName = string.IsNullOrEmpty(generatedClassName)
            ? $"{GeneratedInjectorClassPrefix}{injectorInterfaceType.BaseTypeName}"
            : generatedClassName!;
        return new TypeMetadata(
            injectorInterfaceType.NamespaceName,
            baseName,
            EquatableList<TypeMetadata>.Empty,
            injectorInterfaceType.Location
        );
    }

    /// <summary>
    ///     Creates the spec container type for a given injector and spec (same convention as legacy TypeHelpers.CreateSpecContainerType).
    /// </summary>
    /// <remarks>
    /// SPEC CONTAINER PATTERN:
    /// Spec containers are internal helper classes that group together factory methods and
    /// dependencies for a specific injector+spec combination. They're implementation details
    /// of the generated injector.
    /// 
    /// NAMING CONVENTION:
    /// InjectorBaseName + "_" + SpecBaseName
    /// Example: AppInjector_AppSpec → class AppInjector_AppSpec { ... }
    /// 
    /// WHY STRIP TYPE ARGUMENTS:
    /// Spec containers are non-generic classes that contain the resolved implementations
    /// for a specific configuration. Generic parameters would be redundant and complicate
    /// the generated code structure.
    /// 
    /// COMPATIBILITY NOTE:
    /// Maintains the same naming logic as the legacy generator to ensure consistent behavior
    /// during migration and to support any external tools that depend on these names.
    /// </remarks>
    public static TypeMetadata CreateSpecContainerType(
        this TypeMetadata injectorType,
        TypeMetadata specType
    ) {
        var combinedBaseName = $"{injectorType.BaseTypeName}_{specType.BaseTypeName}";
        return specType with {
            BaseTypeName = combinedBaseName,
            TypeArguments = EquatableList<TypeMetadata>.Empty
        };
    }
}
