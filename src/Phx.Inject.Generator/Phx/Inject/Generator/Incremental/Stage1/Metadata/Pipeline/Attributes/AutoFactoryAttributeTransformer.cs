// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Util;
using static Phx.Inject.Generator.Incremental.PhxInject;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms AutoFactory attribute data into metadata.
/// </summary>
/// <remarks>
///     Extracts optional <c>FabricationMode</c> for parameters receiving auto-generated factory
///     delegates. Generator analyzes target type constructor/dependencies and creates factory
///     on-demand without explicit <c>[Factory]</c> method. Tries named argument first, then
///     constructor argument filtered by type for signature resilience. Inverse of
///     <c>[FactoryReference]</c> (references explicit factory methods).
///
///     ## FabricationMode Options
///
///     - **Transient**: Each factory call creates a new instance. No storage needed.
///     - **Scoped**: First factory call creates instance, subsequent calls return cached instance within scope.
///       Generated: Field storage + lazy initialization check.
///     - **Container/ContainerScoped**: Container-hierarchy scoping for child injectors (see ChildInjector docs).
///     
///     ## Validation Constraints - Enforced by Later Stages
///
///     Transformer doesn't validate auto-factory generation feasibility. Later validation ensures:
///
///     - Parameter type is Func&lt;T&gt; or compatible delegate type
///     - Target type T has accessible constructor or static factory method
///     - All transitive dependencies for T can be resolved from injector
///     - FabricationMode is appropriate for target type (e.g., Constructor mode requires concrete class)
///     - [AutoFactory] isn't combined with [FactoryReference] or [BuilderReference] on same parameter
///     - No circular auto-factory chains (A auto-creates B, B auto-creates A)
///     
///     ## Common Errors Prevented
///
///     - **Target type has no accessible constructor**: Attempting [AutoFactory] for interface without explicit FabricationMode fails.
///       Validator requires either concrete class or StaticMethod mode specification.
///     - **Missing transitive dependencies**: Auto-factory for type requiring IDatabase when no IDatabase factory exists.
///       Validator walks entire dependency graph to ensure all required factories exist.
///     - **Circular auto-factory chain**: TypeA has [AutoFactory] parameter for TypeB, TypeB has [AutoFactory] for TypeA.
///       Validator detects cycles preventing infinite recursion.
///     - **Wrong delegate signature**: Using Func&lt;IUserService&gt; when generator can only create concrete UserServiceImpl.
///       Validator ensures delegate return type matches what generator can construct.
/// </remarks>
internal sealed class AutoFactoryAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<AutoFactoryAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static AutoFactoryAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    private const string FabricationModeClassName = $"{NamespaceName}.{nameof(FabricationMode)}";

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, AutoFactoryAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<AutoFactoryAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            AutoFactoryAttributeMetadata.AttributeClassName
        );

        var fabricationMode =
            attributeData.GetNamedArgument<FabricationMode?>(nameof(AutoFactoryAttribute.FabricationMode))
            ?? attributeData.GetConstructorArgument<FabricationMode>(argument =>
                argument.Type!.GetFullyQualifiedName() == FabricationModeClassName,
                default);

        return new AutoFactoryAttributeMetadata(fabricationMode, attributeMetadata).ToOkResult();
    }
}
