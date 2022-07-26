// -----------------------------------------------------------------------------
//  <copyright file="LabelAttribute.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject {
    using System;

    /// <summary>
    ///     Annotates a factory method or dependency with a unique label used to
    ///     discriminate them from other dependencies with the same type.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     The attribute can also be added to a custom attribute class, and
    ///     that attribute can be used in place of this attribute with a raw 
    ///     string.
    /// </para>
    /// <para>
    ///     LabelAttributes applied directly to a dependency will not conflict
    ///     with LabelAttributes applied indirectly through a custom attribute.
    ///     Custom label attributes that use the same string label will be
    ///     considered equivalent.
    /// </para>
    /// </remarks>
    [AttributeUsage(Usage)]
    public class LabelAttribute : Attribute {
        /// <summary>
        ///     The <see cref="AttributeTargets"/> flags applied to the label 
        ///     attribute. These same flags should be applied to custom label
        ///     attributes.
        /// </summary>
        public const AttributeTargets Usage = AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter;

        /// <summary>
        ///     The unique name for this label.
        /// </summary>
        public string Label { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LabelAttribute"/> class.
        /// </summary>
        /// <param name="label">The unique name for this label.</param>
        public LabelAttribute(string label) {
            Label = label;
        }
    }
}