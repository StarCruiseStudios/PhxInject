using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata.Auto;

internal record AutoBuilderMetadata(
    string AutoBuilderMethodName,
    QualifiedTypeMetadata BuiltType,
    IEnumerable<QualifiedTypeMetadata> Parameters,
    AutoBuilderAttributeMetadata AutoBuilderAttributeMetadata,
    GeneratorIgnored<Location> Location
) : ISourceCodeElement { }