// -----------------------------------------------------------------------------
// <copyright file="QualifierTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Types;

internal class QualifierTransformer(
    IAttributeTransformer<LabelAttributeMetadata> labelAttributeTransformer,
    IAttributeTransformer<QualifierAttributeMetadata> qualifierAttributeTransformer
) {
    public static QualifierTransformer Instance { get; } = new(
        LabelAttributeTransformer.Instance,
        QualifierAttributeTransformer.Instance
    );
    
    private IAttributeChecker? labelChecker = labelAttributeTransformer as IAttributeChecker;
    private IAttributeChecker? qualifierChecker = qualifierAttributeTransformer as IAttributeChecker;
    
    public IQualifierMetadata Transform(ISymbol targetSymbol) {
        if (labelChecker?.HasAttribute(targetSymbol) == true) {
            var labelAttributeMetadata = labelAttributeTransformer.Transform(targetSymbol);
            return new LabelQualifierMetadata(labelAttributeMetadata);
        }

        if (qualifierChecker?.HasAttribute(targetSymbol) == true) {
            var qualifierAttributeMetadata = qualifierAttributeTransformer.Transform(targetSymbol);
            return new CustomQualifierMetadata(qualifierAttributeMetadata);
        }

        return NoQualifierMetadata.Instance;
    }
}

