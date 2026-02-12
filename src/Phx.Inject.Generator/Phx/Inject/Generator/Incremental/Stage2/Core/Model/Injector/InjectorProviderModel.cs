// -----------------------------------------------------------------------------
// <copyright file="InjectorProviderModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Model.Injector;

/// <summary>
///     Stage 2 implementation model for generating provider methods in injector classes.
/// </summary>
/// <remarks>
///     <para><strong>Domain Model:</strong></para>
///     <para>
///         InjectorProviderModel represents a single provider method that will be generated in the
///         concrete injector class. It is the Stage 2 counterpart to 
///         <see cref="Stage1.Metadata.Model.Injector.InjectorProviderMetadata"/>, enriched with
///         resolution strategy information from specification analysis.
///     </para>
///     
///     <para><strong>Transformation from Metadata:</strong></para>
///     <para>
///         InjectorProviderMapper transforms Stage 1 metadata into this model:
///     </para>
///     <list type="number">
///         <item>Validates that ProvidedType matches a factory in the injector's specifications</item>
///         <item>Resolves qualified type (including [Label] qualifiers) to specific factory method</item>
///         <item>Determines which specification container owns the factory</item>
///         <item>Preserves method name for interface implementation</item>
///     </list>
///     
///     <para><strong>Code Generation Pattern:</strong></para>
///     <para>
///         Each InjectorProviderModel generates a method in the injector class that:
///     </para>
///     <list type="number">
///         <item>Matches the user's interface method signature (name + return type)</item>
///         <item>Delegates to the appropriate specification container's factory method</item>
///         <item>Returns the constructed instance to the caller</item>
///         <item>Benefits from singleton caching if the factory is scoped as singleton</item>
///     </list>
///     
///     <para><strong>Example Transformation:</strong></para>
///     <code>
///         // User writes (Stage 1 Metadata):
///         [Injector(typeof(DatabaseSpec))]
///         interface IDatabaseInjector {
///             [Label(DatabaseSpec.Primary)]
///             IDatabase GetPrimaryDatabase();
///         }
///         
///         // Analyzed as InjectorProviderMetadata:
///         - ProviderMethodName: "GetPrimaryDatabase"
///         - ProvidedType: QualifiedType(IDatabase, Label=Primary)
///         
///         // Mapped to InjectorProviderModel (this class):
///         - ProvidedType: QualifiedType(IDatabase, Label=Primary)
///         - ProviderMethodName: "GetPrimaryDatabase"
///         
///         // Generates (Stage 2 Output):
///         public IDatabase GetPrimaryDatabase() {
///             return this.databaseSpecContainer.Fac_L_Primary_IDatabase();
///         }
///     </code>
///     
///     <para><strong>Qualified Type Resolution:</strong></para>
///     <para>
///         The ProvidedType includes qualification metadata (labels) that directs the generator to
///         the correct factory method. For example:
///     </para>
///     <list type="bullet">
///         <item>Unqualified: IDatabase → Fac_L_None_IDatabase()</item>
///         <item>Single label: [Label(Primary)] IDatabase → Fac_L_Primary_IDatabase()</item>
///         <item>Multiple labels: [Label(Primary, ReadOnly)] IDatabase → Fac_L_Primary_ReadOnly_IDatabase()</item>
///     </list>
///     
///     <para><strong>Specification Container Delegation:</strong></para>
///     <para>
///         The generated method delegates to a specification container field. The container is
///         determined during Stage 2 transformation:
///     </para>
///     <list type="bullet">
///         <item>
///             <term>Local Construction:</term>
///             <description>
///                 If the factory is in a ConstructedSpecification, delegate to that container field.
///                 Example: this.requestSpecContainer.Fac_L_None_IUserContext()
///             </description>
///         </item>
///         <item>
///             <term>Parent Delegation:</term>
///             <description>
///                 If the factory is in a parent specification, delegate through the parent dependency
///                 interface. Example: this.parent.GetLogger() (which internally calls parent's container)
///             </description>
///         </item>
///     </list>
///     
///     <para><strong>Scope and Caching:</strong></para>
///     <para>
///         Provider methods inherit the scope behavior defined by the underlying factory:
///     </para>
///     <list type="bullet">
///         <item>
///             <term>Singleton:</term>
///             <description>
///                 Container caches the first invocation result and returns the same instance on
///                 subsequent calls. Cache is scoped to the container instance (injector lifetime).
///             </description>
///         </item>
///         <item>
///             <term>Transient:</term>
///             <description>
///                 Container creates a new instance on every invocation. No caching.
///             </description>
///         </item>
///         <item>
///             <term>Scoped (via child injectors):</term>
///             <description>
///                 Singleton within a child injector's lifetime but transient across different child
///                 instances. Example: DB transaction is singleton per request but different per request.
///             </description>
///         </item>
///     </list>
///     
///     <para><strong>Thread Safety:</strong></para>
///     <para>
///         Provider methods delegate to specification factories. If the factory is singleton-scoped,
///         the container implements thread-safe caching (typically via lazy initialization or locks).
///         If transient, each call is independent and thread-safe by nature.
///     </para>
///     
///     <para><strong>Relationship to Other Models:</strong></para>
///     <list type="bullet">
///         <item>
///             Contrast with <see cref="InjectorBuilderModel"/>: Providers return values, Builders
///             initialize existing objects (void return)
///         </item>
///         <item>
///             Contrast with <see cref="InjectorChildFactoryModel"/>: Providers return dependency
///             instances, ChildFactories return child injector instances
///         </item>
///         <item>
///             Used by: <see cref="InjectorModel.Providers"/> collection during code generation
///         </item>
///     </list>
/// </remarks>
/// <param name="ProvidedType"> 
///     The qualified type returned by this provider, including any [Label] qualifiers. Used to resolve
///     the correct specification factory method during code generation.
/// </param>
/// <param name="ProviderMethodName"> 
///     The method name from the user's interface (e.g., "GetPrimaryDatabase"). Used as-is in the
///     generated implementation to satisfy interface contract.
/// </param>
/// <param name="Location"> The source location where this provider is defined for diagnostics. </param>
internal record InjectorProviderModel(
    QualifiedTypeMetadata ProvidedType,
    string ProviderMethodName,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
