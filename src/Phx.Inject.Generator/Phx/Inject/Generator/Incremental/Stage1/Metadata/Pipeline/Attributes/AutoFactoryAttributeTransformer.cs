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
///     <para>Purpose - Auto-Generated Factory Injection:</para>
///     <para>
///     [AutoFactory] marks parameters that should receive an automatically-created factory for producing
///     the specified type. Instead of injecting an instance directly or requiring manual factory method
///     declaration, generator creates a factory on-demand. The transformer extracts optional FabricationMode
///     controlling how the auto-generated factory creates instances.
///     </para>
///     
///     <para>User Code Pattern - Implicit Factory Generation:</para>
///     <code>
///     [Specification]
///     public interface IServices {
///         // No explicit factory method needed!
///         [Factory]
///         IProcessor CreateProcessor(
///             [AutoFactory] Func&lt;IUserService&gt; userServiceFactory);
///         
///         // Generator auto-creates factory for IUserService
///         // even though no [Factory] IUserService method exists
///     }
///     </code>
///     <para>
///     Generator analyzes IUserService, determines how to construct it (constructor, dependencies, etc.),
///     and creates a factory delegate automatically. User doesn't need to write explicit factory method.
///     </para>
///     
///     <para>User Code Pattern - FabricationMode Configuration:</para>
///     <code>
///     [Specification]
///     public interface IServices {
///         [Factory]
///         ICache CreateCache(
///             [AutoFactory(FabricationMode.Scoped)]
///             Func&lt;IUserService&gt; userServiceFactory);
///     }
///     </code>
///     <para>
///     FabricationMode controls auto-factory behavior: Recurrent creates new instance per call,
///     Scoped caches first instance, Container/ContainerScoped handle child injector scoping.
///     </para>
///     
///     <para>FabricationMode Extraction - Identical to FactoryReference:</para>
///     <para>
///     Extraction logic mirrors FactoryReferenceAttributeTransformer:
///     </para>
///     <list type="number">
///         <item>
///             <term>Named Argument (Primary):</term>
///             <description>
///             `GetNamedArgument&lt;FabricationMode?&gt;(nameof(AutoFactoryAttribute.FabricationMode))`
///             extracts property-style argument. Returns null if unspecified.
///             </description>
///         </item>
///         <item>
///             <term>Constructor Argument (Fallback):</term>
///             <description>
///             `GetConstructorArgument&lt;FabricationMode&gt;(argument => argument.Type!.GetFullyQualifiedName() == FabricationModeClassName, default)`
///             finds enum by type checking. Returns default(0) if not found.
///             </description>
///         </item>
///     </list>
///     <para>
///     Type-based filtering ensures resilience across attribute signature changes (see FactoryReference docs).
///     </para>
///     
///     <para>Why AutoFactory Needs Special Handling - On-Demand Factory Creation:</para>
///     <para>
///     AutoFactory requires special generation because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             No explicit factory method exists in specification (unlike [FactoryReference])
///             </description>
///         </item>
///         <item>
///             <description>
///             Generator must analyze target type's constructor and dependencies automatically
///             </description>
///         </item>
///         <item>
///             <description>
///             Generated factory delegate captures all transitive dependencies from injector
///             </description>
///         </item>
///         <item>
///             <description>
///             FabricationMode affects generated closure (scoped requires field storage, recurrent doesn't)
///             </description>
///         </item>
///     </list>
///     
///     <para>AutoFactory vs FactoryReference - When To Use Each:</para>
///     <list type="bullet">
///         <item>
///             <term>FactoryReference:</term>
///             <description>
///             References an explicitly-declared factory method. Use when you want to control factory
///             implementation, add custom logic, or expose factory to external callers.
///             Example: [Factory] IUser CreateUser() { /* custom logic */ }
///             </description>
///         </item>
///         <item>
///             <term>AutoFactory:</term>
///             <description>
///             Generator creates factory automatically. Use when factory is purely dependency wiring
///             with no custom logic. Reduces boilerplate for simple construction scenarios.
///             Example: Just need Func&lt;IUser&gt; without explicit method.
///             </description>
///         </item>
///     </list>
///     
///     <para>FabricationMode Interpretation - Explicit Control:</para>
///     <list type="bullet">
///         <item>
///             <term>Default (0 or unspecified):</term>
///             <description>
///             Generator analyzes target type to determine mode. Sealed concrete classes default to
///             Constructor mode, interfaces/abstracts require explicit mode specification or error.
///             </description>
///         </item>
///         <item>
///             <term>Recurrent:</term>
///             <description>
///             Each factory call creates new instance. Generated: () => new TargetType(deps...)
///             No caching, maximum flexibility.
///             </description>
///         </item>
///         <item>
///             <term>Scoped:</term>
///             <description>
///             First factory call creates instance, subsequent calls return cached instance within scope.
///             Generated: Field storage + lazy initialization check.
///             </description>
///         </item>
///         <item>
///             <term>Container/ContainerScoped:</term>
///             <description>
///             Container-hierarchy scoping for child injectors (see ChildInjector docs).
///             </description>
///         </item>
///     </list>
///     
///     <para>Validation Constraints - Enforced by Later Stages:</para>
///     <para>
///     Transformer doesn't validate auto-factory generation feasibility. Later validation ensures:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Parameter type is Func&lt;T&gt; or compatible delegate type
///             </description>
///         </item>
///         <item>
///             <description>
///             Target type T has accessible constructor or static factory method
///             </description>
///         </item>
///         <item>
///             <description>
///             All transitive dependencies for T can be resolved from injector
///             </description>
///         </item>
///         <item>
///             <description>
///             FabricationMode is appropriate for target type (e.g., Constructor mode requires concrete class)
///             </description>
///         </item>
///         <item>
///             <description>
///             [AutoFactory] isn't combined with [FactoryReference] or [BuilderReference] on same parameter
///             </description>
///         </item>
///         <item>
///             <description>
///             No circular auto-factory chains (A auto-creates B, B auto-creates A)
///             </description>
///         </item>
///     </list>
///     
///     <para>Common Errors Prevented:</para>
///     <list type="bullet">
///         <item>
///             <term>Target type has no accessible constructor:</term>
///             <description>
///             Attempting [AutoFactory] for interface without explicit FabricationMode fails.
///             Validator requires either concrete class or StaticMethod mode specification.
///             </description>
///         </item>
///         <item>
///             <term>Missing transitive dependencies:</term>
///             <description>
///             Auto-factory for type requiring IDatabase when no IDatabase factory exists.
///             Validator walks entire dependency graph to ensure all required factories exist.
///             </description>
///         </item>
///         <item>
///             <term>Circular auto-factory chain:</term>
///             <description>
///             TypeA has [AutoFactory] parameter for TypeB, TypeB has [AutoFactory] for TypeA.
///             Validator detects cycles preventing infinite recursion.
///             </description>
///         </item>
///         <item>
///             <term>Wrong delegate signature:</term>
///             <description>
///             Using Func&lt;IUserService&gt; when generator can only create concrete UserServiceImpl.
///             Validator ensures delegate return type matches what generator can construct.
///             </description>
///         </item>
///         <item>
///             <term>Mixing factory attributes:</term>
///             <description>
///             Using both [AutoFactory] and [FactoryReference] on same parameter is ambiguous
///             (auto-create or reference existing?). Validator requires single factory strategy.
///             </description>
///         </item>
///     </list>
/// </remarks>
internal class AutoFactoryAttributeTransformer(
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
