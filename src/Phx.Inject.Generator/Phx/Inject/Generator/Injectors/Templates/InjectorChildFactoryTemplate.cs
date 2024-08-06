﻿// -----------------------------------------------------------------------------
//  <copyright file="InjectorChildFactoryTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Injectors.Templates {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common.Templates;

    internal record InjectorChildFactoryTemplate(
        string ChildInterfaceTypeQualifiedName,
        string MethodName,
        string ChildTypeQualifiedName,
        IEnumerable<InjectorConstructorParameter> ConstructorParameters,
        IEnumerable<IInjectorChildConstructorArgumentTemplate> ChildConstructorArguments,
        Location Location
    ) : IInjectorMemberTemplate {
        public void Render(IRenderWriter writer) {
            writer.Append($"public {ChildInterfaceTypeQualifiedName} {MethodName}(");

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

            using (var collectionWriter = writer.GetCollectionWriter(
                new CollectionWriterProperties(
                    OpeningString: $"return new {ChildTypeQualifiedName}(",
                    ClosingString: ");",
                    CloseWithNewline: false))) {
                foreach (var arg in ChildConstructorArguments) {
                    var elementWriter = collectionWriter.GetElementWriter();
                    arg.Render(elementWriter);
                }
            }

            writer.AppendLine()
                .DecreaseIndent(1)
                .AppendLine("}");
        }
    }
}
