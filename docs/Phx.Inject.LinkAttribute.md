# <a id="Phx_Inject_LinkAttribute"></a> Class LinkAttribute

Namespace: [Phx.Inject](Phx.Inject.md)  
Assembly: Phx.Inject.Generator.dll  

Models a link between one dependency key and another.

```csharp
[AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface, Inherited = false, AllowMultiple = true)]
public class LinkAttribute : Attribute
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[Attribute](https://learn.microsoft.com/dotnet/api/system.attribute) ← 
[LinkAttribute](Phx.Inject.LinkAttribute.md)

#### Inherited Members

[Attribute.Equals\(object\)](https://learn.microsoft.com/dotnet/api/system.attribute.equals), 
[Attribute.GetCustomAttribute\(Assembly, Type\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattribute\#system\-attribute\-getcustomattribute\(system\-reflection\-assembly\-system\-type\)), 
[Attribute.GetCustomAttribute\(Assembly, Type, bool\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattribute\#system\-attribute\-getcustomattribute\(system\-reflection\-assembly\-system\-type\-system\-boolean\)), 
[Attribute.GetCustomAttribute\(MemberInfo, Type\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattribute\#system\-attribute\-getcustomattribute\(system\-reflection\-memberinfo\-system\-type\)), 
[Attribute.GetCustomAttribute\(MemberInfo, Type, bool\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattribute\#system\-attribute\-getcustomattribute\(system\-reflection\-memberinfo\-system\-type\-system\-boolean\)), 
[Attribute.GetCustomAttribute\(Module, Type\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattribute\#system\-attribute\-getcustomattribute\(system\-reflection\-module\-system\-type\)), 
[Attribute.GetCustomAttribute\(Module, Type, bool\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattribute\#system\-attribute\-getcustomattribute\(system\-reflection\-module\-system\-type\-system\-boolean\)), 
[Attribute.GetCustomAttribute\(ParameterInfo, Type\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattribute\#system\-attribute\-getcustomattribute\(system\-reflection\-parameterinfo\-system\-type\)), 
[Attribute.GetCustomAttribute\(ParameterInfo, Type, bool\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattribute\#system\-attribute\-getcustomattribute\(system\-reflection\-parameterinfo\-system\-type\-system\-boolean\)), 
[Attribute.GetCustomAttributes\(Assembly\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes\#system\-attribute\-getcustomattributes\(system\-reflection\-assembly\)), 
[Attribute.GetCustomAttributes\(Assembly, bool\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes\#system\-attribute\-getcustomattributes\(system\-reflection\-assembly\-system\-boolean\)), 
[Attribute.GetCustomAttributes\(Assembly, Type\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes\#system\-attribute\-getcustomattributes\(system\-reflection\-assembly\-system\-type\)), 
[Attribute.GetCustomAttributes\(Assembly, Type, bool\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes\#system\-attribute\-getcustomattributes\(system\-reflection\-assembly\-system\-type\-system\-boolean\)), 
[Attribute.GetCustomAttributes\(MemberInfo\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes\#system\-attribute\-getcustomattributes\(system\-reflection\-memberinfo\)), 
[Attribute.GetCustomAttributes\(MemberInfo, bool\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes\#system\-attribute\-getcustomattributes\(system\-reflection\-memberinfo\-system\-boolean\)), 
[Attribute.GetCustomAttributes\(MemberInfo, Type\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes\#system\-attribute\-getcustomattributes\(system\-reflection\-memberinfo\-system\-type\)), 
[Attribute.GetCustomAttributes\(MemberInfo, Type, bool\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes\#system\-attribute\-getcustomattributes\(system\-reflection\-memberinfo\-system\-type\-system\-boolean\)), 
[Attribute.GetCustomAttributes\(Module\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes\#system\-attribute\-getcustomattributes\(system\-reflection\-module\)), 
[Attribute.GetCustomAttributes\(Module, bool\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes\#system\-attribute\-getcustomattributes\(system\-reflection\-module\-system\-boolean\)), 
[Attribute.GetCustomAttributes\(Module, Type\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes\#system\-attribute\-getcustomattributes\(system\-reflection\-module\-system\-type\)), 
[Attribute.GetCustomAttributes\(Module, Type, bool\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes\#system\-attribute\-getcustomattributes\(system\-reflection\-module\-system\-type\-system\-boolean\)), 
[Attribute.GetCustomAttributes\(ParameterInfo\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes\#system\-attribute\-getcustomattributes\(system\-reflection\-parameterinfo\)), 
[Attribute.GetCustomAttributes\(ParameterInfo, bool\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes\#system\-attribute\-getcustomattributes\(system\-reflection\-parameterinfo\-system\-boolean\)), 
[Attribute.GetCustomAttributes\(ParameterInfo, Type\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes\#system\-attribute\-getcustomattributes\(system\-reflection\-parameterinfo\-system\-type\)), 
[Attribute.GetCustomAttributes\(ParameterInfo, Type, bool\)](https://learn.microsoft.com/dotnet/api/system.attribute.getcustomattributes\#system\-attribute\-getcustomattributes\(system\-reflection\-parameterinfo\-system\-type\-system\-boolean\)), 
[Attribute.GetHashCode\(\)](https://learn.microsoft.com/dotnet/api/system.attribute.gethashcode), 
[Attribute.IsDefaultAttribute\(\)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefaultattribute), 
[Attribute.IsDefined\(Assembly, Type\)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefined\#system\-attribute\-isdefined\(system\-reflection\-assembly\-system\-type\)), 
[Attribute.IsDefined\(Assembly, Type, bool\)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefined\#system\-attribute\-isdefined\(system\-reflection\-assembly\-system\-type\-system\-boolean\)), 
[Attribute.IsDefined\(MemberInfo, Type\)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefined\#system\-attribute\-isdefined\(system\-reflection\-memberinfo\-system\-type\)), 
[Attribute.IsDefined\(MemberInfo, Type, bool\)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefined\#system\-attribute\-isdefined\(system\-reflection\-memberinfo\-system\-type\-system\-boolean\)), 
[Attribute.IsDefined\(Module, Type\)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefined\#system\-attribute\-isdefined\(system\-reflection\-module\-system\-type\)), 
[Attribute.IsDefined\(Module, Type, bool\)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefined\#system\-attribute\-isdefined\(system\-reflection\-module\-system\-type\-system\-boolean\)), 
[Attribute.IsDefined\(ParameterInfo, Type\)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefined\#system\-attribute\-isdefined\(system\-reflection\-parameterinfo\-system\-type\)), 
[Attribute.IsDefined\(ParameterInfo, Type, bool\)](https://learn.microsoft.com/dotnet/api/system.attribute.isdefined\#system\-attribute\-isdefined\(system\-reflection\-parameterinfo\-system\-type\-system\-boolean\)), 
[Attribute.Match\(object\)](https://learn.microsoft.com/dotnet/api/system.attribute.match), 
[Attribute.TypeId](https://learn.microsoft.com/dotnet/api/system.attribute.typeid), 
[object.Equals\(object\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\)), 
[object.Equals\(object, object\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\-system\-object\)), 
[object.GetHashCode\(\)](https://learn.microsoft.com/dotnet/api/system.object.gethashcode), 
[object.GetType\(\)](https://learn.microsoft.com/dotnet/api/system.object.gettype), 
[object.MemberwiseClone\(\)](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone), 
[object.ReferenceEquals\(object, object\)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals), 
[object.ToString\(\)](https://learn.microsoft.com/dotnet/api/system.object.tostring)

#### Extension Methods

[FunctionalExtensions.Also<LinkAttribute\>\(LinkAttribute, Action<LinkAttribute\>\)](Phx.Inject.Common.Util.FunctionalExtensions.md\#Phx\_Inject\_Common\_Util\_FunctionalExtensions\_Also\_\_1\_\_\_0\_System\_Action\_\_\_0\_\_), 
[GeneratorIgnoredExtensions.GeneratorIgnored<LinkAttribute\>\(LinkAttribute\)](Phx.Inject.Generator.Incremental.Util.GeneratorIgnoredExtensions.md\#Phx\_Inject\_Generator\_Incremental\_Util\_GeneratorIgnoredExtensions\_GeneratorIgnored\_\_1\_\_\_0\_), 
[FunctionalExtensions.Let<LinkAttribute, TResult\>\(LinkAttribute, Func<LinkAttribute, TResult\>\)](Phx.Inject.Common.Util.FunctionalExtensions.md\#Phx\_Inject\_Common\_Util\_FunctionalExtensions\_Let\_\_2\_\_\_0\_System\_Func\_\_\_0\_\_\_1\_\_), 
[FunctionalExtensions.Then<LinkAttribute\>\(LinkAttribute, Action<LinkAttribute\>\)](Phx.Inject.Common.Util.FunctionalExtensions.md\#Phx\_Inject\_Common\_Util\_FunctionalExtensions\_Then\_\_1\_\_\_0\_System\_Action\_\_\_0\_\_)

## Constructors

### <a id="Phx_Inject_LinkAttribute__ctor_System_Type_System_Type_"></a> LinkAttribute\(Type, Type\)

Initializes a new instance of the <xref href="Phx.Inject.LinkAttribute" data-throw-if-not-resolved="false"></xref> class.

```csharp
public LinkAttribute(Type input, Type output)
```

#### Parameters

`input` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The dependency key for the type consumed by the link.

`output` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The dependency key for the type exposed by the link.

## Properties

### <a id="Phx_Inject_LinkAttribute_Input"></a> Input

The dependency key for the type consumed by the link.

```csharp
public Type Input { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

### <a id="Phx_Inject_LinkAttribute_InputLabel"></a> InputLabel

An optional <xref href="Phx.Inject.LabelAttribute" data-throw-if-not-resolved="false"></xref> qualifier for the input type. Cannot be specified at the same time as
<xref href="Phx.Inject.LinkAttribute.InputQualifier" data-throw-if-not-resolved="false"></xref>.

```csharp
public string? InputLabel { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)?

### <a id="Phx_Inject_LinkAttribute_InputQualifier"></a> InputQualifier

An optional <xref href="Phx.Inject.QualifierAttribute" data-throw-if-not-resolved="false"></xref> qualifier for the input type. Cannot be specified at the same time as
<xref href="Phx.Inject.LinkAttribute.InputLabel" data-throw-if-not-resolved="false"></xref>.

```csharp
public Type? InputQualifier { get; set; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)?

### <a id="Phx_Inject_LinkAttribute_Output"></a> Output

The dependency key for the type exposed by the link.

```csharp
public Type Output { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

### <a id="Phx_Inject_LinkAttribute_OutputLabel"></a> OutputLabel

An optional <xref href="Phx.Inject.LabelAttribute" data-throw-if-not-resolved="false"></xref> qualifier for the output type. Cannot be specified at the same time as
<xref href="Phx.Inject.LinkAttribute.OutputQualifier" data-throw-if-not-resolved="false"></xref>.

```csharp
public string? OutputLabel { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)?

### <a id="Phx_Inject_LinkAttribute_OutputQualifier"></a> OutputQualifier

An optional <xref href="Phx.Inject.QualifierAttribute" data-throw-if-not-resolved="false"></xref> qualifier for the output type. Cannot be specified at the same time as
<xref href="Phx.Inject.LinkAttribute.OutputLabel" data-throw-if-not-resolved="false"></xref>.

```csharp
public Type? OutputQualifier { get; set; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)?

