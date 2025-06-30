// -----------------------------------------------------------------------------
//  <copyright file="ChildInjectorAttribute.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary> Annotates a method used to retrieve a child injector instance. </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ChildInjectorAttribute : Attribute { }
