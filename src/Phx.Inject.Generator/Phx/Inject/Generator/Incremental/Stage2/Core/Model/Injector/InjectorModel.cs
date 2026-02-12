// -----------------------------------------------------------------------------
// <copyright file="InjectorModel.cs" company="Star Cruise Studios LLC">
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
///     Stage 2 implementation model for generating concrete injector container classes.
/// </summary>
/// <remarks>
///     <para><strong>Domain Model:</strong></para>
///     <para>
///         InjectorModel represents the complete blueprint for generating a DI container implementation
///         class. It transforms Stage 1's user-facing interface metadata into an actionable model that
///         drives code generation. Each InjectorModel produces one concrete class that implements the
///         user's injector interface.
///     </para>
///     
///     <para><strong>Stage 1 → Stage 2 Transformation:</strong></para>
///     <para>
///         InjectorMapper transforms <see cref="Stage1.Metadata.Model.Injector.InjectorInterfaceMetadata"/> 
///         into this model:
///     </para>
///     <list type="number">
///         <item>
///             <term>Type Resolution:</term>
///             <description>
///                 Generates implementation class name from interface + specifications. 
///                 Example: IApplicationInjector + AppSpec → ApplicationInjector_AppSpec
///             </description>
///         </item>
///         <item>
///             <term>Specification Analysis:</term>
///             <description>
///                 Separates "constructed" specs (injector creates instances) from "referenced" specs
///                 (accessed via parent dependency). Constructed specs become constructor parameters
///                 and private fields.
///             </description>
///         </item>
///         <item>
///             <term>Method Mapping:</term>
///             <description>
///                 Providers → InjectorProviderModel, Activators → InjectorBuilderModel,
///                 ChildProviders → InjectorChildFactoryModel. Each mapper enriches metadata with
///                 resolution strategy.
///             </description>
///         </item>
///         <item>
///             <term>Dependency Wiring:</term>
///             <description>
///                 If [Dependency] present, adds parent injector as constructor parameter and field,
///                 enabling child-to-parent delegation.
///             </description>
///         </item>
///     </list>
///     
///     <para><strong>Generated Class Structure:</strong></para>
///     <code>
///         // Example generated implementation for:
///         // [Injector(typeof(RequestSpec))]
///         // [Dependency(typeof(IApplicationDependencies))]
///         // interface IRequestInjector { ... }
///         
///         internal class RequestInjector_RequestSpec : IRequestInjector {
///             // Fields (from ConstructedSpecifications)
///             private readonly RequestSpec_Container requestSpecContainer;
///             
///             // Parent reference (from Dependency)
///             private readonly IApplicationDependencies parent;
///             
///             // External dependencies (from child provider parameters)
///             private readonly HttpContext httpContext;
///             
///             // Constructor
///             public RequestInjector_RequestSpec(
///                 IApplicationDependencies parent,
///                 HttpContext httpContext) {
///                 this.parent = parent;
///                 this.httpContext = httpContext;
///                 
///                 // Initialize specification containers
///                 this.requestSpecContainer = new RequestSpec_Container(
///                     parent,           // Parent dependency interface
///                     httpContext       // External parameter
///                 );
///             }
///             
///             // Provider method implementations (from Providers)
///             public IUserContext GetUserContext() {
///                 return this.requestSpecContainer.Fac_L_None_IUserContext();
///             }
///             
///             public IDbTransaction GetTransaction() {
///                 return this.requestSpecContainer.Fac_L_None_IDbTransaction();
///             }
///             
///             // Builder method implementations (from Builders)
///             public void InitializeService(MyService service) {
///                 this.requestSpecContainer.Bld_L_None_MyService(
///                     service,
///                     this.GetUserContext()
///                 );
///             }
///             
///             // Child factory method implementations (from ChildFactories)
///             public IOperationInjector CreateOperation(Command command) {
///                 return new OperationInjector_OperationSpec(
///                     this,           // Pass self as parent
///                     command         // Pass context parameter
///                 );
///             }
///         }
///     </code>
///     
///     <para><strong>Specification Container Pattern:</strong></para>
///     <para>
///         Each specification type referenced by the injector generates a corresponding "container"
///         class (e.g., RequestSpec → RequestSpec_Container). These containers encapsulate:
///     </para>
///     <list type="bullet">
///         <item>Factory methods (Fac_L_[Label]_[Type]) for object construction</item>
///         <item>Builder methods (Bld_L_[Label]_[Type]) for object initialization</item>
///         <item>Singleton caching logic for scope management</item>
///         <item>Dependency resolution via constructor-injected parent/parameters</item>
///     </list>
///     <para>
///         The injector class aggregates these containers via private fields (ConstructedSpecifications)
///         and delegates provider/builder calls to them. This separation enables:
///     </para>
///     <list type="bullet">
///         <item>Modular code generation (one template per concern)</item>
///         <item>Specification reuse across injectors</item>
///         <item>Clear lifetime boundaries (container = scope)</item>
///     </list>
///     
///     <para><strong>Code Generation Flow:</strong></para>
///     <para>
///         InjectorTemplate (in Legacy pipeline) or equivalent Stage 2 emitter consumes this model:
///     </para>
///     <list type="number">
///         <item>Render class declaration with InjectorType name and InjectorInterfaceType interface</item>
///         <item>Render fields for ConstructedSpecifications + Dependency (if present)</item>
///         <item>Render constructor accepting Dependency + ConstructedSpecifications + external params</item>
///         <item>Render provider methods (Providers) delegating to spec containers</item>
///         <item>Render builder methods (Builders) delegating to spec containers</item>
///         <item>Render child factory methods (ChildFactories) instantiating child injectors</item>
///     </list>
///     
///     <para><strong>Specifications vs ConstructedSpecifications:</strong></para>
///     <list type="bullet">
///         <item>
///             <term>Specifications (all):</term>
///             <description>
///                 Complete set of specification types referenced by this injector, including both
///                 locally constructed and parent-accessed specs. Used for dependency analysis and
///                 imports.
///             </description>
///         </item>
///         <item>
///             <term>ConstructedSpecifications (subset):</term>
///             <description>
///                 Only specifications that this injector instantiates. Each becomes a private field
///                 and constructor parameter. Parent-provided specs are accessed via Dependency
///                 interface, not constructed locally.
///             </description>
///         </item>
///     </list>
///     
///     <para><strong>Scope and Lifetime:</strong></para>
///     <list type="bullet">
///         <item>
///             <term>Injector Lifetime:</term>
///             <description>
///                 Controlled by caller. Root injectors typically live for application lifetime.
///                 Child injectors live for request/operation scope. Injector disposal should dispose
///                 owned ConstructedSpecifications.
///             </description>
///         </item>
///         <item>
///             <term>Singleton Caching:</term>
///             <description>
///                 Managed by specification containers. Each spec container maintains singleton
///                 caches scoped to the container instance. Child injectors have separate containers,
///                 so child singletons don't interfere with parent singletons.
///             </description>
///         </item>
///         <item>
///             <term>Hierarchical Access:</term>
///             <description>
///                 Child injectors access parent dependencies via Dependency interface, which
///                 delegates to parent injector's methods. This maintains encapsulation while
///                 enabling scope inheritance.
///             </description>
///         </item>
///     </list>
///     
///     <para><strong>Relationship to Metadata:</strong></para>
///     <para>
///         This is the Stage 2 counterpart to <see cref="Stage1.Metadata.Model.Injector.InjectorInterfaceMetadata"/>.
///         Metadata captures "what the user wrote", Model captures "what we will generate".
///     </para>
/// </remarks>
/// <param name="InjectorType"> 
///     The generated implementation class type (e.g., "RequestInjector_RequestSpec"). Synthesized
///     from interface name + specification types by InjectorMapper.
/// </param>
/// <param name="InjectorInterfaceType"> 
///     The user-defined interface type (e.g., "IRequestInjector"). The generated class implements this.
/// </param>
/// <param name="Specifications"> 
///     All specification types referenced by this injector, including parent-accessed specs. Used for
///     imports and dependency analysis. Superset of ConstructedSpecifications.
/// </param>
/// <param name="ConstructedSpecifications"> 
///     Specifications instantiated by this injector (subset of Specifications). Each becomes a private
///     field in the generated class. Parent-provided specs are excluded.
/// </param>
/// <param name="Dependency"> 
///     Optional parent injector dependency interface (e.g., "IApplicationDependencies"). If present,
///     generates a constructor parameter and field for accessing parent-provided dependencies.
/// </param>
/// <param name="Providers"> 
///     Provider method models to generate. Each produces a method that constructs and returns an object
///     by delegating to a specification container's factory method.
/// </param>
/// <param name="Builders"> 
///     Builder method models to generate. Each produces a method that initializes an existing object
///     by delegating to a specification container's builder method.
/// </param>
/// <param name="ChildFactories"> 
///     Child factory method models to generate. Each produces a method that instantiates a child
///     injector, passing this injector as parent and forwarding context parameters.
/// </param>
/// <param name="Location"> The source location where this injector is defined for diagnostics. </param>
internal record InjectorModel(
    TypeMetadata InjectorType,
    TypeMetadata InjectorInterfaceType,
    IEnumerable<TypeMetadata> Specifications,
    IEnumerable<TypeMetadata> ConstructedSpecifications,
    TypeMetadata? Dependency,
    IEnumerable<InjectorProviderModel> Providers,
    IEnumerable<InjectorBuilderModel> Builders,
    IEnumerable<InjectorChildFactoryModel> ChildFactories,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
