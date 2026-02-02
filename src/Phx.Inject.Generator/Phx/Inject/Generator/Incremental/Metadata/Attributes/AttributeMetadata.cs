// -----------------------------------------------------------------------------
// <copyright file="AttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Model;

namespace Phx.Inject.Generator.Incremental.Metadata.Attributes;

sealed internal record AttributeMetadata(
    string AttributeClassName,
    string TargetName,
    SourceLocation TargetLocation,
    SourceLocation Location
) : ISourceCodeElement {
    public bool Equals(AttributeMetadata? other) {
        if (other is null) return false;
        
        if (ReferenceEquals(this, other)) return true;

        return AttributeClassName.Equals(other.AttributeClassName)
               && TargetName.Equals(other.TargetName);
    }

    public override int GetHashCode() {
        var hash = 17;
        hash = hash * 31 + AttributeClassName.GetHashCode();
        hash = hash * 31 + TargetName.GetHashCode();
        return hash;
    }
    
    public static AttributeMetadata Create(ISymbol targetSymbol, AttributeData attributeData) {
        return new AttributeMetadata(
            attributeData.GetNamedTypeSymbol().GetFullyQualifiedBaseName(),
            targetSymbol.ToString(),
            targetSymbol.Locations.FirstOrDefault().ToSourceLocation(),
            attributeData.GetAttributeLocation(targetSymbol).ToSourceLocation());
    }

    public static IEqualityComparer<IAttributeElement> AttributeTypeComparer { get; } = new AttributeTypeEqualityComparer();
    private class AttributeTypeEqualityComparer : IEqualityComparer<IAttributeElement> {
        public bool Equals(IAttributeElement? x, IAttributeElement? y) {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.AttributeMetadata.TargetName == y.AttributeMetadata.TargetName;
        }

        public int GetHashCode(IAttributeElement obj) {
            return obj.AttributeMetadata.TargetName.GetHashCode();
        }
    }
    
    public static IEqualityComparer<IAttributeElement> AttributeTargetComparer { get; } = new AttributeTargetEqualityComparer();
    private class AttributeTargetEqualityComparer : IEqualityComparer<IAttributeElement> {
        public bool Equals(IAttributeElement? x, IAttributeElement? y) {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            return x.AttributeMetadata.TargetName == y.AttributeMetadata.TargetName;
        }

        public int GetHashCode(IAttributeElement obj) {
            return obj.AttributeMetadata.TargetName.GetHashCode();
        }
    }
}