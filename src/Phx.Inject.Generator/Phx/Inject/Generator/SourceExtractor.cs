// -----------------------------------------------------------------------------
// <copyright file="SourceExtractor.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Common;
using Phx.Inject.Generator.Descriptors;

namespace Phx.Inject.Generator;

internal class SourceExtractor {
    public SourceDescriptor Extract(SourceSyntaxReceiver syntaxReceiver, GeneratorExecutionContext context) {
        try {
            var descriptorGenerationContext = new DescriptorGenerationContext(context);

            var injectorDescriptors = new InjectorExtractor().Extract(
                syntaxReceiver.InjectorCandidates,
                descriptorGenerationContext);
            Logger.Info($"Discovered {injectorDescriptors.Count} injector types.");

            var specDescriptors = new SpecExtractor().Extract(
                syntaxReceiver.SpecificationCandidates,
                descriptorGenerationContext);
            Logger.Info($"Discovered {specDescriptors.Count} specification types.");

            var externalDependencyDescriptors = new ExternalDependencyExtractor().Extract(
                syntaxReceiver.InjectorCandidates,
                descriptorGenerationContext);
            Logger.Info($"Discovered {externalDependencyDescriptors.Count} external dependency types.");

            return new SourceDescriptor(
                injectorDescriptors: injectorDescriptors,
                specDescriptors: specDescriptors,
                externalDependencyDescriptors: externalDependencyDescriptors
            );
        } catch (Exception e) {
            var diagnosticData = (e is InjectionException ie)
                ? ie.DiagnosticData
                : Diagnostics.UnexpectedError;
            
            throw new InjectionException(
                diagnosticData,
                $"An error occurred while extracting source descriptors.",
                Location.None,
                e);
        }
    }
}
