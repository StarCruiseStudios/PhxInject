# <a id="Phx_Inject_Common_Util_FunctionalExtensions"></a> Class FunctionalExtensions

Namespace: [Phx.Inject.Common.Util](Phx.Inject.Common.Util.md)  
Assembly: Phx.Inject.Generator.dll  

Extension methods for functional programming patterns inspired by Kotlin/Scala.

PURPOSE:
- Enables functional transformation and side-effect patterns in C#
- Reduces temporary variables and improves code flow
- Makes complex transformations more readable through chaining

WHY THIS EXISTS:
C# has limited built-in support for functional patterns that are common in other languages.
These extensions bring three key patterns:

1. Let: Transform a value inline (Kotlin's let, Scala's map)
2. Also: Perform side effects while passing through a value (Kotlin's also, Scala's tap)
3. Then: Terminate a chain with a side effect (Kotlin's also at end of chain)

PATTERN COMPARISON:

Traditional C#:
  var temp = GetValue();
  var transformed = Transform(temp);
  Log(transformed);
  DoSomething(transformed);
  return transformed;

With functional extensions:
  return GetValue()
      .Let(Transform)
      .Also(Log)
      .Also(DoSomething);

DESIGN DECISIONS:

1. Why "Let" instead of "Map" or "Select"?
   - LINQ already has Select() for IEnumerable
   - "Let" is Kotlin terminology, familiar to many developers
   - Avoids confusion with LINQ operators

2. Why both "Also" and "Then"?
   - Also: Returns the value (chainable, for multiple side effects)
   - Then: Returns void (terminal operation, signals end of chain)
   - Different signatures prevent accidental misuse

3. Why are these useful in generators?
   - Generators involve multi-stage transformations (syntax → symbols → metadata → code)
   - Each stage may need logging, validation, or diagnostics
   - These patterns make transformation pipelines explicit and linear

USAGE EXAMPLES:

Transformation chain:
  var metadata = symbol
      .Let(ExtractInfo)
      .Let(ValidateInfo)
      .Also(info =&gt; Log($"Processing {info.Name}"))
      .Let(CreateMetadata);

Builder pattern enhancement:
  return new StringBuilder()
      .Also(sb =&gt; sb.AppendLine("// Generated code"))
      .Also(sb =&gt; sb.AppendLine($"class {className}"))
      .Also(sb =&gt; AppendMembers(sb, members))
      .ToString();

Side effects without breaking flow:
  var result = CalculateValue()
      .Also(val =&gt; Debug.Assert(val &gt; 0, "Expected positive"))
      .Also(val =&gt; metrics.Record(val))
      .Let(val =&gt; val * 2);

PERFORMANCE CONSIDERATIONS:
- Zero overhead: Calls inline to the provided function/action
- No allocations beyond what the lambda captures
- JIT can inline these for optimal performance
- Equivalent performance to writing the code manually

```csharp
public static class FunctionalExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[FunctionalExtensions](Phx.Inject.Common.Util.FunctionalExtensions.md)

#### Inherited Members

[object.Equals\(object\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\)), 
[object.Equals\(object, object\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\-system\-object\)), 
[object.GetHashCode\(\)](https://learn.microsoft.com/dotnet/api/system.object.gethashcode), 
[object.GetType\(\)](https://learn.microsoft.com/dotnet/api/system.object.gettype), 
[object.MemberwiseClone\(\)](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone), 
[object.ReferenceEquals\(object, object\)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals), 
[object.ToString\(\)](https://learn.microsoft.com/dotnet/api/system.object.tostring)

## Methods

### <a id="Phx_Inject_Common_Util_FunctionalExtensions_Also__1___0_System_Action___0__"></a> Also<T\>\(T, Action<T\>\)

Performs an action on a value and returns the value.

```csharp
public static T Also<T>(this T self, Action<T> action)
```

#### Parameters

`self` T

The value.

`action` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<T\>

The action to perform.

#### Returns

 T

The original value.

#### Type Parameters

`T` 

The type of the value.

#### Remarks

PATTERN: Side effects in a transformation chain.

Use for logging, validation, metrics, or debugging without breaking the flow:
  return value
      .Also(v =&gt; Log($"Processing {v}"))
      .Also(v =&gt; Debug.Assert(v != null))
      .Let(Transform);

CHAINABLE:
Returns the original value, so you can chain multiple Also() calls or
continue with Let() transformations.

### <a id="Phx_Inject_Common_Util_FunctionalExtensions_Let__2___0_System_Func___0___1__"></a> Let<T, TResult\>\(T, Func<T, TResult\>\)

Transforms a value using the provided function.

```csharp
public static TResult Let<T, TResult>(this T self, Func<T, TResult> func)
```

#### Parameters

`self` T

The input value.

`func` [Func](https://learn.microsoft.com/dotnet/api/system.func\-2)<T, TResult\>

The transformation function.

#### Returns

 TResult

The result of applying the function to the input.

#### Type Parameters

`T` 

The type of the input value.

`TResult` 

The type of the result.

#### Remarks

PATTERN: Inline transformation without temporary variables.

Instead of:
  var temp = value;
  var result = Transform(temp);

Write:
  var result = value.Let(Transform);

Especially useful in expression contexts where statements aren't allowed,
or when building transformation pipelines.

### <a id="Phx_Inject_Common_Util_FunctionalExtensions_Then__1___0_System_Action___0__"></a> Then<T\>\(T, Action<T\>\)

Performs an action on a value.

```csharp
public static void Then<T>(this T self, Action<T> action)
```

#### Parameters

`self` T

The value.

`action` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<T\>

The action to perform.

#### Type Parameters

`T` 

The type of the value.

#### Remarks

PATTERN: Terminal operation for a transformation chain.

Use to end a chain with a side effect:
  value
      .Let(Transform)
      .Also(Validate)
      .Then(Save);  // Final operation, returns void

DIFFERENCE FROM ALSO:
- Also: Returns the value (chainable)
- Then: Returns void (terminal, cannot chain further)

This makes the intent clear: Then signals "this is the end of the chain."

