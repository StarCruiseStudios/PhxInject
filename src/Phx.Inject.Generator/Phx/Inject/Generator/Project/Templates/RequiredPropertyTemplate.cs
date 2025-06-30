namespace Phx.Inject.Generator.Project.Templates;

internal record RequiredPropertyTemplate(
    string PropertyName,
    SpecContainerFactoryInvocationTemplate PropertyValue
);
