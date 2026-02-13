// -----------------------------------------------------------------------------
// <copyright file="FunctionalExtensions.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Common.Util;

/// <summary>
/// Extension methods for functional programming patterns inspired by Kotlin/Scala.
/// 
/// PURPOSE:
/// - Enables functional transformation and side-effect patterns in C#
/// - Reduces temporary variables and improves code flow
/// - Makes complex transformations more readable through chaining
/// 
/// WHY THIS EXISTS:
/// C# has limited built-in support for functional patterns that are common in other languages.
/// These extensions bring three key patterns:
/// 
/// 1. Let: Transform a value inline (Kotlin's let, Scala's map)
/// 2. Also: Perform side effects while passing through a value (Kotlin's also, Scala's tap)
/// 3. Then: Terminate a chain with a side effect (Kotlin's also at end of chain)
/// 
/// PATTERN COMPARISON:
/// 
/// Traditional C#:
///   var temp = GetValue();
///   var transformed = Transform(temp);
///   Log(transformed);
///   DoSomething(transformed);
///   return transformed;
/// 
/// With functional extensions:
///   return GetValue()
///       .Let(Transform)
///       .Also(Log)
///       .Also(DoSomething);
/// 
/// DESIGN DECISIONS:
/// 
/// 1. Why "Let" instead of "Map" or "Select"?
///    - LINQ already has Select() for IEnumerable
///    - "Let" is Kotlin terminology, familiar to many developers
///    - Avoids confusion with LINQ operators
/// 
/// 2. Why both "Also" and "Then"?
///    - Also: Returns the value (chainable, for multiple side effects)
///    - Then: Returns void (terminal operation, signals end of chain)
///    - Different signatures prevent accidental misuse
/// 
/// 3. Why are these useful in generators?
///    - Generators involve multi-stage transformations (syntax → symbols → metadata → code)
///    - Each stage may need logging, validation, or diagnostics
///    - These patterns make transformation pipelines explicit and linear
/// 
/// USAGE EXAMPLES:
/// 
/// Transformation chain:
///   var metadata = symbol
///       .Let(ExtractInfo)
///       .Let(ValidateInfo)
///       .Also(info => Log($"Processing {info.Name}"))
///       .Let(CreateMetadata);
/// 
/// Builder pattern enhancement:
///   return new StringBuilder()
///       .Also(sb => sb.AppendLine("// Generated code"))
///       .Also(sb => sb.AppendLine($"class {className}"))
///       .Also(sb => AppendMembers(sb, members))
///       .ToString();
/// 
/// Side effects without breaking flow:
///   var result = CalculateValue()
///       .Also(val => Debug.Assert(val > 0, "Expected positive"))
///       .Also(val => metrics.Record(val))
///       .Let(val => val * 2);
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - Zero overhead: Calls inline to the provided function/action
/// - No allocations beyond what the lambda captures
/// - JIT can inline these for optimal performance
/// - Equivalent performance to writing the code manually
/// </summary>
public static class FunctionalExtensions {
    /// <summary>
    ///     Transforms a value using the provided function.
    /// </summary>
    /// <remarks>
    /// PATTERN: Inline transformation without temporary variables.
    /// 
    /// Instead of:
    ///   var temp = value;
    ///   var result = Transform(temp);
    ///   
    /// Write:
    ///   var result = value.Let(Transform);
    /// 
    /// Especially useful in expression contexts where statements aren't allowed,
    /// or when building transformation pipelines.
    /// </remarks>
    /// <typeparam name="T"> The type of the input value. </typeparam>
    /// <typeparam name="TResult"> The type of the result. </typeparam>
    /// <param name="self"> The input value. </param>
    /// <param name="func"> The transformation function. </param>
    /// <returns> The result of applying the function to the input. </returns>
    public static TResult Let<T, TResult>(this T self, Func<T, TResult> func) {
        return func(self);
    }

    /// <summary>
    ///     Performs an action on a value and returns the value.
    /// </summary>
    /// <remarks>
    /// PATTERN: Side effects in a transformation chain.
    /// 
    /// Use for logging, validation, metrics, or debugging without breaking the flow:
    ///   return value
    ///       .Also(v => Log($"Processing {v}"))
    ///       .Also(v => Debug.Assert(v != null))
    ///       .Let(Transform);
    /// 
    /// CHAINABLE:
    /// Returns the original value, so you can chain multiple Also() calls or
    /// continue with Let() transformations.
    /// </remarks>
    /// <typeparam name="T"> The type of the value. </typeparam>
    /// <param name="self"> The value. </param>
    /// <param name="action"> The action to perform. </param>
    /// <returns> The original value. </returns>
    public static T Also<T>(this T self, Action<T> action) {
        action(self);
        return self;
    }

    /// <summary>
    ///     Performs an action on a value.
    /// </summary>
    /// <remarks>
    /// PATTERN: Terminal operation for a transformation chain.
    /// 
    /// Use to end a chain with a side effect:
    ///   value
    ///       .Let(Transform)
    ///       .Also(Validate)
    ///       .Then(Save);  // Final operation, returns void
    /// 
    /// DIFFERENCE FROM ALSO:
    /// - Also: Returns the value (chainable)
    /// - Then: Returns void (terminal, cannot chain further)
    /// 
    /// This makes the intent clear: Then signals "this is the end of the chain."
    /// </remarks>
    /// <typeparam name="T"> The type of the value. </typeparam>
    /// <param name="self"> The value. </param>
    /// <param name="action"> The action to perform. </param>
    public static void Then<T>(this T self, Action<T> action) {
        action(self);
    }
}
