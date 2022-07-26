// -----------------------------------------------------------------------------
//  <copyright file="GenerationConstants.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Construct {
    public static class GenerationConstants {
        public const string BuilderAttributeClassName = "Phx.Inject.BuilderAttribute";
        public const string FactoryAttributeClassName = "Phx.Inject.FactoryAttribute";
        public const string InjectorAttributeClassName = "Phx.Inject.InjectorAttribute";
        public const string LabelAttributeClassName = "Phx.Inject.LabelAttribute";
        public const string LinkAttributeClassName = "Phx.Inject.LinkAttribute";
        public const string QualifierAttributeClassName = "Phx.Inject.QualifierAttribute";
        public const string SpecificationAttributeClassName = "Phx.Inject.SpecificationAttribute";

        public const string SpecificationContainerSuffix = "Container";
        public static string GetSpecificationContainerName(string specificationName, string injectorName) {
            return $"{injectorName}_{specificationName}{SpecificationContainerSuffix}";
        }

        public const string SpecContainerCollectionClassName = "SpecContainerCollection";
        public const string SpecContainerCollectionInterfaceName = "ISpecContainerCollection";
    }
}
