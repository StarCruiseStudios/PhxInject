// -----------------------------------------------------------------------------
// <copyright file="AutoBuilderAttribute.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary>
///     Annotates a builder method that will be automatically invoked to complete the construction 
///     of a given dependency.
/// </summary>
/// <remarks>
/// Auto-builders enable automatic identification and invocation of builder methods based on
/// method signatures, without requiring explicit <see cref="BuilderAttribute"/> annotations.
///
/// ## Difference from BuilderAttribute
///
/// - <see cref="BuilderAttribute"/> requires explicit builder method annotation in specifications
/// - <see cref="AutoBuilderAttribute"/> allows automatic inference of builder logic from
///   method naming conventions and signatures
/// </remarks>
/// <seealso cref="BuilderAttribute"/>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class AutoBuilderAttribute : Attribute { }
