// -----------------------------------------------------------------------------
//  <copyright file="SourceGenerator.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

// #define ATTACH_DEBUGGER

namespace Phx.Inject.Generator {
#if ATTACH_DEBUGGER
    using System.Threading;
#endif
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Construct;
    using Phx.Inject.Generator.Construct.Definitions;
    using Phx.Inject.Generator.Extract;
    using Phx.Inject.Generator.Extract.Model;
    using Phx.Inject.Generator.Map;
    using Phx.Inject.Generator.Render;
    using Phx.Inject.Generator.Render.Templates;

    [Generator]
    internal class SourceGenerator : ISourceGenerator {
        private const string GeneratedFileExtension = "generated.cs";

        private IModelExtractor<SpecificationModel> SpecificationExtractor { get; }
        private IModelExtractor<InjectorModel> InjectorExtractor { get; }
        private IInjectionMapper InjectionMapper { get; }
        private IFileTemplateBuilder<SpecContainerDefinition> SpecContainerTemplateBuilder { get; }
        private IFileTemplateBuilder<InjectorDefinition> InjectorTemplateBuilder { get; }

        private ITemplateRenderer TemplateRenderer { get; }

        public SourceGenerator(
            IModelExtractor<SpecificationModel> specificationExtractor,
            IModelExtractor<InjectorModel> injectorExtractor,
            IInjectionMapper injectionMapper,
            IFileTemplateBuilder<SpecContainerDefinition> specContainerTemplateBuilder,
            IFileTemplateBuilder<InjectorDefinition> injectorTemplateBuilder,
            ITemplateRenderer templateRenderer
        ) {
            SpecificationExtractor = specificationExtractor;
            InjectorExtractor = injectorExtractor;
            InjectionMapper = injectionMapper;
            SpecContainerTemplateBuilder = specContainerTemplateBuilder;
            InjectorTemplateBuilder = injectorTemplateBuilder;
            TemplateRenderer = templateRenderer;
        }

        public SourceGenerator() : this(
            specificationExtractor: new ModelExtractor<SpecificationModel>(
                new SpecificationSymbolRecognizer(),
                new SpecificationModelBuilder()),
            injectorExtractor: new ModelExtractor<InjectorModel>(
                new InjectorSymbolRecognizer(),
                new InjectorModelBuilder()),
            injectionMapper: new InjectionMapper(
                new InjectorMapper(),
                new SpecContainerMapper()
            ),
            specContainerTemplateBuilder: new SpecContainerTemplateBuilder(
                new InstanceHolderDeclarationTemplateBuilder(),
                new FactoryMethodContainerTemplateBuilder(
                    new FactoryMethodContainerInvocationTemplateBuilder()),
                new BuilderMethodContainerTemplateBuilder(
                    new FactoryMethodContainerInvocationTemplateBuilder())),
            injectorTemplateBuilder: new InjectorTemplateBuilder(
                new InjectorMethodTemplateBuilder(
                    new FactoryMethodContainerInvocationTemplateBuilder()),
                new InjectorBuilderMethodTemplateBuilder(
                    new BuilderMethodContainerInvocationTemplateBuilder())),
            templateRenderer: new TemplateRenderer(() => new RenderWriter())
        ) { }

        public void Initialize(GeneratorInitializationContext context) {
#if ATTACH_DEBUGGER
            System.Diagnostics.Debugger.Launch();
            while (!System.Diagnostics.Debugger.IsAttached) {
                Thread.Sleep(500);
            }
#endif
            context.RegisterForSyntaxNotifications(() => new InjectorSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context) {
            try {
                InjectorSyntaxReceiver syntaxReceiver = context.SyntaxReceiver as InjectorSyntaxReceiver
                    ?? throw new InvalidOperationException("Incorrect Syntax Receiver."); // This should never happen.

                // Extract
                var injectorModels = InjectorExtractor.Extract(syntaxReceiver.InjectorCandidates, context);
                var specModels = SpecificationExtractor.Extract(syntaxReceiver.SpecificationCandidates, context);

                foreach (var injectorModel in injectorModels) {
                    // Map
                    var injectionDefinition = InjectionMapper.MapToDefinition(injectorModel, specModels);

                    // Construct
                    var templates = new List<(TypeDefinition, IRenderTemplate)>();
                    foreach (var specDefinition in injectionDefinition.SpecContainers) {
                        templates.Add((specDefinition.ContainerType, SpecContainerTemplateBuilder.Build(specDefinition)));
                    }
                    templates.Add((injectorModel.InjectorType.ToTypeDefinition(), InjectorTemplateBuilder.Build(injectionDefinition.Injector)));

                    // Render
                    if (!Directory.Exists(RenderConstants.GeneratedSourceDir)) {
                        Directory.CreateDirectory(RenderConstants.GeneratedSourceDir);
                    }

                    foreach (var (classType, template) in templates) {
                        var fileName = $"{classType.QualifiedName}.{GeneratedFileExtension}";
                        TemplateRenderer.RenderTemplate(fileName, template, context);
                    }
                }
            } catch (Exception ex) {
                Console.Error.WriteLine(ex);
                throw;
            }
        }
    }
}
