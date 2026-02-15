// -----------------------------------------------------------------------------
// <copyright file="AutoBuilderAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms AutoBuilder attribute data into metadata.
/// </summary>
/// <remarks>
///     Marker attribute for parameters requiring automatically-generated builder instances.
///     <c>[AutoBuilder]</c> enables fluent construction patterns without manual builder definition.
///     Generator analyzes builder interface and creates implementation with fluent methods. Has no
///     configuration arguments - builder behavior derived entirely from interface definition.
/// </remarks>
///             </description>
///         </item>
///     </list>
///     
///     <para>Builder Generation Logic - Interface Method Analysis:</para>
///     <para>
///     Generator analyzes builder interface to create implementation:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Methods returning void or builder type become property setters (WithName(string name))
///             </description>
///         </item>
///         <item>
///             <description>
///             Build() method (returns target type) becomes construction method using accumulated state
///             </description>
///         </item>
///         <item>
///             <description>
///             Generated class has fields storing builder state (name, age, etc.)
///             </description>
///         </item>
///         <item>
///             <description>
///             Fluent methods return this for chaining: builder.WithName("Alice").WithAge(30)
///             </description>
///         </item>
///     </list>
///     
///     <para>Validation Constraints - Enforced by Later Stages:</para>
///     <para>
///     Transformer doesn't validate builder generation feasibility. Later validation ensures:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Parameter type is valid builder interface (has methods with builder pattern)
///             </description>
///         </item>
///         <item>
///             <description>
///             Builder interface is accessible (not private/internal when cross-assembly)
///             </description>
///         </item>
///         <item>
///             <description>
///             Builder interface has Build() or similar construction method returning target type
///             </description>
///         </item>
///         <item>
///             <description>
///             Target type constructed by Build() has accessible constructor or factory
///             </description>
///         </item>
///         <item>
///             <description>
///             All transitive dependencies for target type can be resolved from injector
///             </description>
///         </item>
///         <item>
///             <description>
///             [AutoBuilder] isn't combined with [BuilderReference], [FactoryReference], or [AutoFactory]
///             </description>
///         </item>
///     </list>
///     
///     <para>Common Errors Prevented:</para>
///     <list type="bullet">
///         <item>
///             <term>Builder interface has no Build method:</term>
///             <description>
///             Builder without construction method can't be used to create target instance.
///             Validator requires method returning target type (conventional name: Build()).
///             </description>
///         </item>
///         <item>
///             <term>Builder methods have incompatible signatures:</term>
///             <description>
///             Methods that don't follow fluent pattern (return wrong type, unexpected parameters)
///             can't be auto-implemented. Validator checks method signatures match builder conventions.
///             </description>
///         </item>
///         <item>
///             <term>Target type cannot be constructed:</term>
///             <description>
///             Builder builds UserImpl but UserImpl has private constructor with no static factory.
///             Validator ensures target type is constructible via accessible mechanism.
///             </description>
///         </item>
///         <item>
///             <term>Missing builder dependencies:</term>
///             <description>
///             Builder's Build() method needs IDatabase but no IDatabase factory exists.
///             Validator walks transitive dependencies ensuring all are resolvable.
///             </description>
///         </item>
///         <item>
///             <term>Mixing builder attributes:</term>
///             <description>
///             Using both [AutoBuilder] and [BuilderReference] on same parameter is contradictory
///             (auto-create or reference existing?). Validator requires single builder strategy.
///             </description>
///         </item>
///     </list>
/// </remarks>
internal sealed class AutoBuilderAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<AutoBuilderAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static AutoBuilderAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, AutoBuilderAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<AutoBuilderAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            AutoBuilderAttributeMetadata.AttributeClassName
        );
        
        return new AutoBuilderAttributeMetadata(attributeMetadata).ToOkResult();
    }
}
