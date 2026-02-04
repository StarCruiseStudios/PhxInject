using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata.Auto;

internal record AutoFactoryRequiredPropertyMetadata(
    string RequiredPropertyName,
    QualifiedTypeMetadata RequiredPropertyType,
    GeneratorIgnored<Location> Location
) : ISourceCodeElement { }