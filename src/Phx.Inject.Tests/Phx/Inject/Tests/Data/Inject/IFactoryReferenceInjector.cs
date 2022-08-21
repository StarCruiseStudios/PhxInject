namespace Phx.Inject.Tests.Data.Inject {
    using Phx.Inject.Tests.Data.Model;
    using Phx.Inject.Tests.Data.Specification;

    [Injector(
            typeof(FactoryReferenceSpec)
    )]
    internal interface IFactoryReferenceInjector {
        public IntLeaf GetIntLeaf();
        public StringLeaf GetStringLeaf();
        
        // public void Build(LazyType lazyType);
    }
}
