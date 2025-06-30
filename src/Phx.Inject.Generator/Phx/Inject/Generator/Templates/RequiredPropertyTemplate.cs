namespace Phx.Inject.Generator.Templates;

internal record RequiredPropertyTemplate(
    string PropertyName,
    SpecContainerFactoryInvocationTemplate PropertyValue
);
