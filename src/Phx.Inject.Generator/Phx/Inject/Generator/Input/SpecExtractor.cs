// -----------------------------------------------------------------------------
//  <copyright file="SpecExtractor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Input {
    using Phx.Inject.Generator.Model.Descriptors;

    internal class SpecExtractor {
        public SpecExtractor() {
            new SpecDescriptor.Builder(
                    new SpecFactoryDescriptor.Builder().Build,
                    new SpecBuilderDescriptor.Builder().Build,
                    new LinkDescriptor.Builder().Build);
        }
    }
}
