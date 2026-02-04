using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata.Injector;

internal record InjectorActivatorMetadata(
    string ActivatorMethodName,
    QualifiedTypeMetadata ActivatedType,
    GeneratorIgnored<Location> Location
): ISourceCodeElement { }