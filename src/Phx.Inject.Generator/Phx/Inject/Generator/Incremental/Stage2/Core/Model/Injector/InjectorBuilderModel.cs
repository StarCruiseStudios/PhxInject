// -----------------------------------------------------------------------------
// <copyright file="InjectorBuilderModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Model.Injector;

/// <summary>
///     Code generation model for a builder (activator) method in the injector class.
/// </summary>
/// <param name="BuiltType">
///     The qualified type to be initialized (the target parameter type).
/// </param>
/// <param name="BuilderMethodName">
///     The method name from the user's interface (e.g., "InitializeViewModel").
/// </param>
/// <param name="Location">The source location where this builder is defined.</param>
/// <remarks>
///     Stage 2 counterpart to <see cref="Stage1.Metadata.Model.Injector.InjectorActivatorMetadata"/>.
///     Stage 1 uses "Activator", Stage 2 uses "Builder" for the same concept: methods that initialize
///     existing objects rather than constructing new ones. Accepts an existing object instance and
///     initializes its dependencies by delegating to the specification container's builder method.
/// </remarks>
/// <param name="BuiltType"> 
///     The qualified type to be initialized (the target parameter type), including any [Label] qualifiers. 
///     Used to resolve the correct specification builder method during code generation.
/// </param>
/// <param name="BuilderMethodName"> 
///     The method name from the user's interface (e.g., "InitializeViewModel"). Used as-is in the
///     generated implementation to satisfy interface contract.
/// </param>
/// <param name="Location"> The source location where this builder is defined for diagnostics. </param>
internal record InjectorBuilderModel(
    QualifiedTypeMetadata BuiltType,
    string BuilderMethodName,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
