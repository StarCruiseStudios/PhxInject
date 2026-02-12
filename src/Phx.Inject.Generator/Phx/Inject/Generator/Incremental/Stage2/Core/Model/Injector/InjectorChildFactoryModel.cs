// -----------------------------------------------------------------------------
// <copyright file="InjectorChildFactoryModel.cs" company="Star Cruise Studios LLC">
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
///     Stage 2 implementation model for generating child injector factory methods.
/// </summary>
/// <remarks>
///     <para>Domain Model:</para>
///     <para>
///         InjectorChildFactoryModel represents a factory method that creates hierarchical child
///         injector instances. It is the Stage 2 counterpart to 
///         <see cref="Stage1.Metadata.Model.Injector.InjectorChildProviderMetadata"/>, which uses
///         "ChildProvider" while Stage 2 uses "ChildFactory" to emphasize the factory pattern.
///     </para>
///     
///     <para>Hierarchical Dependency Injection:</para>
///     <para>
///         Child factories enable scoped sub-containers that inherit access to parent dependencies
///         while maintaining isolated state. This models real-world scope hierarchies:
///     </para>
///     <list type="bullet">
///         <item>Application scope (root) → Request scope (child) → Operation scope (grandchild)</item>
///         <item>Desktop app (root) → Window (child) → Dialog (grandchild)</item>
///         <item>Game engine (root) → Scene (child) → Entity (grandchild)</item>
///     </list>
///     
///     <para>Transformation from Metadata:</para>
///     <para>
///         InjectorChildProviderMapper transforms Stage 1 metadata into this model:
///     </para>
///     <list type="number">
///         <item>Validates child injector interface exists and is properly decorated</item>
///         <item>Resolves child's generated implementation class name</item>
///         <item>Validates parameter types match child's expected external dependencies</item>
///         <item>Renames method: ChildProviderMethodName → ChildFactoryMethodName</item>
///     </list>
///     
///     <para>Code Generation Pattern:</para>
///     <para>
///         Each InjectorChildFactoryModel generates a method in the parent injector class that:
///     </para>
///     <list type="number">
///         <item>Accepts context parameters (e.g., HttpContext, Command, RequestData)</item>
///         <item>Instantiates the child injector's implementation class</item>
///         <item>Passes parent (this) as first constructor argument to child</item>
///         <item>Passes context parameters as subsequent constructor arguments to child</item>
///         <item>Returns the child injector interface reference</item>
///     </list>
///     
///     <para>Example Transformation:</para>
///     <code>
///         // Parent Interface (Stage 1 Metadata):
///         [Injector(typeof(ApplicationSpec))]
///         public interface IApplicationInjector {
///             [ChildInjector]
///             IRequestInjector CreateRequest(HttpContext httpContext);
///         }
///         
///         // Child Interface:
///         [Injector(typeof(RequestSpec))]
///         [Dependency(typeof(IApplicationDependencies))]
///         public interface IRequestInjector { ... }
///         
///         // Analyzed as InjectorChildProviderMetadata:
///         - ChildProviderMethodName: "CreateRequest"
///         - ChildInjectorType: IRequestInjector
///         - Parameters: [HttpContext]
///         
///         // Mapped to InjectorChildFactoryModel (this class):
///         - ChildFactoryMethodName: "CreateRequest"
///         - ChildInjectorType: IRequestInjector
///         - Parameters: [HttpContext]
///         
///         // Generates in ApplicationInjector_ApplicationSpec (Stage 2 Output):
///         public IRequestInjector CreateRequest(HttpContext httpContext) {
///             return new RequestInjector_RequestSpec(
///                 this,           // Parent implements IApplicationDependencies
///                 httpContext     // External context parameter
///             );
///         }
///         
///         // Child Constructor (in RequestInjector_RequestSpec):
///         public RequestInjector_RequestSpec(
///             IApplicationDependencies parent,
///             HttpContext httpContext) {
///             this.parent = parent;
///             this.httpContext = httpContext;
///             this.requestSpecContainer = new RequestSpec_Container(parent, httpContext);
///         }
///     </code>
///     
///     <para>Parent-Child Dependency Flow:</para>
///     <para>
///         The [Dependency] attribute on the child interface specifies the parent dependency contract.
///         This enables compile-time safe access to parent-provided dependencies:
///     </para>
///     <list type="number">
///         <item>
///             <term>Define Parent Dependency Interface:</term>
///             <description>
///                 Create [InjectorDependency] interface exposing parent's providers as [Factory] methods.
///                 Example: IApplicationDependencies { [Factory] ILogger GetLogger(); }
///             </description>
///         </item>
///         <item>
///             <term>Link Child to Parent:</term>
///             <description>
///                 Decorate child with [Dependency(typeof(IApplicationDependencies))]. Parent
///                 injector implementation automatically implements this interface.
///             </description>
///         </item>
///         <item>
///             <term>Runtime Resolution:</term>
///             <description>
///                 Child requests dependency: ILogger logger = parent.GetLogger()
///                 Parent interface delegates: return this.parentInjector.GetLogger()
///                 Parent injector resolves: return this.appSpecContainer.Fac_L_None_ILogger()
///             </description>
///         </item>
///     </list>
///     
///     <para>Parameter Propagation (External Dependencies):</para>
///     <para>
///         Parameters represent context-specific data passed from parent to child. These become
///         available as "external dependencies" in the child's specifications:
///     </para>
///     <code>
///         // Parent passes HttpContext to child
///         IRequestInjector child = parent.CreateRequest(httpContext);
///         
///         // Child's specification can access httpContext
///         [Specification]
///         class RequestSpec {
///             [Factory]
///             public static IUserContext CreateUserContext(HttpContext httpContext) {
///                 // httpContext is automatically resolved from constructor parameter
///                 return new UserContext(httpContext.User);
///             }
///         }
///     </code>
///     <para>
///         The child's SpecContainer constructor receives the httpContext parameter and makes it
///         available to all factories/builders in that specification.
///     </para>
///     
///     <para>Scope Isolation and Lifetime:</para>
///     <list type="bullet">
///         <item>
///             <term>State Isolation:</term>
///             <description>
///                 Child has separate SpecContainer instances. Child singletons are scoped to child
///                 lifetime, not shared with parent or sibling children. Example: Each request gets
///                 its own DB transaction (singleton per request).
///             </description>
///         </item>
///         <item>
///             <term>Lifetime Management:</term>
///             <description>
///                 Child lifetime is controlled by caller. Child can be disposed independently without
///                 affecting parent. Parent must outlive active children to satisfy dependency references.
///                 Example: Request injector disposed at end of HTTP request, application injector lives
///                 for full app lifetime.
///             </description>
///         </item>
///         <item>
///             <term>Threading:</term>
///             <description>
///                 Each child typically belongs to a single thread/context (e.g., one per HTTP request
///                 thread). Parent may be shared across threads, requiring thread-safe singleton caching.
///             </description>
///         </item>
///         <item>
///             <term>Memory:</term>
///             <description>
///                 Child holds strong reference to parent (via Dependency parameter). Parent does NOT
///                 hold references to children. Children can be garbage collected when caller releases
///                 them.
///             </description>
///         </item>
///     </list>
///     
///     <para>Multi-Level Hierarchies:</para>
///     <para>
///         Child injectors can themselves have child factories, creating arbitrarily deep hierarchies:
///     </para>
///     <code>
///         // Application → Request → Operation hierarchy
///         var app = new ApplicationInjector_ApplicationSpec();
///         var request = app.CreateRequest(httpContext);
///         var operation = request.CreateOperation(command);
///         
///         // Operation can access dependencies from request and application
///         operation.Execute();  // Uses operation, request, and application dependencies
///     </code>
///     <para>
///         Each level has its own [Dependency] interface referencing its immediate parent. Grandchild
///         accesses grandparent dependencies by requesting them from parent, which delegates to grandparent.
///     </para>
///     
///     <para>When to Use Child Injectors:</para>
///     <list type="bullet">
///         <item>Different lifetime scopes (application vs request vs operation)</item>
///         <item>Context-specific dependencies (per-request DB transaction, per-operation state)</item>
///         <item>Isolation of stateful components (request-scoped cache doesn't leak to other requests)</item>
///         <item>Memory efficiency (short-lived resources disposed with child, not held for app lifetime)</item>
///         <item>Multi-tenancy (per-tenant child injector with tenant-specific config)</item>
///     </list>
///     
///     <para>Relationship to Other Models:</para>
///     <list type="bullet">
///         <item>
///             Contrast with <see cref="InjectorProviderModel"/>: Providers return dependency instances,
///             ChildFactories return child injector containers
///         </item>
///         <item>
///             Contrast with <see cref="InjectorBuilderModel"/>: Builders initialize objects,
///             ChildFactories create sub-containers
///         </item>
///         <item>
///             Links to: Child's <see cref="InjectorModel"/> via ChildInjectorType resolution
///         </item>
///         <item>
///             Used by: Parent's <see cref="InjectorModel.ChildFactories"/> collection during code generation
///         </item>
///     </list>
/// </remarks>
/// <param name="ChildInjectorType"> 
///     The type metadata of the child injector interface (e.g., IRequestInjector). Used to resolve
///     the child's generated implementation class name (e.g., RequestInjector_RequestSpec).
/// </param>
/// <param name="ChildFactoryMethodName"> 
///     The method name from the parent interface (e.g., "CreateRequest"). Used as-is in the
///     generated parent implementation to satisfy interface contract.
/// </param>
/// <param name="Parameters"> 
///     Context parameters passed to the factory method (e.g., [HttpContext, Command]). These become
///     constructor arguments for the child injector and are available as external dependencies in
///     the child's specifications.
/// </param>
/// <param name="Location"> The source location where this factory is defined for diagnostics. </param>
internal record InjectorChildFactoryModel(
    TypeMetadata ChildInjectorType,
    string ChildFactoryMethodName,
    IEnumerable<TypeMetadata> Parameters,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
