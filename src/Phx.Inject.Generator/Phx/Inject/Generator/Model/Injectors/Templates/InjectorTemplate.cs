// -----------------------------------------------------------------------------
//  <copyright file="InjectorTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Injectors.Templates {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Input;
    using Phx.Inject.Generator.Model.Injectors.Definitions;
    using Phx.Inject.Generator.Model.Specifications;
    using Phx.Inject.Generator.Model.Specifications.Templates;

    internal delegate InjectorTemplate CreateInjectorTemplate(
            InjectorDefinition injectorDefinition,
            TemplateGenerationContext context);

    internal record InjectorTemplate(
            string InjectorClassName,
            string InjectorInterfaceQualifiedName,
            string SpecContainerCollectionTypeName,
            string SpecContainerCollectionReferenceName,
            IEnumerable<InjectorSpecContainerCollectionProperty> SpecContainerCollectionProperties,
            IEnumerable<InjectorConstructorParameter> ConstructorParameters,
            IEnumerable<IInjectorMemberTemplate> InjectorMemberTemplates,
            Location Location
    ) : IRenderTemplate {
        public void Render(IRenderWriter writer) {
            //  internal partial class InjectorClassName : InjectorInterfaceQualifiedName {
            writer.AppendLine($"internal partial class {InjectorClassName} : {InjectorInterfaceQualifiedName} {{")
                    .IncreaseIndent(1);

            //      internal record SpecContainerCollection(
            //              SpecContainerQualifiedName SpecContainerReference,
            //              SpecContainerQualifiedName2 SpecContainerReference2);
            using (var collectionWriter = writer.GetCollectionWriter(
                           new CollectionWriterProperties(
                                   OpeningString: $"internal record {SpecContainerCollectionTypeName} (",
                                   ClosingString: ");",
                                   CloseWithNewline: false))) {
                foreach (var property in SpecContainerCollectionProperties) {
                    var elementWriter = collectionWriter.GetElementWriter();
                    elementWriter.Append($"{property.PropertyTypeQualifiedName} {property.PropertyName}");
                }
            }

            writer.AppendBlankLine();

            //      private readonly SpecContainerCollection specContainers;
            writer.AppendLine(
                    $"private readonly {SpecContainerCollectionTypeName} {SpecContainerCollectionReferenceName};")
                    .AppendBlankLine();

            //      public InjectorClassName(
            //              ConstructedSpecificationQualifiedName constructedSpecificationReference
            //      ) {
            writer.Append($"public {InjectorClassName}(");

            if (ConstructorParameters.Any()) {
                using (var collectionWriter = writer.GetCollectionWriter(CollectionWriterProperties.Default)) {
                    foreach (var parameter in ConstructorParameters) {
                        var elementWriter = collectionWriter.GetElementWriter();
                        elementWriter.Append($"{parameter.ParameterTypeQualifiedName} {parameter.ParameterName}");
                    }
                }
            }

            writer.AppendLine(") {")
                    .IncreaseIndent(1);

            //          specContainers = new SpecContainerCollection(
            //                  SpecContainerReference: new SpecContainerQualifiedName(constructedSpecificationReference),
            //                  SpecContainerReference2: new SpecContainerQualifiedName2());
            using (var collectionWriter = writer.GetCollectionWriter(new CollectionWriterProperties(
                           OpeningString: $"{SpecContainerCollectionReferenceName} = new {SpecContainerCollectionTypeName}(",
                           ClosingString: ");",
                           CloseWithNewline: false
                           ))) {
                foreach (var property in SpecContainerCollectionProperties) {
                    var elementWriter = collectionWriter.GetElementWriter();
                    elementWriter.Append($"{property.PropertyName}: new {property.PropertyTypeQualifiedName}(");
                    if (property.ConstructorArgumentName != null) {
                        elementWriter.Append(property.ConstructorArgumentName);
                    }

                    elementWriter.Append(")");
                }
            }

            //      }
            writer.AppendLine()
                    .DecreaseIndent(1)
                    .AppendLine("}");

            //      public FactoryQualifiedReturnType FactoryName() {
            //          return specContainers.SpecContainerReference.FactoryName(specContainers);
            //      }
            //
            //      public void BuilderName(BuilderQualifiedType target) {
            //          specContainers.SpecContainerReference2.BuilderName(target, specContainers);
            //      }
            //
            //      public ChildInjectorQualifiedInterfaceType ChildFactoryName() {
            //          return new ChildInjectorQualifiedImplementationType(new ExternalDependencyImplementation(specContainers));
            //      }
            foreach (var memberTemplate in InjectorMemberTemplates) {
                writer.AppendBlankLine();
                memberTemplate.Render(writer);
            }

            // }
            writer.DecreaseIndent(1)
                    .AppendLine("}");
        }

        public class Builder {
            private const string SpecContainerCollectionReferenceName = "specContainers";

            public InjectorTemplate Build(
                    InjectorDefinition injectorDefinition,
                    TemplateGenerationContext context
            ) {
                var specContainers = injectorDefinition.Specifications.Select(
                        spec => context.GetSpecContainer(spec, injectorDefinition.Location));

                var specContainerProperties = specContainers.Select(
                                specContainer => {
                                    var constructorArgument = specContainer.SpecInstantiationMode ==
                                            SpecInstantiationMode.Static
                                                    ? null
                                                    : specContainer.SpecificationType.GetVariableName();
                                    return new InjectorSpecContainerCollectionProperty(
                                            specContainer.SpecContainerType.QualifiedName,
                                            specContainer.SpecContainerType.GetPropertyName(),
                                            constructorArgument);
                                })
                        .ToImmutableList();

                var constructorParameters = specContainers.Where(
                                specContainer =>
                                        specContainer.SpecInstantiationMode == SpecInstantiationMode.Instantiated)
                        .Select(specContainer => specContainer.SpecificationType)
                        .Select(
                                specType => new InjectorConstructorParameter(
                                        specType.QualifiedName,
                                        specType.GetVariableName()))
                        .ToImmutableList();

                IEnumerable<IInjectorMemberTemplate> injectorMemberTemplates = injectorDefinition.Providers.Select(
                                provider => {
                                    var invocationDefinition = provider.SpecContainerFactoryInvocation;
                                    var factoryInvocation = new SpecContainerFactoryInvocationTemplate(
                                            SpecContainerCollectionReferenceName,
                                            invocationDefinition.SpecContainerType.GetPropertyName(),
                                            invocationDefinition.FactoryMethodName,
                                            invocationDefinition.Location);

                                    return new InjectorProviderTemplate(
                                            provider.ProvidedType.TypeModel.QualifiedName,
                                            provider.InjectorProviderMethodName,
                                            factoryInvocation,
                                            provider.Location);
                                })
                        .Concat<IInjectorMemberTemplate>(
                                injectorDefinition.Builders.Select(
                                        builder => {
                                            var builderTargetName = "target";
                                            var invocationDefinition = builder.SpecContainerBuilderInvocation;
                                            var builderInvocation = new SpecContainerBuilderInvocationTemplate(
                                                    SpecContainerCollectionReferenceName,
                                                    invocationDefinition.SpecContainerType.GetPropertyName(),
                                                    invocationDefinition.BuilderMethodName,
                                                    builderTargetName,
                                                    invocationDefinition.Location);

                                            return new InjectorBuilderTemplate(
                                                    builder.BuiltType.TypeModel.QualifiedName,
                                                    builder.InjectorBuilderMethodName,
                                                    builderTargetName,
                                                    builderInvocation,
                                                    builder.Location);
                                        }))
                        .Concat(
                                injectorDefinition.ChildFactories.Select(
                                        factory => {
                                            var childInjector = context.GetInjector(factory.InjectorChildInterfaceType, factory.Location);
                                            var childTypeQualifiedName = childInjector.InjectorType.QualifiedName;

                                            // Name of the generated class that implements the external dependency interface.
                                            var childExternalDependencies = childInjector.ExternalDependencies
                                                    .Select(
                                                            externalDependencyInterfaceType =>
                                                                    context.GetExternalDependency(
                                                                            externalDependencyInterfaceType,
                                                                            childInjector.Location))
                                                    .Select(
                                                            externalDependencyImplementation =>
                                                                    externalDependencyImplementation
                                                                            .ExternalDependencyImplementationType
                                                                            .QualifiedName)
                                                    .ToImmutableList();

                                            return new InjectorChildFactoryTemplate(
                                                    factory.InjectorChildInterfaceType.QualifiedName,
                                                    factory.InjectorChildFactoryMethodName,
                                                    childTypeQualifiedName,
                                                    childExternalDependencies,
                                                    SpecContainerCollectionReferenceName,
                                                    factory.Location);
                                        }))
                        .ToImmutableList();

                return new InjectorTemplate(
                        injectorDefinition.InjectorType.TypeName,
                        injectorDefinition.InjectorInterfaceType.QualifiedName,
                        SymbolProcessors.SpecContainerCollectionTypeName,
                        SpecContainerCollectionReferenceName,
                        specContainerProperties,
                        constructorParameters,
                        injectorMemberTemplates,
                        injectorDefinition.Location);
            }
        }
    }
}
