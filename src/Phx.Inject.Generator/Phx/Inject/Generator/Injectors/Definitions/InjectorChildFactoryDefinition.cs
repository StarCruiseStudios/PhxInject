﻿// -----------------------------------------------------------------------------
//  <copyright file="InjectorChildFactoryDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Injectors.Definitions {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Definitions;

    internal record InjectorChildFactoryDefinition(
            TypeModel InjectorChildInterfaceType,
            string InjectorChildFactoryMethodName,
            IList<TypeModel> Parameters,
            Location Location
    ) : IDefinition;
}
