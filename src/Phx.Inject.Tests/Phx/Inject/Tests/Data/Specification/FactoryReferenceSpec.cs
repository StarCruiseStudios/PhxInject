namespace Phx.Inject.Tests.Data.Specification {
    using System;
    using Phx.Inject.Tests.Data.Model;

    [Specification]
    [Link(typeof(IntLeaf), typeof(ILeaf))]
    internal static class FactoryReferenceSpec {
        [Factory]
        internal static int IntValue => 10;
        
        [Factory]
        internal static string StringValue => "stringvalue";

        [BuilderReference]
        internal static Action<LazyType, ILeaf> BuildLazyType => LazyType.Inject;
        
        [Label("Field")]
        [BuilderReference]
        internal static readonly Action<LazyType, ILeaf> BuildLazyTypeField = LazyType.Inject;

        [FactoryReference(FabricationMode.Scoped)]
        internal static Func<int, IntLeaf> GetIntLeaf => IntLeaf.Construct;
        
        [FactoryReference(FabricationMode.Scoped)]
        internal static readonly Func<string, StringLeaf> GetStringLeaf = StringLeaf.Construct;
    }
}
