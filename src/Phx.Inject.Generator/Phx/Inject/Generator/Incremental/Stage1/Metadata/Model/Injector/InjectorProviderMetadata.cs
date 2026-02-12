// -----------------------------------------------------------------------------
// <copyright file="InjectorProviderMetadata.cs" company="Star Cruise Studios LLC">
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
///     Stage 1 metadata for a provider method that returns constructed object instances.
/// </summary>
/// <remarks>
///     <para>Domain Model:</para>
///     <para>
///         A provider is a factory method on the injector interface that returns a fully constructed
///         object. This is the primary pattern for retrieving dependencies from the DI container.
///         The method signature determines both the type to construct and any qualifiers (labels).
///     </para>
///     
///     <para>Provider vs Activator vs ChildProvider:</para>
///     <list type="bullet">
///         <item>
///             <term>Provider (this class):</term>
///             <description>
///                 Returns T - Constructs and returns an instance. Use when you need a reference to
///                 a new or cached object. Example: IDatabase GetDatabase()
///             </description>
///         </item>
///         <item>
///             <term>Activator:</term>
///             <description>
///                 Returns void - Initializes an existing object's dependencies. Use when the object
///                 is already constructed but needs injection. Example: void Init(MyService service)
///             </description>
///         </item>
///         <item>
///             <term>ChildProvider:</term>
///             <description>
///                 Returns child injector interface - Creates a scoped sub-container. Use for
///                 hierarchical lifetimes. Example: IRequestScope CreateScope(RequestContext ctx)
///             </description>
///         </item>
///     </list>
///     
///     <para>Example User Code:</para>
///     <code>
///         [Injector(typeof(DatabaseSpec))]
///         interface IDatabaseInjector {
///             // Simple provider
///             IDatabase GetDatabase();
///             
///             // Qualified provider (with label)
///             [Label(DatabaseSpec.ReadReplica)]
///             IDatabase GetReadDatabase();
///         }
///     </code>
///     
///     <para>Code Generation:</para>
///     <para>
///         Transformed by InjectorProviderMapper into <see cref="Stage2.Core.Model.Injector.InjectorProviderModel"/>,
///         which generates implementation methods that:
///     </para>
///     <list type="number">
///         <item>Resolve the ProvidedType from the specification's factory methods</item>
///         <item>Apply qualification (labels) to select the correct factory</item>
///         <item>Cache the result if the specification declares singleton scope</item>
///         <item>Return the constructed instance to the caller</item>
///     </list>
///     
///     <para>Generated Implementation Pattern:</para>
///     <code>
///         // For: IDatabase GetDatabase()
///         public IDatabase GetDatabase() {
///             return this.specContainer.Fac_L_None_IDatabase();
///         }
///         
///         // For: [Label(DatabaseSpec.ReadReplica)] IDatabase GetReadDatabase()
///         public IDatabase GetReadDatabase() {
///             return this.specContainer.Fac_L_ReadReplica_IDatabase();
///         }
///     </code>
///     
///     <para>Scope Implications:</para>
///     <para>
///         Providers delegate to specification factories, which determine the actual scope/lifetime
///         (singleton, transient, scoped). The injector itself manages singleton caching via
///         SpecContainerCollection fields. Child injectors can request parent-provided types by
///         declaring them in their [Dependency] interface.
///     </para>
/// </remarks>
/// <param name="ProviderMethodName"> 
///     The method name from the user's interface (e.g., "GetDatabase"). Used as-is in generated implementation. 
/// </param>
/// <param name="ProvidedType"> 
///     The qualified type returned by this provider, including any [Label] qualifiers from the method.
///     Used to match against specification factories.
/// </param>
/// <param name="Location"> The source location of the provider definition for diagnostics. </param>
internal record InjectorProviderMetadata(
    string ProviderMethodName,
    QualifiedTypeMetadata ProvidedType,
    GeneratorIgnored<LocationInfo?> Location
): ISourceCodeElement { }
