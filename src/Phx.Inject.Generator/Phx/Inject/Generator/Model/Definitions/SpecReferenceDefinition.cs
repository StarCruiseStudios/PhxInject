// -----------------------------------------------------------------------------
//  <copyright file="SpecReferenceDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Definitions {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Descriptors;

    internal delegate SpecReferenceDefinition CreateSpecReferenceDefinition(SpecDescriptor specDescriptor);

    /// <summary>
    ///     A reference to a specification used inside of a Spec Container to invoke the specification methods.
    /// </summary>
    /// <param name="SpecType"> The type of the specification that is reference. </param>
    /// <param name="InstantiationMode"> Whether the reference is static or instantiated.</param>
    /// <param name="Location"> The location in the original code this reference is based on.</param>
    internal record SpecReferenceDefinition(
            TypeModel SpecType,
            string? SpecReferenceName,
            SpecInstantiationMode InstantiationMode,
            Location Location
    ) : IDefinition {
        public class Builder {
            public SpecReferenceDefinition Build(SpecDescriptor specDescriptor) {
                var specReferenceName = specDescriptor.InstantiationMode == SpecInstantiationMode.Instantiated
                        ? "instance"
                        : null;
                return new SpecReferenceDefinition(
                        specDescriptor.SpecType,
                        specReferenceName,
                        specDescriptor.InstantiationMode,
                        specDescriptor.Location);
            }
        }
    }
}
