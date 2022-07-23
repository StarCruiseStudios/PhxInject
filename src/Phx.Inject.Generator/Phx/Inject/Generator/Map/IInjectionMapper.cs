// -----------------------------------------------------------------------------
//  <copyright file="IInjectionMapper.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Map {
    using System.Collections.Generic;
    using Phx.Inject.Generator.Construct.Definitions;
    using Phx.Inject.Generator.Extract.Model;

    internal interface IInjectionMapper {
        public InjectionDefinition MapToDefinition(InjectorModel injectorModel, IEnumerable<SpecificationModel> specModels);
    }
}