// -----------------------------------------------------------------------------
// <copyright file="BuilderAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

/// <summary>
///     Metadata representing an analyzed [Builder] attribute that marks a method as a
///     dependency initializer in the DI framework.
/// </summary>
/// <param name="AttributeMetadata">
///     The common attribute metadata (class name, target, locations) shared by all attributes.
/// </param>
/// <remarks>
///     <para><b>Role in DI Framework:</b></para>
///     <para>
///     Represents a user-declared builder method that initializes an existing object by injecting
///     dependencies into its properties or fields. Builders complement Factories by enabling
///     property injection patterns rather than constructor injection. They are essential for
///     initializing objects that cannot use constructor injection (e.g., objects created by
///     third-party factories, or classes with complex initialization sequences).
///     </para>
///     
///     <para><b>What User Declarations Represent:</b></para>
///     <para>
///     When users write "[Builder] static void Initialize(MyClass target, IDependency dep) { target.Dep = dep; }",
///     this metadata captures that declaration. The method signature defines what type is being
///     initialized (first parameter) and what dependencies are injected (remaining parameters).
///     Builders must return void and accept the target instance as the first parameter.
///     </para>
///     
///     <para><b>Why These Properties Were Chosen:</b></para>
///     <para>
///     Unlike FactoryAttributeMetadata which includes FabricationMode, builders have no lifetime
///     semantics because they operate on existing instances. The builder's behavior is purely
///     determined by its method signature. Therefore, only the base AttributeMetadata is needed
///     for diagnostics and cache keying. The method signature analysis happens in BuilderModel,
///     which extracts parameter types and validates the void return and target parameter.
///     </para>
///     
///     <para><b>Code Generation Needs:</b></para>
///     <para>
///     Code generation requires knowing which methods are builders (vs factories) to generate
///     appropriate invocation code. Builders generate void method calls in injector activator
///     methods, while factories generate return-value assignments or field caching. The lack
///     of additional properties reflects that the distinction is binary: either it's a builder
///     (void, target-first) or it's not.
///     </para>
///     
///     <para><b>Immutability Requirements:</b></para>
///     <para>
///     Contains only the immutable AttributeMetadata record, making this a stable cache key
///     for incremental compilation. Changes to the builder method signature affect the
///     BuilderModel (extracted in later stages) rather than this attribute metadata, correctly
///     localizing cache invalidation to signature changes.
///     </para>
///     
///     <para><b>Relationship to Other Models:</b></para>
///     <list type="bullet">
///         <item>
///             <description>
///             Used by BuilderModel which combines this metadata with method signature analysis
///             </description>
///         </item>
///         <item>
///             <description>
///             Distinguished from FactoryAttributeMetadata by initialization vs creation semantics
///             </description>
///         </item>
///         <item>
///             <description>
///             Related to SpecificationAttributeMetadata as builders are defined within specifications
///             </description>
///         </item>
///         <item>
///             <description>
///             Used by InjectorModel to generate activator methods that call builders
///             </description>
///         </item>
///     </list>
/// </remarks>
internal record BuilderAttributeMetadata(
    AttributeMetadata AttributeMetadata
) : IAttributeElement {
    public const string AttributeClassName = $"{PhxInject.NamespaceName}.{nameof(BuilderAttribute)}";
    
    public GeneratorIgnored<LocationInfo?> Location { get; } = AttributeMetadata.Location;
}