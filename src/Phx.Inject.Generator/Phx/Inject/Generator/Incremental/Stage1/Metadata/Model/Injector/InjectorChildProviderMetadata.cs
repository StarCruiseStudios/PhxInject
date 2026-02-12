// -----------------------------------------------------------------------------
// <copyright file="InjectorChildProviderMetadata.cs" company="Star Cruise Studios LLC">
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
///     Stage 1 metadata for a child provider method that creates hierarchical sub-container instances.
/// </summary>
/// <remarks>
///     <para><strong>Domain Model:</strong></para>
///     <para>
///         A child provider is a factory method that creates a scoped child injector. This enables
///         hierarchical dependency injection where child containers inherit access to parent-provided
///         dependencies while maintaining their own isolated state and lifetime. The [ChildInjector]
///         attribute marks these methods to distinguish them from regular providers.
///     </para>
///     
///     <para><strong>Hierarchical Injection Pattern:</strong></para>
///     <para>
///         Parent-child relationships model nested scopes with different lifetimes:
///     </para>
///     <list type="bullet">
///         <item>
///             <term>Application Injector (Root):</term>
///             <description>
///                 Singleton scope - Lives for entire application lifetime. Provides shared resources
///                 like configuration, logging infrastructure, connection pools.
///             </description>
///         </item>
///         <item>
///             <term>Request Injector (Child):</term>
///             <description>
///                 Request scope - Lives for single HTTP request. Provides request-specific data like
///                 user context, database transaction, request-scoped caches. Can access application-level
///                 dependencies from parent.
///             </description>
///         </item>
///         <item>
///             <term>Operation Injector (Grandchild):</term>
///             <description>
///                 Operation scope - Lives for single business operation. Provides operation-specific
///                 state like command context, validation errors. Can access both request and application
///                 dependencies from ancestor chain.
///             </description>
///         </item>
///     </list>
///     
///     <para><strong>Example User Code:</strong></para>
///     <code>
///         // Parent (Application-level injector)
///         [Injector(typeof(ApplicationSpec))]
///         public interface IApplicationInjector {
///             ILogger GetLogger();
///             IConfiguration GetConfig();
///             
///             // Child provider: Creates request-scoped container
///             [ChildInjector]
///             IRequestInjector CreateRequestScope(HttpContext httpContext);
///         }
///         
///         // Child (Request-level injector)
///         [Injector(typeof(RequestSpec))]
///         [Dependency(typeof(IApplicationDependencies))]  // Links to parent
///         public interface IRequestInjector {
///             IUserContext GetUserContext();
///             IDbTransaction GetTransaction();
///             
///             // Grandchild provider: Creates operation-scoped container
///             [ChildInjector]
///             IOperationInjector CreateOperationScope(Command command);
///         }
///         
///         // Dependency interface: Exposes parent's providers to child
///         [InjectorDependency]
///         public interface IApplicationDependencies {
///             [Factory]
///             ILogger GetLogger();
///             
///             [Factory]
///             IConfiguration GetConfig();
///         }
///     </code>
///     
///     <para><strong>Parameter Propagation:</strong></para>
///     <para>
///         The Parameters collection captures method arguments (e.g., HttpContext, Command) that are
///         passed to the child injector's constructor. These parameters become available as "external
///         dependencies" within the child's specification, enabling context-specific dependency
///         resolution. For example, RequestSpec can use the HttpContext to extract user claims.
///     </para>
///     
///     <para><strong>Code Generation:</strong></para>
///     <para>
///         Transformed by InjectorChildProviderMapper into <see cref="Stage2.Core.Model.Injector.InjectorChildFactoryModel"/>,
///         which generates factory methods that:
///     </para>
///     <list type="number">
///         <item>Accept context parameters (e.g., HttpContext, Command)</item>
///         <item>Instantiate the child injector implementation class</item>
///         <item>Pass parent (this) as constructor argument to child</item>
///         <item>Pass context parameters to child constructor</item>
///         <item>Return the child injector interface reference</item>
///     </list>
///     
///     <para><strong>Generated Implementation Pattern:</strong></para>
///     <code>
///         // In ApplicationInjector_ApplicationSpec class:
///         public IRequestInjector CreateRequestScope(HttpContext httpContext) {
///             return new RequestInjector_RequestSpec(
///                 this,           // Parent injector (implements IApplicationDependencies)
///                 httpContext     // Context parameter
///             );
///         }
///         
///         // In RequestInjector_RequestSpec class:
///         public RequestInjector_RequestSpec(
///             IApplicationDependencies parent,
///             HttpContext httpContext) {
///             this.parent = parent;
///             this.httpContext = httpContext;
///             // Initialize RequestSpec with parent and context
///         }
///     </code>
///     
///     <para><strong>Dependency Flow:</strong></para>
///     <para>
///         The [Dependency] attribute on the child injector interface specifies which parent interface
///         the child depends on. This must match a [InjectorDependency] interface that exposes parent
///         methods via [Factory] attributes. At runtime:
///     </para>
///     <list type="number">
///         <item>Child requests parent dependency: ILogger logger = parent.GetLogger()</item>
///         <item>Parent interface delegates to parent injector: return parentInjector.GetLogger()</item>
///         <item>Parent injector resolves from its scope: return specContainer.Fac_Logger()</item>
///     </list>
///     
///     <para><strong>Scope Isolation and Lifetime:</strong></para>
///     <list type="bullet">
///         <item>
///             <term>Isolation:</term>
///             <description>
///                 Child injectors have separate SpecContainer instances, so child singletons don't
///                 leak into parent scope. Request-scoped DB transaction lives only in child.
///             </description>
///         </item>
///         <item>
///             <term>Lifetime:</term>
///             <description>
///                 Child lifetime is controlled by caller. When child is disposed/GC'd, its resources
///                 (e.g., DB transaction) are cleaned up, but parent remains unaffected. Parent must
///                 outlive active children to satisfy dependency references.
///             </description>
///         </item>
///         <item>
///             <term>Threading:</term>
///             <description>
///                 Each child is typically thread-local (e.g., one per HTTP request thread). Parent
///                 may be shared across threads (application singleton), so parent factories must be
///                 thread-safe.
///             </description>
///         </item>
///     </list>
///     
///     <para><strong>Relationship to Other Metadata:</strong></para>
///     <list type="bullet">
///         <item>ChildInjectorAttribute: Contains specification types for the child injector</item>
///         <item>ChildInjectorType: References the child's InjectorInterfaceMetadata</item>
///         <item>Parameters: External dependencies available to child's specifications</item>
///     </list>
/// </remarks>
/// <param name="ChildProviderMethodName"> 
///     The method name from the parent interface (e.g., "CreateRequestScope"). Used as-is in generated implementation. 
/// </param>
/// <param name="ChildInjectorType"> 
///     The type metadata of the child injector interface (e.g., IRequestInjector). Used to resolve
///     the child's generated implementation class name.
/// </param>
/// <param name="Parameters"> 
///     Context parameters passed to the child factory method (e.g., [HttpContext, Command]). These
///     become constructor arguments for the child injector, providing external dependencies.
/// </param>
/// <param name="ChildInjectorAttribute"> 
///     The [ChildInjector] attribute metadata, which may include additional specification types
///     specific to the child scope.
/// </param>
/// <param name="Location"> The source location of the child provider definition for diagnostics. </param>
internal record InjectorChildProviderMetadata(
    string ChildProviderMethodName,
    TypeMetadata ChildInjectorType,
    EquatableList<TypeMetadata> Parameters,
    ChildInjectorAttributeMetadata ChildInjectorAttribute,
    GeneratorIgnored<LocationInfo?> Location
): ISourceCodeElement { }
