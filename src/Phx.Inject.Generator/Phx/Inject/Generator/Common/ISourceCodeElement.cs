﻿// -----------------------------------------------------------------------------
//  <copyright file="ISourceCodeElement.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Common {
    using Microsoft.CodeAnalysis;

    internal interface ISourceCodeElement {
        Location Location { get; }
    }
}
