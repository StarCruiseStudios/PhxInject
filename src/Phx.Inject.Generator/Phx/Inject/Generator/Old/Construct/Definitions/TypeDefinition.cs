// -----------------------------------------------------------------------------
//  <copyright file="TypeDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Construct.Definitions {
    using System.Diagnostics.CodeAnalysis;
    using Phx.Inject.Generator.Extract.Model;

    internal record TypeDefinition(string NamespaceName, string Name) {
        public string QualifiedName => $"{NamespaceName}.{Name}";
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static class ITypeModelExtensions {
        public static TypeDefinition ToTypeDefinition(this TypeModel typeModel) {
            return new TypeDefinition(
                    typeModel.NamespaceName,
                    typeModel.Name);
        }
    }
}
