using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata.Injector;

internal record InjectorProviderMetadata(
    string ProviderMethodName,
    QualifiedTypeMetadata ProvidedType,
    GeneratorIgnored<Location> Location
): ISourceCodeElement { }