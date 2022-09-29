// -----------------------------------------------------------------------------
//  <copyright file="LabelAttribute.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject {
    using System;

    /// <summary>
    ///     Annotates a factory method or dependency with a unique label used to
    ///     discriminate them from other dependencies with the same type.
    /// </summary>
    [AttributeUsage(QualifierAttribute.Usage)]
    public class LabelAttribute : Attribute {
        /// <summary> The unique name for this label. </summary>
        public string Label { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LabelAttribute" />
        ///     class.
        /// </summary>
        /// <param name="label"> The unique name for this label. </param>
        public LabelAttribute(string label) {
            Label = label;
        }
    }
}
