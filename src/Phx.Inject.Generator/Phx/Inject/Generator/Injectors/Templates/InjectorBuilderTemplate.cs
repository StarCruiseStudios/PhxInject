﻿// -----------------------------------------------------------------------------
//  <copyright file="InjectorBuilderTemplate.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Injectors.Templates {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Specifications.Templates;

    internal record InjectorBuilderTemplate(
            string BuiltTypeQualifiedName,
            string MethodName,
            string BuilderTargetName,
            SpecContainerBuilderInvocationTemplate SpecContainerBuilderInvocation,
            Location Location
    ) : IInjectorMemberTemplate {
        public void Render(IRenderWriter writer) {
            writer.AppendLine($"public void {MethodName}({BuiltTypeQualifiedName} {BuilderTargetName}) {{")
                    .IncreaseIndent(1);
            SpecContainerBuilderInvocation.Render(writer);
            writer.AppendLine(";")
                    .DecreaseIndent(1)
                    .AppendLine("}");
        }
    }
}