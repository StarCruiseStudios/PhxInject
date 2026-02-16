// -----------------------------------------------------------------------------
// <copyright file="InjectorAttribute.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary> Annotates an injector interface as the entry point to a dependency graph. </summary>
/// <remarks>
/// <!-- ApiDoc:Injector -->
/// <para>
/// An [Injector] is the interface used to construct and access dependencies in
/// the dependency graph. An injector will always be an interface annotated with
/// the <see cref="InjectorAttribute"/> and will contain [Provider] and 
/// [Activator] methods used by your application as access points into the
/// dependency graph. Each [Injector] also has a list of [Specification] types
/// that provide the framework with the dependencies used to construct the
/// dependency graph.
/// </para>
/// <!-- ... -->
/// <!-- ApiDoc:Provider -->
/// <para>
/// [Provider]s are parameterless methods that are defined on the [Injector]
/// interface. They will be linked to a [Factory] in the [Injector]'s set of
/// [Specification]s based on the return type and [Qualifier] attributes of the
/// [Provider]s.
/// </para>
/// <list type="bullet">
/// <item>
/// - [Provider]s must always be parameterless and have a non void return type.
/// </item>
/// <item>
/// - [Provider]s can have any name.
/// </item>
/// </list>
/// <!-- ... -->
/// <!-- ApiDoc:Activator -->
/// <para>
/// [Activator]s are methods that initialize an object using values from the
/// dependency graph. They will be linked to a [Builder] in the [Injector]'s set
/// of [Specification]s based on the type of the first parameter and the
/// [Qualifier] of the method.
/// </para>
/// </remarks>
/// <example>
/// <!-- ApiDoc:Injector -->
/// <code>
/// // An injector interface with a single specification.
/// [Injector(typeof(MySpecification))]
/// public interface IMyInjector {
///     MyService GetService();
/// }
/// </code>
/// </example>
/// <example>
/// <!-- ApiDoc:Provider -->
/// <code>
/// [Injector(
///     typeof(TestSpecification)
/// )]
/// public interface ITestInjector {
///     // Providers are parameterless methods that return a dependency from the graph.
///     public int GetMyInt();
///     
///     /// Qualifiers can be used to differentiate between dependencies of the
///     /// same type.
///     [Label("MyLabel")]
///     public int GetOtherInt();
///     
///     /// Providers are linked based on the qualifiers AND return type. 
///     /// The same qualifier attributes can be reused with different types.
///     [Label("MyLabel")]
///     public string GetString();
/// }
/// </code>
/// </example>
/// <example>
/// <!-- ApiDoc:Activator -->
/// <code>
/// [Injector(typeof(TestSpecification))]
/// public interface ITestInjector {
///     /// Activators must always return void and contain a single parameter of the
///     /// type that is to be injected.
///     public void Build(MyClass class);
///     
///     /// Qualifier attributes should be placed on the activator method, not on the
///     /// parameter.
///     [Label("MyLabel")]
///     public void BuildOther(MyClass class);
/// }
/// </code>
/// </example>
/// <seealso cref="ChildInjectorAttribute"/>
/// <seealso cref="DependencyAttribute"/>
/// <seealso cref="SpecificationAttribute"/>
/// <seealso cref="LabelAttribute"/>
/// <seealso cref="QualifierAttribute"/>
[AttributeUsage(AttributeTargets.Interface)]
public class InjectorAttribute : Attribute {
    /// <summary> The name to use for the generated injector class. </summary>
    /// <remarks>
    /// <!-- ApiDoc -->
    /// <para>
    /// By default, the generated [Injector] will be named by prefixing the name
    /// of the [Injector] interface with "Generated", after removing the "I"
    /// prefix if there is one.
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// - <c>ITestInjector</c> generates <c>GeneratedTestInjector</c>.
    /// </item>
    /// <item>
    /// - <c>ApplicationInjector</c> generates <c>GeneratedApplicationInjector</c>.
    /// </item>
    /// </list>
    ///
    /// <para>
    /// The generated [Injector] will always use the same namespace as the
    /// [Injector] interface. To explicitly define the generated [Injector] name,
    /// use the optional <see cref="GeneratedClassName"/> property.
    /// </para> 
    /// </remarks>
    /// <example>
    /// <code>
    /// [Injector(
    ///     generatedClassName: "CustomInjector", // Generated class will be named `CustomInjector`
    ///     typeof(TestSpecification))]
    /// public interface ITestInjector {
    ///     // ...
    /// }
    /// </code>
    /// </example>
    public string? GeneratedClassName { get; set; } = null;
    public IEnumerable<Type> Specifications { get; }

    public InjectorAttribute(params Type[] specifications) {
        Specifications = specifications;
    }
    
    public InjectorAttribute(string? generatedClassName, params Type[] specifications) {
        GeneratedClassName = generatedClassName;
        Specifications = specifications;
    }
}
