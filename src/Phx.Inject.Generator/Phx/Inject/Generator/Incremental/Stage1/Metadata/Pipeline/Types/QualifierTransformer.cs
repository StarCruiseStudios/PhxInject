// -----------------------------------------------------------------------------
// <copyright file="QualifierTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Types;

internal class QualifierTransformer(
    IAttributeTransformer<LabelAttributeMetadata> labelAttributeTransformer,
    IAttributeTransformer<QualifierAttributeMetadata> qualifierAttributeTransformer
) : ITransformer<ISymbol, IQualifierMetadata> {
    public static QualifierTransformer Instance { get; } = new(
        LabelAttributeTransformer.Instance,
        QualifierAttributeTransformer.Instance
    );
    
    public bool CanTransform(ISymbol input) {
        return true;
    }
    
    public IResult<IQualifierMetadata> Transform(ISymbol targetSymbol) {
        return DiagnosticsRecorder.Capture<IQualifierMetadata>(diagnostics => {
            if (labelAttributeTransformer.HasAttribute(targetSymbol)) {
                var labelAttributeMetadata = labelAttributeTransformer.Transform(targetSymbol).OrThrow(diagnostics);
                return new LabelQualifierMetadata(labelAttributeMetadata);
            }

            if (qualifierAttributeTransformer.HasAttribute(targetSymbol)) {
                var qualifierAttributeMetadata = qualifierAttributeTransformer.Transform(targetSymbol).OrThrow(diagnostics);
                return new CustomQualifierMetadata(qualifierAttributeMetadata);
            }

            return NoQualifierMetadata.Instance;
        });
    }
}

