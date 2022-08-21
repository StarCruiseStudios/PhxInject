namespace Phx.Inject.Tests.Data.Inject {
    using Phx.Inject.Tests.Data.Model;
    using Phx.Inject.Tests.Data.Specification;

    [Injector(
            specifications: new[] {
                typeof(IPropertyFactorySpec),
                typeof(PropertyFactoryStaticSpec)
            })]
    internal interface IPropertyFactoryInjector {
        public ILeaf GetLeaf();
        public StringLeaf GetStringLeaf();
    }
}
