// -----------------------------------------------------------------------------
// <copyright file="InjectorDependency.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary>
///     Annotates an interface as an injector dependency used to pass values from a parent to a
///     child injector.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public class InjectorDependency : Attribute { }
