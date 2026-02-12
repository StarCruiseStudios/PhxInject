// -----------------------------------------------------------------------------
// <copyright file="InjectorActivatorMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;

/// <summary>
///     Stage 1 metadata for an activator method that initializes existing object instances.
/// </summary>
/// <remarks>
///     <para><strong>Domain Model:</strong></para>
///     <para>
///         An activator is a void method that accepts an existing object and injects its dependencies.
///         This supports scenarios where object construction is external to the DI container (e.g.,
///         deserialized objects, framework-created instances, or objects created via reflection), but
///         dependency injection is still desired. Also called the "builder pattern" in this codebase.
///     </para>
///     
///     <para><strong>When to Use Activators vs Providers:</strong></para>
///     <list type="bullet">
///         <item>
///             <term>Use Provider:</term>
///             <description>
///                 When the injector should construct AND return the object. Standard DI pattern.
///                 Example: IDatabase GetDatabase() → injector creates database instance
///             </description>
///         </item>
///         <item>
///             <term>Use Activator:</term>
///             <description>
///                 When the object already exists but needs dependencies injected. Post-construction
///                 initialization pattern. Example: void InitializeViewModel(ViewModel vm) →
///                 injector populates vm's dependencies but caller already created vm
///             </description>
///         </item>
///     </list>
///     
///     <para><strong>Example User Code:</strong></para>
///     <code>
///         [Injector(typeof(ViewModelSpec))]
///         interface IViewModelInjector {
///             // Activator: Initializes an existing ViewModel instance
///             void Initialize(MainViewModel viewModel);
///             
///             // Compare to Provider: Creates and returns a new ViewModel
///             MainViewModel CreateViewModel();
///         }
///     </code>
///     
///     <para><strong>Specification Counterpart:</strong></para>
///     <para>
///         Activators correspond to [Builder] methods in specifications, not [Factory] methods.
///         Builders have void return type and accept the object to initialize as a parameter:
///     </para>
///     <code>
///         [Specification]
///         class ViewModelSpec {
///             [Builder]
///             public static void BuildMainViewModel(
///                 MainViewModel target,
///                 IDatabase database,
///                 ILogger logger) {
///                 target.Database = database;
///                 target.Logger = logger;
///             }
///         }
///     </code>
///     
///     <para><strong>Code Generation:</strong></para>
///     <para>
///         Transformed by InjectorActivatorMapper into <see cref="Stage2.Core.Model.Injector.InjectorBuilderModel"/>,
///         which generates implementation methods that:
///     </para>
///     <list type="number">
///         <item>Accept the target object as a parameter</item>
///         <item>Resolve required dependencies from specification factories</item>
///         <item>Call the specification's builder method with target + dependencies</item>
///         <item>Return void (initialization is in-place)</item>
///     </list>
///     
///     <para><strong>Generated Implementation Pattern:</strong></para>
///     <code>
///         // For: void Initialize(MainViewModel viewModel)
///         public void Initialize(MainViewModel viewModel) {
///             this.specContainer.Bld_L_None_MainViewModel(
///                 viewModel,
///                 this.specContainer.Fac_L_None_IDatabase(),
///                 this.specContainer.Fac_L_None_ILogger()
///             );
///         }
///     </code>
///     
///     <para><strong>Scope Implications:</strong></para>
///     <para>
///         Activators resolve dependencies at invocation time, using the current injector's scope.
///         The activated object itself is not managed by the container - its lifetime is controlled
///         by the caller. This enables "partial DI" where some objects are externally managed but
///         still benefit from dependency injection.
///     </para>
///     
///     <para><strong>Common Use Cases:</strong></para>
///     <list type="bullet">
///         <item>Initializing view models created by UI frameworks</item>
///         <item>Injecting into deserialized DTOs</item>
///         <item>Post-construction initialization of pooled objects</item>
///         <item>Lazy initialization of existing singletons</item>
///     </list>
/// </remarks>
/// <param name="ActivatorMethodName"> 
///     The method name from the user's interface (e.g., "Initialize"). Used as-is in generated implementation. 
/// </param>
/// <param name="ActivatedType"> 
///     The qualified type to be initialized (the method parameter type), including any [Label] qualifiers.
///     Used to match against specification builders.
/// </param>
/// <param name="Location"> The source location of the activator definition for diagnostics. </param>
internal record InjectorActivatorMetadata(
    string ActivatorMethodName,
    QualifiedTypeMetadata ActivatedType,
    GeneratorIgnored<LocationInfo?> Location
): ISourceCodeElement { }