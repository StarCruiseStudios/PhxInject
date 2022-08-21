namespace Phx.Inject {
    using System;

    /// <summary>
    ///     Annotates a field or property that references a builder method that
    ///     will be invoked to complete the construction of a given dependency.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BuilderReferenceAttribute : Attribute { }
}
