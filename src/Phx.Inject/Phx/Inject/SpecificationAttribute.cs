// -----------------------------------------------------------------------------
//  <copyright file="SpecificationAttribute.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject {
    using System;

    /// <summary>
    ///     Annotates a specification class that contains factory methods and links
    ///     used to construct a DAG.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class SpecificationAttribute : Attribute { }
}
