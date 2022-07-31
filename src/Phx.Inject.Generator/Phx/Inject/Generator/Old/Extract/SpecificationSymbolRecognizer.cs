// -----------------------------------------------------------------------------
//  <copyright file="SpecificationSymbolRecognizer.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Extract {
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Extract.Model;
    using static Construct.GenerationConstants;

    internal class SpecificationSymbolRecognizer : ISymbolRecognizer<SpecificationModel> {
        public bool IsExpectedSymbol(ITypeSymbol symbol) {
            var specificationAttributes = symbol.GetAttributes()
                    .Where(
                            attributeData =>
                                    attributeData.AttributeClass!.ToString() == SpecificationAttributeClassName);
            if (!specificationAttributes.Any()) {
                return false;
            }

            if (symbol.TypeKind != TypeKind.Class || !symbol.IsStatic) {
                return false;
            }

            return true;
        }
    }
}
