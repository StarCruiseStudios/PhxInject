// -----------------------------------------------------------------------------
//  <copyright file="InjectorDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Descriptors {
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Model;

    internal record InjectorDescriptor(
        TypeModel InjectorInterfaceType,
        string GeneratedInjectorTypeName,
        IEnumerable<TypeModel> SpecificationsTypes,
        IEnumerable<TypeModel> ExternalDependencyInterfaceTypes,
        IEnumerable<InjectorProviderDescriptor> Providers,
        IEnumerable<InjectorBuilderDescriptor> Builders,
        IEnumerable<InjectorChildFactoryDescriptor> ChildFactories,
        Location Location
    ) : IDescriptor {
        public TypeModel InjectorType { get; } = InjectorInterfaceType with {
            BaseTypeName = GeneratedInjectorTypeName,
            TypeArguments = ImmutableList<TypeModel>.Empty
        };
        
        public interface IBuilder {
            InjectorDescriptor Build(
                ITypeSymbol injectorInterfaceSymbol,
                DescriptorGenerationContext context
            );
        }

        public class Builder : IBuilder {
            private readonly InjectorBuilderDescriptor.IBuilder injectorBuilderDescriptorBuilder;
            private readonly InjectorChildFactoryDescriptor.IBuilder injectorChildFactoryDescriptorBuilder;
            private readonly InjectorProviderDescriptor.IBuilder injectorProviderDescriptionBuilder;

            public Builder(
                InjectorProviderDescriptor.IBuilder injectorProviderDescriptionBuilder,
                InjectorBuilderDescriptor.IBuilder injectorBuilderDescriptorBuilder,
                InjectorChildFactoryDescriptor.IBuilder injectorChildFactoryDescriptorBuilder
            ) {
                this.injectorProviderDescriptionBuilder = injectorProviderDescriptionBuilder;
                this.injectorBuilderDescriptorBuilder = injectorBuilderDescriptorBuilder;
                this.injectorChildFactoryDescriptorBuilder = injectorChildFactoryDescriptorBuilder;
            }
            
            public Builder() : this(
                new InjectorProviderDescriptor.Builder(),
                new InjectorBuilderDescriptor.Builder(),
                new InjectorChildFactoryDescriptor.Builder()) { }

            public InjectorDescriptor Build(
                ITypeSymbol injectorInterfaceSymbol,
                DescriptorGenerationContext context
            ) {
                var injectorInterfaceType = TypeModel.FromTypeSymbol(injectorInterfaceSymbol);
                var generatedInjectorTypeName = MetadataHelpers.GetGeneratedInjectorClassName(injectorInterfaceSymbol);

                var externalDependencyInterfaceTypes = MetadataHelpers
                    .GetExternalDependencyTypes(injectorInterfaceSymbol)
                    .Select(TypeModel.FromTypeSymbol)
                    .ToImmutableList();

                var specificationTypes = MetadataHelpers.GetInjectorSpecificationTypes(injectorInterfaceSymbol)
                    .Select(TypeModel.FromTypeSymbol)
                    .Concat(externalDependencyInterfaceTypes)
                    .ToImmutableList();

                var injectorMethods = injectorInterfaceSymbol
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .ToImmutableList();
                
                var providers = injectorMethods
                    .Select(method => injectorProviderDescriptionBuilder.Build(method, context))
                    .Where(provider => provider != null)
                    .Select(provider => provider!)
                    .ToImmutableList();

                var builders = injectorMethods
                    .Select(method => injectorBuilderDescriptorBuilder.Build(method, context))
                    .Where(builder => builder != null)
                    .Select(builder => builder!)
                    .ToImmutableList();

                var childFactories = injectorMethods
                    .Select(method => injectorChildFactoryDescriptorBuilder.Build(method, context))
                    .Where(childFactory => childFactory != null)
                    .Select(childFactory => childFactory!)
                    .ToImmutableList();

                return new InjectorDescriptor(
                    injectorInterfaceType,
                    generatedInjectorTypeName,
                    specificationTypes,
                    externalDependencyInterfaceTypes,
                    providers,
                    builders,
                    childFactories,
                    injectorInterfaceSymbol.Locations.First());
            }
        }
    }
}
