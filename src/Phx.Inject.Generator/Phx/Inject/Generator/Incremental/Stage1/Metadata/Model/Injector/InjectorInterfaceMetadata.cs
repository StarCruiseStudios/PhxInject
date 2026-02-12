// -----------------------------------------------------------------------------
// <copyright file="InjectorInterfaceMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;

/// <summary>
///     Stage 1 metadata representing a user-defined injector interface contract.
/// </summary>
/// <remarks>
///     <para>Domain Model:</para>
///     <para>
///         An injector interface is the user-facing API for a dependency injection container. Users write
///         interfaces decorated with [Injector] that declare what objects they want to retrieve from the
///         container. This metadata captures the analyzed contract before transformation to implementation.
///     </para>
///     
///     <para>Design Rationale:</para>
///     <list type="bullet">
///         <item>
///             <term>Interface-First Design:</term>
///             <description>
///                 Users define contracts, not implementations. The generator creates concrete classes
///                 that implement these interfaces, ensuring compile-time safety and testability.
///             </description>
///         </item>
///         <item>
///             <term>Method Classification:</term>
///             <description>
///                 Three distinct method types serve different lifecycle patterns:
///                 - Providers: Return constructed instances (factory pattern)
///                 - Activators: Initialize existing objects (builder pattern)
///                 - ChildProviders: Create scoped sub-containers (hierarchy pattern)
///             </description>
///         </item>
///         <item>
///             <term>Specification Binding:</term>
///             <description>
///                 The [Injector] attribute references specification types that define the dependency
///                 graph. This separates "what to inject" (specification) from "how to access" (injector).
///             </description>
///         </item>
///     </list>
///     
///     <para>Example User Code:</para>
///     <code>
///         [Injector(typeof(AppSpecification))]
///         public interface IApplicationInjector {
///             // Provider: Returns a constructed database instance
///             IDatabase GetDatabase();
///             
///             // Activator: Initializes an existing service
///             void InitializeService(MyService service);
///             
///             // ChildProvider: Creates a scoped request-level container
///             [ChildInjector]
///             IRequestInjector CreateRequestScope(RequestContext context);
///         }
///     </code>
///     
///     <para>Stage 1 → Stage 2 Transformation:</para>
///     <para>
///         InjectorMapper transforms this metadata into <see cref="Stage2.Core.Model.Injector.InjectorModel"/>,
///         which drives code generation of the implementation class (e.g., ApplicationInjector_AppSpecification).
///     </para>
///     
///     <para>Generated Implementation Structure:</para>
///     <list type="bullet">
///         <item>Constructor: Accepts parent injector (if [Dependency] present) + specifications</item>
///         <item>Fields: One per constructed specification, initialized in constructor</item>
///         <item>Provider Methods: Delegate to specification factories, with caching for singletons</item>
///         <item>Activator Methods: Call specification builders with dependency resolution</item>
///         <item>ChildProvider Methods: Instantiate child injector, passing parent as dependency</item>
///     </list>
///     
///     <para>Scope and Lifetime:</para>
///     <para>
///         Injectors themselves are stateful containers. If [Dependency] is present, this is a child
///         injector that receives its parent as a constructor parameter, enabling hierarchical scopes
///         (e.g., Application → Request → Operation). The parent-child relationship allows:
///     </para>
///     <list type="bullet">
///         <item>Scope isolation: Child cannot access parent's transient state</item>
///         <item>Dependency delegation: Child requests parent-provided types via dependency interface</item>
///         <item>Lifetime control: Parent outlives children, child disposal doesn't affect parent</item>
///     </list>
/// </remarks>
/// <param name="InjectorInterfaceType"> The type metadata of the injector interface (e.g., IApplicationInjector). </param>
/// <param name="Providers"> Provider methods that return constructed objects (factory pattern). </param>
/// <param name="Activators"> Activator methods that initialize existing objects (builder pattern). </param>
/// <param name="ChildProviders"> Child provider methods that create scoped sub-containers (hierarchy pattern). </param>
/// <param name="InjectorAttributeMetadata"> 
///     The [Injector] attribute metadata, containing specification types that define the dependency graph. 
/// </param>
/// <param name="DependencyAttributeMetadata"> 
///     Optional [Dependency] attribute metadata. If present, this is a child injector that receives
///     a parent injector reference. The dependency type exposes parent-provided factories.
/// </param>
/// <param name="Location"> The source location of the interface definition for diagnostics. </param>
internal record InjectorInterfaceMetadata(
    TypeMetadata InjectorInterfaceType,
    EquatableList<InjectorProviderMetadata> Providers,
    EquatableList<InjectorActivatorMetadata> Activators,
    EquatableList<InjectorChildProviderMetadata> ChildProviders,
    InjectorAttributeMetadata InjectorAttributeMetadata,
    DependencyAttributeMetadata? DependencyAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
