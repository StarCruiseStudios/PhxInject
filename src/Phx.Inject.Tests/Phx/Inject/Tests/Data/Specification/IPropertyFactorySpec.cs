namespace Phx.Inject.Tests.Data.Specification {
    using Phx.Inject.Tests.Data.Model;
    [Specification]
    internal interface IPropertyFactorySpec {
        [Factory]
        public ILeaf Leaf { get; }
    }
    
    internal record PropertyFactorySpec(ILeaf Leaf) : IPropertyFactorySpec;
    
    [Specification]
    internal static class PropertyFactoryStaticSpec {
        public const string StringValue = "Hello";
        [Factory]
        public static StringLeaf StringLeaf { get; } = new StringLeaf(StringValue);
    }
}
