// -----------------------------------------------------------------------------
// <copyright file="InjectorBuilderModel.cs" company="Star Cruise Studios LLC">
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
///     Stage 2 implementation model for generating builder (activator) methods in injector classes.
/// </summary>
/// <remarks>
///     <para><strong>Domain Model:</strong></para>
///     <para>
///         InjectorBuilderModel represents a single builder method that will be generated in the
///         concrete injector class. It is the Stage 2 counterpart to 
///         <see cref="Stage1.Metadata.Model.Injector.InjectorActivatorMetadata"/>, which uses the
///         term "Activator" while Stage 2 uses "Builder" for the same concept: methods that initialize
///         existing objects rather than constructing new ones.
///     </para>
///     
///     <para><strong>Terminology Note:</strong></para>
///     <list type="bullet">
///         <item>
///             <term>Stage 1 (Metadata):</term>
///             <description>Uses "Activator" to describe void methods that inject dependencies into
///             existing objects. Focuses on user's perspective: "activating" an object's dependencies.</description>
///         </item>
///         <item>
///             <term>Stage 2 (Model):</term>
///             <description>Uses "Builder" to align with specification terminology ([Builder] attribute).
///             Focuses on implementation: "building up" an object's state via dependency injection.</description>
///         </item>
///         <item>
///             <term>Same Concept:</term>
///             <description>Both terms refer to the same pattern: void methods that accept an existing
///             object and initialize its dependencies. Activator = Builder in this architecture.</description>
///         </item>
///     </list>
///     
///     <para><strong>Transformation from Metadata:</strong></para>
///     <para>
///         InjectorActivatorMapper transforms Stage 1 metadata into this model:
///     </para>
///     <list type="number">
///         <item>Validates that ActivatedType matches a builder in the injector's specifications</item>
///         <item>Resolves qualified type (including [Label] qualifiers) to specific builder method</item>
///         <item>Determines which specification container owns the builder</item>
///         <item>Renames method: ActivatorMethodName → BuilderMethodName</item>
///     </list>
///     
///     <para><strong>Code Generation Pattern:</strong></para>
///     <para>
///         Each InjectorBuilderModel generates a method in the injector class that:
///     </para>
///     <list type="number">
///         <item>Accepts an existing object instance as a parameter (the target to initialize)</item>
///         <item>Resolves required dependencies from specification factories</item>
///         <item>Delegates to the specification container's builder method with target + dependencies</item>
///         <item>Returns void (initialization is in-place, no return value)</item>
///     </list>
///     
///     <para><strong>Example Transformation:</strong></para>
///     <code>
///         // User writes (Stage 1 Metadata):
///         [Injector(typeof(ViewModelSpec))]
///         interface IViewModelInjector {
///             void InitializeViewModel(MainViewModel viewModel);
///         }
///         
///         // Analyzed as InjectorActivatorMetadata:
///         - ActivatorMethodName: "InitializeViewModel"
///         - ActivatedType: QualifiedType(MainViewModel, Label=None)
///         
///         // Mapped to InjectorBuilderModel (this class):
///         - BuilderMethodName: "InitializeViewModel"
///         - BuiltType: QualifiedType(MainViewModel, Label=None)
///         
///         // Generates (Stage 2 Output):
///         public void InitializeViewModel(MainViewModel viewModel) {
///             this.viewModelSpecContainer.Bld_L_None_MainViewModel(
///                 viewModel,
///                 this.viewModelSpecContainer.Fac_L_None_IDatabase(),
///                 this.viewModelSpecContainer.Fac_L_None_ILogger()
///             );
///         }
///     </code>
///     
///     <para><strong>Specification Builder Pattern:</strong></para>
///     <para>
///         The generated method delegates to a specification container's builder method. The builder
///         is defined in the specification class with [Builder] attribute:
///     </para>
///     <code>
///         [Specification]
///         class ViewModelSpec {
///             [Builder]
///             public static void BuildMainViewModel(
///                 MainViewModel target,        // The object to initialize
///                 IDatabase database,           // Dependencies to inject
///                 ILogger logger) {
///                 target.Database = database;
///                 target.Logger = logger;
///                 target.IsInitialized = true;
///             }
///         }
///     </code>
///     <para>
///         The container's Bld_L_None_MainViewModel method wraps this builder, resolving dependencies
///         from other factories before passing them to the builder.
///     </para>
///     
///     <para><strong>When to Use Builders vs Providers:</strong></para>
///     <list type="bullet">
///         <item>
///             <term>Use Provider (returns T):</term>
///             <description>
///                 When the injector should construct the object. The injector owns the object's
///                 creation and lifetime. Example: IDatabase GetDatabase() → injector creates database.
///             </description>
///         </item>
///         <item>
///             <term>Use Builder (returns void):</term>
///             <description>
///                 When the object already exists but needs dependencies injected. The caller owns
///                 the object's creation and lifetime. Example: void Init(MainViewModel vm) → caller
///                 created vm, injector populates it.
///             </description>
///         </item>
///     </list>
///     
///     <para><strong>Common Use Cases:</strong></para>
///     <list type="bullet">
///         <item>Initializing framework-created objects (e.g., ASP.NET controllers, WPF view models)</item>
///         <item>Injecting into deserialized objects (e.g., DTOs with service dependencies)</item>
///         <item>Post-construction initialization of objects created via reflection</item>
///         <item>Lazy initialization of existing singletons</item>
///         <item>Injecting into pooled objects before reuse</item>
///     </list>
///     
///     <para><strong>Qualified Type Resolution:</strong></para>
///     <para>
///         BuiltType includes qualification metadata (labels) that directs the generator to the
///         correct builder method. Follows same pattern as providers:
///     </para>
///     <list type="bullet">
///         <item>Unqualified: MainViewModel → Bld_L_None_MainViewModel(target, ...)</item>
///         <item>Single label: [Label(Admin)] AdminViewModel → Bld_L_Admin_AdminViewModel(target, ...)</item>
///     </list>
///     
///     <para><strong>Scope Implications:</strong></para>
///     <para>
///         Builders resolve dependencies at invocation time using the current injector's scope.
///         The initialized object itself is NOT managed by the container:
///     </para>
///     <list type="bullet">
///         <item>
///             <term>Object Lifetime:</term>
///             <description>
///                 Controlled by the caller. The injector does not track or dispose the object.
///             </description>
///         </item>
///         <item>
///             <term>Dependency Lifetime:</term>
///             <description>
///                 Dependencies are resolved from the injector's scope. If a dependency is singleton,
///                 all initialized objects share the same instance.
///             </description>
///         </item>
///         <item>
///             <term>Timing:</term>
///             <description>
///                 Builder executes when called, not when object is created. Enables deferred
///                 initialization or re-initialization scenarios.
///             </description>
///         </item>
///     </list>
///     
///     <para><strong>Relationship to Other Models:</strong></para>
///     <list type="bullet">
///         <item>
///             Contrast with <see cref="InjectorProviderModel"/>: Builders accept target parameter
///             and return void, Providers accept no parameters and return constructed instance
///         </item>
///         <item>
///             Contrast with <see cref="InjectorChildFactoryModel"/>: Builders initialize objects,
///             ChildFactories create child injector containers
///         </item>
///         <item>
///             Used by: <see cref="InjectorModel.Builders"/> collection during code generation
///         </item>
///     </list>
/// </remarks>
/// <param name="BuiltType"> 
///     The qualified type to be initialized (the target parameter type), including any [Label] qualifiers. 
///     Used to resolve the correct specification builder method during code generation.
/// </param>
/// <param name="BuilderMethodName"> 
///     The method name from the user's interface (e.g., "InitializeViewModel"). Used as-is in the
///     generated implementation to satisfy interface contract.
/// </param>
/// <param name="Location"> The source location where this builder is defined for diagnostics. </param>
internal record InjectorBuilderModel(
    QualifiedTypeMetadata BuiltType,
    string BuilderMethodName,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
