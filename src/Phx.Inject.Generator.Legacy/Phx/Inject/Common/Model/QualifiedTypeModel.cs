// -----------------------------------------------------------------------------
// <copyright file="QualifiedTypeModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Extract.Metadata;

namespace Phx.Inject.Common.Model;

internal record QualifiedTypeModel(
    TypeModel TypeModel,
    QualifierMetadata Qualifier
) {
    public virtual bool Equals(QualifiedTypeModel? other) {
        if (other is null) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        return TypeModel.Equals(other.TypeModel)
            && Qualifier.Qualifier.Equals(other.Qualifier.Qualifier);
    }
    public override int GetHashCode() {
        unchecked {
            return (TypeModel.GetHashCode() * 397) ^ Qualifier.Qualifier.GetHashCode();
        }
    }
    public override string ToString() {
        return Qualifier.Qualifier is NoQualifier
            ? TypeModel.ToString()
            : $"[{Qualifier.Qualifier}] {TypeModel}";
    }
}
