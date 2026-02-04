using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata.Injector;

internal record InjectorDependencyFactoryPropertyMetadata(
    string FactoryPropertyName,
    QualifiedTypeMetadata FactoryReturnType,
    FactoryAttributeMetadata FactoryAttributeMetadata,
    GeneratorIgnored<Location> Location
) : ISourceCodeElement { }