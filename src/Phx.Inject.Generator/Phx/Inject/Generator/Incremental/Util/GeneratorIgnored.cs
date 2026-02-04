namespace Phx.Inject.Generator.Incremental.Util;

/// <summary>
///     Wraps a type in a way that only checks type identity for equality comparisons, allowing
///     a value to be used in incremental generator models without triggering regeneration when the
///     value changes.
/// </summary>
/// <param name="value"> The value referenced. </param>
public class GeneratorIgnored<T>(T value) {
    public T Value { get; } = value;
    
    /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
    public virtual bool Equals(GeneratorIgnored<T>? other) {
        return other is not null;
    }

    /// <inheritdoc cref="Object.GetHashCode"/>
    public override int GetHashCode() {
        return typeof(T).GetHashCode();
    }
    
    /// <inheritdoc cref="Object.ToString"/>
    public override string ToString() {
        return Value?.ToString() ?? "null";
    }
}

public static class GeneratorIgnoredExtensions {
    public static GeneratorIgnored<T> GeneratorIgnored<T>(this T value) {
        return new GeneratorIgnored<T>(value);
    }
}