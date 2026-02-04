using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata.Injector;

internal record InjectorChildProviderMetadata(
    string ChildProviderMethodName,
    TypeMetadata ChildInjectorType,
    IReadOnlyList<TypeMetadata> Parameters,
    ChildInjectorAttributeMetadata ChildInjectorAttribute,
    GeneratorIgnored<Location> Location
): ISourceCodeElement { }