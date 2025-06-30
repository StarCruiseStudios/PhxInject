// -----------------------------------------------------------------------------
//  <copyright file="InjectorTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Map.Definitions;

namespace Phx.Inject.Generator.Project.Templates;

internal record InjectorTemplate(
    string InjectorClassName,
    string InjectorInterfaceQualifiedName,
    string SpecContainerCollectionTypeName,
    string SpecContainerCollectionReferenceName,
    IEnumerable<InjectorTemplate.InjectorSpecContainerCollectionProperty> SpecContainerCollectionProperties,
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
        //              SpecContainerQualifiedName2 SpecContainerReference2
        //      ) {
        using (var collectionWriter = writer.GetCollectionWriter(
            new CollectionWriterProperties(
                OpeningString: $"internal record {SpecContainerCollectionTypeName} ("))) {
            foreach (var property in SpecContainerCollectionProperties) {
                var elementWriter = collectionWriter.GetElementWriter();
                elementWriter.Append($"{property.PropertyTypeQualifiedName} {property.PropertyName}");
            }
        }

        writer.Append(") {")
            .IncreaseIndent(1)
            .AppendLine();

        //          internal SpecContainerCollection CreateNewFrame() {
        //              return new SpecContainerCollection(
        //                      SpecContainerReference.CreateNewFrame(),
        //                      SpecContainerReference2.CreateNewFrame());
        //          }
        //      }
        writer.Append($"internal {SpecContainerCollectionTypeName} CreateNewFrame() {{")
            .IncreaseIndent(1)
            .AppendLine();

        if (SpecContainerCollectionProperties.Any()) {
            using (var collectionWriter = writer.GetCollectionWriter(
                new CollectionWriterProperties(
                    OpeningString: $"return new {SpecContainerCollectionTypeName}(",
                    ClosingString: ");",
                    CloseWithNewline: false))) {
                foreach (var property in SpecContainerCollectionProperties) {
                    var elementWriter = collectionWriter.GetElementWriter();
                    elementWriter.Append($"{property.PropertyName}.CreateNewFrame()");
                }
            }
        } else {
            writer.Append("return this;");
        }

        writer.DecreaseIndent(1)
            .AppendLine()
            .Append("}")
            .DecreaseIndent(1)
            .AppendLine()
            .Append("}");

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
        using (var collectionWriter = writer.GetCollectionWriter(
            new CollectionWriterProperties(
                OpeningString:
                $"{SpecContainerCollectionReferenceName} = new {SpecContainerCollectionTypeName}(",
                ClosingString: ");",
                CloseWithNewline: false))) {
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
        //          return new ChildInjectorQualifiedImplementationType(new DependencyImplementation(specContainers));
        //      }
        foreach (var memberTemplate in InjectorMemberTemplates) {
            writer.AppendBlankLine();
            memberTemplate.Render(writer);
        }

        // }
        writer.DecreaseIndent(1)
            .AppendLine("}");
    }

    public record InjectorSpecContainerCollectionProperty(
        string PropertyTypeQualifiedName,
        string PropertyName,
        string? ConstructorArgumentName
    );
    
    public interface IBuilder {
        InjectorTemplate Build(
            InjectorDef injectorDef,
            TemplateGenerationContext context
        );
    }

    public class Builder : IBuilder {
        private const string SpecContainerCollectionReferenceName = "specContainers";

        public InjectorTemplate Build(
            InjectorDef injectorDef,
            TemplateGenerationContext context
        ) {
            IReadOnlyList<SpecContainerDef> specContainers = context.SpecContainers.Values.ToImmutableList();
            IReadOnlyList<InjectorSpecContainerCollectionProperty> specContainerProperties = specContainers
                .Select(specContainer => {
                    var constructorArgument = specContainer.SpecInstantiationMode == SpecInstantiationMode.Static
                        ? null
                        : specContainer.SpecificationType.GetVariableName();
                    return new InjectorSpecContainerCollectionProperty(
                        specContainer.SpecContainerType.QualifiedName,
                        specContainer.SpecContainerType.GetPropertyName(),
                        constructorArgument);
                })
                .ToImmutableList();

            IReadOnlyList<InjectorConstructorParameter> constructorParameters = specContainers.Where(specContainer =>
                    specContainer.SpecInstantiationMode == SpecInstantiationMode.Instantiated)
                .Select(specContainer => specContainer.SpecificationType)
                .Select(specType => new InjectorConstructorParameter(
                    specType.QualifiedName,
                    specType.GetVariableName()))
                .ToImmutableList();

            IReadOnlyList<IInjectorMemberTemplate> injectorMemberTemplates = injectorDef.Providers.Select(provider => {
                    var invocationDef = provider.SpecContainerFactoryInvocation;

                    IReadOnlyList<SpecContainerFactorySingleInvocationTemplate> singleInvocationTemplates =
                        invocationDef
                            .FactoryInvocationDefs.Select(def => {
                                return new SpecContainerFactorySingleInvocationTemplate(
                                    SpecContainerCollectionReferenceName,
                                    def.SpecContainerType.GetPropertyName(),
                                    def.FactoryMethodName,
                                    def.Location
                                );
                            })
                            .ToImmutableList();

                    string? multiBindQualifiedTypeArgs = null;
                    if (invocationDef.FactoryInvocationDefs.Count > 1) {
                        multiBindQualifiedTypeArgs =
                            TypeHelpers.GetQualifiedTypeArgs(invocationDef.FactoryReturnType);
                    }

                    var factoryInvocation = new SpecContainerFactoryInvocationTemplate(
                        singleInvocationTemplates,
                        multiBindQualifiedTypeArgs,
                        invocationDef.RuntimeFactoryProvidedType?.QualifiedName,
                        invocationDef.Location);

                    return new InjectorProviderTemplate(
                        provider.ProvidedType.TypeModel.QualifiedName,
                        provider.InjectorProviderMethodName,
                        factoryInvocation,
                        provider.Location);
                })
                .Concat<IInjectorMemberTemplate>(
                    injectorDef.Builders.Select(builder => {
                        var builderTargetName = "target";
                        var invocationDef = builder.SpecContainerBuilderInvocation;
                        var builderInvocation = new SpecContainerBuilderInvocationTemplate(
                            SpecContainerCollectionReferenceName,
                            invocationDef.SpecContainerType.GetPropertyName(),
                            invocationDef.BuilderMethodName,
                            builderTargetName,
                            invocationDef.Location);

                        return new ActivatorTemplate(
                            builder.BuiltType.TypeModel.QualifiedName,
                            builder.ActivatorMethodName,
                            builderTargetName,
                            builderInvocation,
                            builder.Location);
                    }))
                .Concat(
                    injectorDef.ChildFactories.Select(factory => {
                        var childInjector = context.GetInjector(
                            factory.InjectorChildInterfaceType,
                            factory.Location);
                        var childTypeQualifiedName = childInjector.InjectorType.QualifiedName;

                        IReadOnlyList<TypeModel> missingDependencies =
                            childInjector.ConstructedSpecifications.Where(specType =>
                                    !factory.Parameters.Contains(specType))
                                .ToImmutableList();
                        if (missingDependencies.Any()) {
                            var constructedSpecificationsString = string.Join(",",
                                childInjector.ConstructedSpecifications.Select(spec =>
                                    spec.ToString()));
                            var missingDependenciesString = string.Join(",",
                                missingDependencies.Select(dep => dep.ToString()));

                            throw new InjectionException(
                                Diagnostics.InvalidSpecification,
                                $"Child Injector factory must contain parameters for each of the child injector's constructed specification types: {constructedSpecificationsString}."
                                + $" Missing: {missingDependenciesString}.",
                                factory.Location);
                        }

                        IReadOnlyList<TypeModel> unusedParameters = factory.Parameters.Where(paramType =>
                                !childInjector.ConstructedSpecifications
                                    .Contains(paramType))
                            .ToImmutableList();
                        if (unusedParameters.Any()) {
                            var unusedParametersString = string.Join(",",
                                unusedParameters.Select(dep => dep.ToString()));
                            throw new InjectionException(
                                Diagnostics.InvalidSpecification,
                                $"Child Injector factory contains unused parameters: {unusedParametersString}.",
                                factory.Location);
                        }

                        // Use factory.Parameters instead of childInjector.ConstructedSpecifications
                        // to guarantee the ordering is the same as the interface. Validation logic
                        // above checks the two lists are otherwise identical.
                        IReadOnlyList<InjectorConstructorParameter> childConstructorParameters = factory.Parameters
                            .Select(specType => new InjectorConstructorParameter(
                                specType.QualifiedName,
                                specType.GetVariableName()))
                            .ToImmutableList();

                        var specConstructorArgs =
                            childConstructorParameters.Select(parameter =>
                                new InjectorChildConstructedSpecConstructorArgumentTemplate(
                                    parameter.ParameterName,
                                    parameter.ParameterName,
                                    childInjector.Location));
                        var dependencyConstructorArgs =
                            childInjector.Dependencies
                                .Select(dependencyInterfaceType => {
                                    var dependencyImplementation =
                                        context.GetDependency(
                                            dependencyInterfaceType,
                                            childInjector.Location);
                                    var dependencyImplementationQualifiedName =
                                        dependencyImplementation
                                            .DependencyImplementationType
                                            .QualifiedName;

                                    return new
                                        InjectorChildDependencyConstructorArgumentTemplate(
                                            dependencyInterfaceType
                                                .GetVariableName(),
                                            dependencyImplementationQualifiedName,
                                            SpecContainerCollectionReferenceName,
                                            childInjector.Location);
                                });

                        IReadOnlyList<IInjectorChildConstructorArgumentTemplate> args = specConstructorArgs
                            .Concat<IInjectorChildConstructorArgumentTemplate>(
                                dependencyConstructorArgs)
                            .ToImmutableList();

                        return new InjectorChildFactoryTemplate(
                            factory.InjectorChildInterfaceType.QualifiedName,
                            factory.InjectorChildFactoryMethodName,
                            childTypeQualifiedName,
                            childConstructorParameters,
                            args,
                            factory.Location);
                    }))
                .ToImmutableList();

            return new InjectorTemplate(
                injectorDef.InjectorType.TypeName,
                injectorDef.InjectorInterfaceType.QualifiedName,
                NameHelpers.SpecContainerCollectionTypeName,
                SpecContainerCollectionReferenceName,
                specContainerProperties,
                constructorParameters,
                injectorMemberTemplates,
                injectorDef.Location);
        }
    }
}
