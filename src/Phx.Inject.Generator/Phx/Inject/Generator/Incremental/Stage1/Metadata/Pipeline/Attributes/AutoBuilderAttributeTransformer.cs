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
///     <para><b>Purpose - Auto-Generated Builder Injection:</b></para>
///     <para>
///     [AutoBuilder] marks parameters that should receive an automatically-created builder instance
///     for constructing the specified type. Instead of requiring explicit builder method declaration,
///     generator creates a builder on-demand. This enables fluent construction patterns without manual
///     builder definition boilerplate.
///     </para>
///     
///     <para><b>User Code Pattern - Implicit Builder Generation:</b></para>
///     <code>
///     [Specification]
///     public interface IServices {
///         // No explicit builder method needed!
///         [Factory]
///         IUser CreateUser([AutoBuilder] IUserBuilder builder);
///         
///         // Generator auto-creates IUserBuilder implementation
///         // even though no [Builder] IUserBuilder method exists
///     }
///     
///     // External code:
///     // var userBuilder = /* auto-created by generator */;
///     // userBuilder.WithName("Alice").WithAge(30);
///     // var user = services.CreateUser(userBuilder);
///     </code>
///     <para>
///     Generator analyzes IUserBuilder interface, generates implementation with fluent methods,
///     and provides it to factory. User doesn't write explicit builder specification method.
///     </para>
///     
///     <para><b>Why AutoBuilder Has No Arguments - Pure Auto-Generation:</b></para>
///     <para>
///     Unlike AutoFactory (which has FabricationMode), AutoBuilder has no configuration because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Builders don't have fabrication modes - they're always created fresh per request
///             </description>
///         </item>
///         <item>
///             <description>
///             Builder behavior (fluent methods, property setters) is determined entirely by builder
///             interface definition
///             </description>
///         </item>
///         <item>
///             <description>
///             No lifecycle concerns - builders are transient by nature (configure once, build once, discard)
///             </description>
///         </item>
///         <item>
///             <description>
///             All generation logic derived from analyzing IUserBuilder interface methods
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Why AutoBuilder Needs Special Handling - Builder Interface Analysis:</b></para>
///     <para>
///     AutoBuilder requires special generation because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             No explicit [Builder] method exists in specification (unlike [BuilderReference])
///             </description>
///         </item>
///         <item>
///             <description>
///             Generator must analyze builder interface to determine what properties/methods to generate
///             </description>
///         </item>
///         <item>
///             <description>
///             Generated builder class implements fluent interface (methods return this)
///             </description>
///         </item>
///         <item>
///             <description>
///             Builder construction logic must be integrated into factory parameter injection
///             </description>
///         </item>
///         <item>
///             <description>
///             Generated Build() method must construct target type using accumulated configuration
///             </description>
///         </item>
///     </list>
///     
///     <para><b>AutoBuilder vs BuilderReference - When To Use Each:</b></para>
///     <list type="bullet">
///         <item>
///             <term>BuilderReference:</term>
///             <description>
///             References explicitly-declared builder method. Use when you want builder to be part of
///             specification's public API, or when builder needs custom implementation logic.
///             Example: [Builder] IUserBuilder CreateUserBuilder() returns manually-implemented builder.
///             Factory receives external builder: [Factory] IUser CreateUser([BuilderReference] IUserBuilder).
///             </description>
///         </item>
///         <item>
///             <term>AutoBuilder:</term>
///             <description>
///             Generator creates builder automatically. Use when builder is pure fluent wrapper with
///             no custom logic. Reduces boilerplate when builder is only used internally.
///             Example: [Factory] IUser CreateUser([AutoBuilder] IUserBuilder) - builder auto-created.
///             </description>
///         </item>
///     </list>
///     
///     <para><b>No Configuration Arguments - Simplicity by Design:</b></para>
///     <para>
///     [AutoBuilder] is a marker attribute with no arguments because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Builder interface itself is the complete specification (methods = properties to set)
///             </description>
///         </item>
///         <item>
///             <description>
///             No ambiguity about behavior - fluent pattern is standardized
///             </description>
///         </item>
///         <item>
///             <description>
///             Reduces cognitive load - attribute presence is only signal needed
///             </description>
///         </item>
///     </list>
///     
///     <para><b>Builder Generation Logic - Interface Method Analysis:</b></para>
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
///     <para><b>Validation Constraints - Enforced by Later Stages:</b></para>
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
///     <para><b>Common Errors Prevented:</b></para>
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
///         <item>
///             <term>Non-interface builder type:</term>
///             <description>
///             [AutoBuilder] on concrete class parameter is invalid (can't auto-implement concrete class).
///             Validator requires interface types for auto-builder generation.
///             </description>
///         </item>
///     </list>
/// </remarks>
internal class AutoBuilderAttributeTransformer(
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
