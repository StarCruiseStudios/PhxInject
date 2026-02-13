# <a id="Phx_Inject"></a> Namespace Phx.Inject

### Classes

 [AutoBuilderAttribute](Phx.Inject.AutoBuilderAttribute.md)

Annotates a builder method that will be invoked to complete the construction of a given
dependency.

 [AutoFactoryAttribute](Phx.Inject.AutoFactoryAttribute.md)

Annotates a type or constructor that will be invoked to construct a given dependency.

 [BuilderAttribute](Phx.Inject.BuilderAttribute.md)

Annotates a builder method that will be invoked to complete the construction of a given
dependency.

 [BuilderReferenceAttribute](Phx.Inject.BuilderReferenceAttribute.md)

Annotates a field or property that references a builder method that will be invoked to complete
the construction of a given dependency.

 [ChildInjectorAttribute](Phx.Inject.ChildInjectorAttribute.md)

Annotates a method used to retrieve a child injector instance.

 [DependencyAttribute](Phx.Inject.DependencyAttribute.md)

Defines an dependency that is required by an injector interface.

 [Factory<T\>](Phx.Inject.Factory\-1.md)

A type that can be used to inject a dependency on the factory method for a class instead of the
class itself.

 [FactoryAttribute](Phx.Inject.FactoryAttribute.md)

Annotates a factory method that will be invoked to construct a given dependency.

 [FactoryReferenceAttribute](Phx.Inject.FactoryReferenceAttribute.md)

Annotates a field or property that references a factory method that will be invoked to
construct a given dependency.

 [InjectionUtil](Phx.Inject.InjectionUtil.md)

 [InjectorAttribute](Phx.Inject.InjectorAttribute.md)

Annotates an injector interface as the entry point to a DAG.

 [InjectorDependencyAttribute](Phx.Inject.InjectorDependencyAttribute.md)

Annotates an interface as an injector dependency used to pass values from a parent to a
child injector.

 [LabelAttribute](Phx.Inject.LabelAttribute.md)

Annotates a factory method or dependency with a unique label used to discriminate them from
other dependencies with the same type.

 [LinkAttribute](Phx.Inject.LinkAttribute.md)

Models a link between one dependency key and another.

 [PartialAttribute](Phx.Inject.PartialAttribute.md)

Annotates a factory method as a partial factory. This can be used on a factory that returns a
List, Set, or Dictionary to indicate that multiple factories with the same type and qualifiers
should be combined into a single dependency.

 [PhxInjectAttribute](Phx.Inject.PhxInjectAttribute.md)

 [QualifierAttribute](Phx.Inject.QualifierAttribute.md)

Annotates an attribute as a qualifier that can be applied to a factory method or dependency as
a unique label used to discriminate them from other dependencies with the same type.

 [SpecificationAttribute](Phx.Inject.SpecificationAttribute.md)

Annotates a specification class that contains factory methods and links used to construct a
DAG.

### Enums

 [FabricationMode](Phx.Inject.FabricationMode.md)

Enumerates the modes of fabrication that can be used by a factory method.

