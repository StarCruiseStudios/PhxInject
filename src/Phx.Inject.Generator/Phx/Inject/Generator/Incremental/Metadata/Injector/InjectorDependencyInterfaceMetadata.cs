using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Metadata.Attributes;
using Phx.Inject.Generator.Incremental.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Metadata.Injector;

internal record InjectorDependencyInterfaceMetadata(
    TypeMetadata InjectorDependencyInterfaceType,
    IEnumerable<SpecFactoryMethodMetadata> FactoryMethods,
    IEnumerable<SpecFactoryPropertyMetadata> FactoryProperties,
    InjectorDependencyAttributeMetadata InjectorDependencyAttributeMetadata,
    GeneratorIgnored<Location> Location
) : ISourceCodeElement { }
