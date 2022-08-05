﻿// -----------------------------------------------------------------------------
//  <copyright file="QualifiedTypeModel.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Common {
    internal record QualifiedTypeModel(
            TypeModel TypeModel,
            string Qualifier
    ) {
        public const string NoQualifier = "";

        public override string ToString() {
            return string.IsNullOrEmpty(Qualifier)
                    ? TypeModel.ToString()
                    : $"[{Qualifier}] {TypeModel}";
        }
    }
}
