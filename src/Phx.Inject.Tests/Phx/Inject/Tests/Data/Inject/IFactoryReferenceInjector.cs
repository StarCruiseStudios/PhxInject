namespace Phx.Inject.Tests.Data.Inject {
    using Phx.Inject.Tests.Data.Model;
    using Phx.Inject.Tests.Data.Specification;

    // [Injector(
    //         typeof(FactoryReferenceSpec)
    // )]
    internal interface IFactoryReferenceInjector {
        public ILeaf GetLeaf();
        public void Build(LazyType lazyType);
    }
}
