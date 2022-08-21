namespace Phx.Inject.Tests.Data.Specification {
    using System;
    using Phx.Inject.Tests.Data.Model;

    [Specification]
    [Link(typeof(IntLeaf), typeof(ILeaf))]
    internal static class FactoryReferenceSpec {
        [Factory]
        internal static int IntValue => 10;

        [BuilderReference]
        internal static readonly Action<LazyType, ILeaf> BuildLazyType = LazyType.Inject;

        [FactoryReference(FabricationMode.Scoped)]
        internal static readonly Func<int, IntLeaf> GetIntLeaf = IntLeaf.Construct;
    }
}
