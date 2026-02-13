// -----------------------------------------------------------------------------
// <copyright file="BuilderReferenceAttributeTransformer.cs" company="Star Cruise Studios LLC">
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
///     Transforms BuilderReference attribute data into metadata.
/// </summary>
/// <remarks>
///     <para>Purpose - Manual Builder Wiring:</para>
///     <para>
///     [BuilderReference] marks parameters that should receive an existing builder instance rather than
///     creating a new one. This enables factory methods to accept externally-created builders as dependencies,
///     allowing manual builder configuration before using them in factory methods. Opposite of [AutoBuilder]
///     which generates the builder internally.
///     </para>
///     
///     <para>User Code Pattern - External Builder Injection:</para>
///     <code>
///     [Specification]
///     public interface IServices {
///         [Builder] IUserBuilder CreateUserBuilder();
///         
///         // Factory receives pre-configured builder as parameter
///         [Factory]
///         IUser CreateUser([BuilderReference] IUserBuilder builder);
///         
///         // External code controls builder configuration:
///         // var builder = services.CreateUserBuilder();
///         // builder.WithName("Alice").WithAge(30);
///         // var user = services.CreateUser(builder);
///     }
///     </code>
///     <para>
///     Without [BuilderReference], generator would treat IUserBuilder parameter as a dependency to inject,
///     potentially creating circular dependency. [BuilderReference] tells generator "this is user-provided."
///     </para>
///     
///     <para>Why BuilderReference Needs Special Handling - Dependency vs Parameter:</para>
///     <para>
///     BuilderReference requires special handling because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Normally, factory parameters are dependencies to be resolved from the injector
///             </description>
///         </item>
///         <item>
///             <description>
///             [BuilderReference] marks parameter as "pass-through" - not injected, but provided by caller
///             </description>
///         </item>
///         <item>
///             <description>
///             Generated factory method signature includes builder parameter (not hidden like injected dependencies)
///             </description>
///         </item>
///         <item>
///             <description>
///             Prevents attempting to resolve builder from injector (would fail or recurse)
///             </description>
///         </item>
///     </list>
///     
///     <para>No Arguments - Simple Marker Semantics:</para>
///     <para>
///     BuilderReference has no configuration arguments because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Behavior is binary: parameter is either external reference or injected dependency
///             </description>
///         </item>
///         <item>
///             <description>
///             Builder type (IUserBuilder) is determined by parameter type, not attribute
///             </description>
///         </item>
///         <item>
///             <description>
///             No lifecycle or scope configuration needed (builder is externally managed)
///             </description>
///         </item>
///     </list>
///     
///     <para>BuilderReference vs FactoryReference - Parallel Patterns:</para>
///     <para>
///     BuilderReference for builders parallels FactoryReference for factories:
///     </para>
///     <list type="bullet">
///         <item>
///             <term>BuilderReference:</term>
///             <description>
///             Parameter receives externally-configured builder instance. Caller controls builder state
///             before passing to factory.
///             </description>
///         </item>
///         <item>
///             <term>FactoryReference:</term>
///             <description>
///             Parameter receives externally-invoked factory method result. Caller controls instantiation
///             timing and configuration.
///             </description>
///         </item>
///     </list>
///     <para>
///     Both prevent generator from treating parameter as injected dependency.
///     </para>
///     
///     <para>Validation Constraints - Enforced by Later Stages:</para>
///     <para>
///     Transformer doesn't validate builder usage. Later validation ensures:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Parameter type is actually a builder (has [Builder] specification method)
///             </description>
///         </item>
///         <item>
///             <description>
///             [BuilderReference] isn't used with [FactoryReference] on same parameter (mutually exclusive)
///             </description>
///         </item>
///         <item>
///             <description>
///             Builder specification exists in accessible scope (not private/internal when cross-assembly)
///             </description>
///         </item>
///         <item>
///             <description>
///             [BuilderReference] isn't used with other dependency attributes like [Label] or [Qualifier]
///             (reference parameters aren't injected, so qualifiers don't apply)
///             </description>
///         </item>
///     </list>
///     
///     <para>Common Errors Prevented:</para>
///     <list type="bullet">
///         <item>
///             <term>Circular builder dependency:</term>
///             <description>
///             Without [BuilderReference], generator tries to inject IUserBuilder into CreateUser,
///             which requires CreateUserBuilder factory, which might depend on CreateUser (circular).
///             [BuilderReference] breaks the cycle by making parameter external.
///             </description>
///         </item>
///         <item>
///             <term>Missing [BuilderReference] on builder parameter:</term>
///             <description>
///             Forgetting [BuilderReference] causes generator to search for builder factory,
///             potentially creating unexpected dependency chains. Explicit marking prevents confusion.
///             </description>
///         </item>
///         <item>
///             <term>Using [BuilderReference] on non-builder:</term>
///             <description>
///             Validator catches applying [BuilderReference] to regular dependencies (IUserService),
///             which should use [FactoryReference] or be injected normally.
///             </description>
///         </item>
///     </list>
/// </remarks>
internal class BuilderReferenceAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<BuilderReferenceAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static BuilderReferenceAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, BuilderReferenceAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<BuilderReferenceAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            BuilderReferenceAttributeMetadata.AttributeClassName
        );
        
        return new BuilderReferenceAttributeMetadata(attributeMetadata).ToOkResult();
    }
}
