// -----------------------------------------------------------------------------
//  <copyright file="InjectorAttribute.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject {
    using System;
    using System.Collections.Generic;

    /// <summary> Annotates an injector interface as the entry point to a DAG. </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class InjectorAttribute : Attribute {
        /// <summary> The name to use for the generated injector class. </summary>
        /// <remarks>
        ///     This value may be null if no custom class name is specified. If no custom
        ///     name is specified a default value of "GeneratedXyz" will be used, where Xyz
        ///     is the annotated interface's name with the leading "I" removed, if there is
        ///     one.
        /// </remarks>
        public string? GeneratedClassName { get; }

        /// <summary> A collection of specification types used by this injector. </summary>
        public IEnumerable<Type> Specifications { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InjectorAttribute" />
        ///     class.
        /// </summary>
        /// <param name="specifications">
        ///     A collection of specification types used by this
        ///     injector.
        /// </param>
        public InjectorAttribute(params Type[] specifications) : this(generatedClassName: null, specifications) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InjectorAttribute" />
        ///     class.
        /// </summary>
        /// <param name="generatedClassName">
        ///     The name to use for the generated injector
        ///     class.
        /// </param>
        /// <param name="specifications">
        ///     A collection of specification types used by this
        ///     injector.
        /// </param>
        public InjectorAttribute(string? generatedClassName, params Type[] specifications) {
            GeneratedClassName = generatedClassName;
            Specifications = specifications;
        }
    }
}
