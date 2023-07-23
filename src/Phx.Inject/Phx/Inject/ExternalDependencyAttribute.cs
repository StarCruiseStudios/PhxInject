// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyAttribute.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject {
    using System;

    /// <summary> Defines an external dependency that is required by an injector interface. </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ExternalDependencyAttribute : Attribute {
        /// <summary> Gets the type of the external dependency. </summary>
        public Type ExternalDependency { get; }

        /// <summary> Initialzes a new instance of the <see cref="ExternalDependencyAttribute" /> class. </summary>
        /// <param name="externalDependency"> The type of the external dependency. </param>
        public ExternalDependencyAttribute(Type externalDependency) {
            ExternalDependency = externalDependency;
        }
    }
}
