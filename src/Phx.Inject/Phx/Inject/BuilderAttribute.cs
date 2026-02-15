// -----------------------------------------------------------------------------
// <copyright file="BuilderAttribute.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary>
///     Annotates a builder method that will be invoked to complete the construction of a given
///     dependency.
/// </summary>
/// <remarks>
/// Builder methods provide post-construction configuration for dependencies. Unlike factories,
/// builders are called after an instance is created to perform additional setup, property
/// injection, or initialization logic.
///
/// ## Execution Order
///
/// When a dependency requires a builder:
/// 1. Factory method creates the instance
/// 2. Builder method is invoked with the instance
/// 3. Configured instance is returned
///
/// ## Usage
///
/// <code>
/// [Specification]
/// public class MySpec {
///     [Factory]
///     public MyService GetService() => new MyService();
///
///     [Builder]
///     public void ConfigureService(MyService service) {
///         service.Initialize();
///     }
/// }
/// </code>
/// </remarks>
/// <seealso cref="AutoBuilderAttribute"/>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class BuilderAttribute : Attribute { }
