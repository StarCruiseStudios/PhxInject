// -----------------------------------------------------------------------------
//  <copyright file="IConstructedSpecification.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Specification {
    using Phx.Inject.Tests.Data.Model;

    [Specification]
    internal interface IConstructedSpecification {
        [Factory]
        internal int GetIntValue();

        [Factory]
        internal IntLeaf GetIntLeaf();
    }
}