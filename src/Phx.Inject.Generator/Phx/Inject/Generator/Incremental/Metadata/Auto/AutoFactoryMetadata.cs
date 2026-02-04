using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata.Auto;

internal record AutoFactoryMetadata( 
    QualifiedTypeMetadata AutoFactoryType,
    IEnumerable<QualifiedTypeMetadata> Parameters,
    IEnumerable<AutoFactoryRequiredPropertyMetadata> RequiredProperties,
    AutoFactoryAttributeMetadata AutoFactoryAttributeMetadata,
    GeneratorIgnored<Location> Location
) : ISourceCodeElement { }