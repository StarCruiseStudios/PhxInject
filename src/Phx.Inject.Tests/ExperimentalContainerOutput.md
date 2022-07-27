```cs
internal class GeneratedConstructedInjector_IConstructedSpecificationContainer {
    // private IConstructedSpecification specification;
    // public GeneratedConstructedInjector_IConstructedSpecificationContainer(IConstructedSpecification specification) {
    //     this.specification = specification;
    // }

    internal int GetIntValue(ISpecContainerCollection specContainers) {
        return specification.GetIntValue();
    }

    internal IntLeaf GetIntLeaf(ISpecContainerCollection specContainers) {
        return specification.GetIntLeaf(specContainers.GeneratedConstructedInjector_IConstructedSpecificationContainer.GetIntValue(specContainers));
    }
}
```

```cs
internal partial class GeneratedConstructedInjector : Phx.Inject.Tests.Data.Inject.IConstructedInjector {
    internal interface ISpecContainerCollection {
        GeneratedConstructedInjector_IConstructedSpecificationContainer GeneratedConstructedInjector_IConstructedSpecificationContainer { get; }
    }

    internal class SpecContainerCollection : ISpecContainerCollection {
        GeneratedConstructedInjector_IConstructedSpecificationContainer GeneratedConstructedInjector_IConstructedSpecificationContainer { get; }

        public SpecContainerCollection(GeneratedConstructedInjector_IConstructedSpecificationContainer GeneratedConstructedInjector_IConstructedSpecificationContainer) {
            this.GeneratedConstructedInjector_IConstructedSpecificationContainer = GeneratedConstructedInjector_IConstructedSpecificationContainer;
        }
    }

    private readonly ISpecContainerCollection specContainers;

    public GeneratedConstructedInjector(IConstructedSpecification specification) {
        specContainers = new SpecContainerCollection(
            new GeneratedConstructedInjector_IConstructedSpecificationContainer(specification)
        );
    }
}
```