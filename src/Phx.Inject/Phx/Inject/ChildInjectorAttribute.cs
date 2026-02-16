// -----------------------------------------------------------------------------
// <copyright file="ChildInjectorAttribute.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary> Annotates a method used to retrieve a child injector instance. </summary>
/// <remarks>
///   <!-- ApiDoc -->
///   <para>
///     A [child injector] is an [injector] that is not constructed directly by the
///     calling code, but that is constructed by another "parent" [injector]. The
///     [child injector] will have access to the dependencies provided by the parent
///     [injector] through a [dependency specification] interface, but the parent
///     will not have implicit access to dependencies provided by the
///     [child injector].
///   </para>
///   <para>
///     [Dependency specification]s can only define [factory] methods, not
///     [factory reference]s, [builder] methods, or [builder reference]s, and the
///     [factory] methods cannot accept any arguments.
///   </para>
///   <para>
///     <blockquote>
///     <b>Note:</b> The [dependency] interface implementation does not accept
///     values from the [child injector]. This means that dependencies created by
///     the [dependency specification] cannot be injected with values from the
///     [child injector].
///     </blockquote>
///   </para>
/// </remarks>
/// <example>
///   <!-- ApiDoc -->
///   <code>
///   [Specification]
///   internal static ChildSpecification {
///       internal static MyClass GetMyClass(int intValue) {
///         return new MyClass(intValue);
///       }
///   }
///   
///   [Specification]
///   internal static ParentSpecification {
///       internal static int GetIntValue() {
///           return 10;
///       }
///   }
///  
///   // The dependency interface defines the types that must be provided
///   // by the parent injector.
///   [InjectorDependency]
///   internal interface IChildDependencies {
///       // Qualifier attributes could also be used to differentiate dependencies
///       // with the same type.
///       [Factory]
///       public int GetIntValue();
///   }
///  
///   // The child injector declares a dependency on the IChildDependencies interface. 
///   [Injector(typeof(ChildSpecification))]
///   [Dependency(typeof(IChildDependencies))]
///   internal interface IChildInjector {
///       public MyClass GetMyClass();
///   }
///  
///   [Injector(typeof(ParentSpecification))]
///   internal interface IParentInjector {
///       // The parent injector will construct an instance of the IChildDependencies
///       // interface and pass it to the child injector when constructing it.
///       [ChildInjector]
///       public IChildInjector GetChildInjector();
///   }
///  
///   // In the application code
///   var parentInjector = new GeneratedParentInjector();
///   var childInjector = parentInjector.GetChildInjector();
///   var myClass = childInjector.GetMyClass();
///   
///   Verify.That(myClass.Value.IsEqualTo(10));
///   </code>
/// </example>
/// <example>
///   <!-- ApiDoc -->
///   <para>
///   [Child injector]s can be useful for defining different scopes and lifetimes
///   within a single dependency graph.
///   </para>
///   <code>
///   [Injector(...)]
///   internal interface IApplicationInjector {
///       // All dependencies share the same application config.
///       public AppConfig GetApplicationConfig();
///       
///       [ChildInjector]
///       public ISessionInjector GetSessionInjector();
///   }
///   
///   [Injector(...)]
///   [Dependency(...)]
///   internal interface ISessionInjector {
///       // Session credentials are shared by all dependencies created within a
///       // session.
///       public Credentials GetSessionCredentials();
///       
///       [ChildInjector]
///       public IRequestInjector GetRequestInjector();
///   }
///   
///   [Injector(...)]
///   [Dependency(...)]
///   internal interface IRequestInjector {
///       // A unique request ID is shared by all dependencies while a request is
///       // processing.
///       public string GetRequestId();
///   }
///   
///   // On Application startup:
///   var appInjector = new GeneratedAppInjector();
///   
///   // Each time a new session is started within the same application:
///   var sessionInjector = appInjector.GetSessionInjector();
///   
///   // Each time a new request is created within a session:
///   var requestInjector = sessionInjector.GetRequestInjector();
///   
///   requestInjector.GetRequestId();
///   </code>
/// </example>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class ChildInjectorAttribute : Attribute { }
