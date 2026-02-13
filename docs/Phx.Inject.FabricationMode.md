# <a id="Phx_Inject_FabricationMode"></a> Enum FabricationMode

Namespace: [Phx.Inject](Phx.Inject.md)  
Assembly: Phx.Inject.Generator.dll  

Enumerates the modes of fabrication that can be used by a factory method.

```csharp
public enum FabricationMode
```

#### Extension Methods

[FunctionalExtensions.Also<FabricationMode\>\(FabricationMode, Action<FabricationMode\>\)](Phx.Inject.Common.Util.FunctionalExtensions.md\#Phx\_Inject\_Common\_Util\_FunctionalExtensions\_Also\_\_1\_\_\_0\_System\_Action\_\_\_0\_\_), 
[GeneratorIgnoredExtensions.GeneratorIgnored<FabricationMode\>\(FabricationMode\)](Phx.Inject.Generator.Incremental.Util.GeneratorIgnoredExtensions.md\#Phx\_Inject\_Generator\_Incremental\_Util\_GeneratorIgnoredExtensions\_GeneratorIgnored\_\_1\_\_\_0\_), 
[FunctionalExtensions.Let<FabricationMode, TResult\>\(FabricationMode, Func<FabricationMode, TResult\>\)](Phx.Inject.Common.Util.FunctionalExtensions.md\#Phx\_Inject\_Common\_Util\_FunctionalExtensions\_Let\_\_2\_\_\_0\_System\_Func\_\_\_0\_\_\_1\_\_), 
[FunctionalExtensions.Then<FabricationMode\>\(FabricationMode, Action<FabricationMode\>\)](Phx.Inject.Common.Util.FunctionalExtensions.md\#Phx\_Inject\_Common\_Util\_FunctionalExtensions\_Then\_\_1\_\_\_0\_System\_Action\_\_\_0\_\_)

## Fields

`Container = 2` 

Indicates that the factory method should construct a new instance each time it is invoked and
create a new container for child container scoped dependencies.



`ContainerScoped = 3` 

Indicates that the factory method should only construct a single instance within a given
container. Returning that first instance on all invocations after the first.



`Recurrent = 0` 

Indicates that the factory method should construct a new instance each time it is invoked.



`Scoped = 1` 

Indicates that the factory method should only construct a single instance within a given scope.
Returning that first instance on all invocations after the first.



