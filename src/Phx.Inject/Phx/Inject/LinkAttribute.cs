// -----------------------------------------------------------------------------
//  <copyright file="LinkAttribute.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject {
    using System;

    /// <summary> Models a link between one dependency key and another. </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class LinkAttribute : Attribute {
        /// <summary> The dependency key for the type consumed by the link. </summary>
        public Type Input { get; }

        /// <summary> The dependency key for the type exposed by the link. </summary>
        public Type Output { get; }

        /// <summary> Initializes a new instance of the <see cref="LinkAttribute" /> class. </summary>
        /// <param name="input"> The dependency key for the type consumed by the link. </param>
        /// <param name="output"> The dependency key for the type exposed by the link. </param>
        public LinkAttribute(Type input, Type output) {
            Input = input;
            Output = output;
        }
    }
}
