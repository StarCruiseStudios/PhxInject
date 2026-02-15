# C# Documentation Standards for PhxInject

This guide establishes standards for XML documentation comments in the PhxInject projects. All public API and critical internal functionality must be documented following these guidelines.

## Core Principles

### When to Document

**Always document:**
- All public types, members, and methods (part of external API contract)

**Document internal members when:**
- They implement critical functionality in the five-stage pipeline
- They have non-obvious behavior or important invariants
- They have architectural significance within system design
- They have important threading, performance, or side-effect implications
- They are called from multiple places where understanding their contract matters

**Skip documentation when:**
- Code is trivially obvious (self-documenting)
- Member is immediately deprecated
- It's a simple getter/setter with no special semantics
- Comments would merely restate what the declaration already conveys

### What to Avoid

- **Low-value comments**: Comments that describe what code literally does without adding insight
- **Redundant comments**: Remove if information is trivially derived from the declaration
- **Implementation details**: Don't document "how" unless it affects usage
- **HTML formatting tags**: Use only C# XML doc comment tags (`<c>`, `<see>`, `<paramref>`, etc.)
  - **Never use** `<para>`, `<list type="bullet">`, or other HTML-style formatting
  - **Use markdown instead**: bullet points with `-`, section headers with `##`, blank lines for paragraphs

## XML Documentation Tags

### For All Members

| Tag | Purpose | Example |
|-----|---------|---------|
| `<summary>` | Brief, one-sentence description using present-tense, third-person verb | `Gets the current state of the injector.` |
| `<remarks>` | Additional information, implementation details, usage notes, architectural context | Detailed explanation in multiple paragraphs |
| `<c>` | Inline code snippets (keywords, types, identifiers) | `<c>null</c>`, `<c>true</c>`, `<c>[Factory]</c>` |
| `<see cref="..." />` | Inline reference to types/members (in sentences) | `See <see cref="InjectorBuilder"/> for configuration.` |
| `<seealso cref="..." />` | Standalone "See Also" section references | Listed at end of documentation |
| `<see langword="..." />` | Language-specific keywords | `<see langword="true" />`, `<see langword="null" />` |
| `<inheritdoc/>` | Inherit documentation from base classes/interfaces | Use when member semantics are identical |

### For Methods

| Tag | Purpose | Format |
|-----|---------|--------|
| `<param>` | Parameter description | Noun phrase starting with article; specific wording by type |
| `<paramref>` | Reference parameter name in text | `where <paramref name="factory"/> defines...` |
| `<typeparam>` | Generic type parameter description | Noun phrase describing constraint |
| `<typeparamref>` | Reference to type parameter | `The type <typeparamref name="T"/>...` |
| `<returns>` | What method returns | Noun phrase starting with article |
| `<exception cref="...">` | Exceptions thrown directly by this member | Only document exceptions users will encounter |

**Parameter Wording by Type:**

- **Boolean**: `<see langword="true" /> to [action]; otherwise, <see langword="false" />.`
- **Flag Enum**: `A bitwise combination of the enumeration values that specifies...`
- **Non-Flag Enum**: `One of the enumeration values that specifies...`
- **Out Parameter**: `When this method returns, contains [description]. This parameter is treated as uninitialized.`

**Boolean Returns:**

- `<see langword="true" /> if [condition]; otherwise, <see langword="false" />.`

### For Properties

**`<summary>` format:**

- Read-write property: `Gets or sets [description].`
- Read-only property: `Gets [description].`
- Boolean property: `Gets [or sets] a value that indicates whether [condition].`

**`<value>` tag for property values:**

- Description as noun phrase (don't specify data type)
- For Boolean with default: `<see langword="true" /> if [condition]; otherwise, <see langword="false" />. The default is [value].`
- Always include default value if applicable: `The default is <see langword="false" />.`

### For Constructors

**`<summary>` format:**

- `Initializes a new instance of the [class/struct name].`

### For Types (Classes, Interfaces, Records)

**`<summary>`:** Brief description of the type's purpose

**`<remarks>`:** Include all relevant context:

- **Why the type exists**: Problem it solves or role it plays
- **Architectural role**: Position in the pipeline (Metadata, Core, Linking, Code Generation, Rendering)
- **Key invariants**: Important constraints on state or behavior
- **Usage context**: When to use/not use this type
- **Threading model**: Thread-safety guarantees if applicable
- **Performance characteristics**: Important performance implications if non-obvious

**Example structure for remarks:**
```xml
/// <remarks>
/// This analyzer is the first stage of the five-stage pipeline. It examines 
/// source code for injection attributes and extracts metadata used by 
/// subsequent stages (Core, Linking, Code Generation, and Rendering).
///
/// ## Key Invariants
///
/// - Never throws exceptions from <see cref="Execute"/> method
/// - All diagnostics reported via <see cref="ReportDiagnostic"/>
/// - Thread-safe across multiple compilation units
///
/// ## Usage
///
/// This analyzer is automatically invoked by the source generator. 
/// For testing, use <see cref="AnalyzerTestHarness"/>.
/// </remarks>
```

### For Enum Values

**Document each enum value with `<summary>`:**

```csharp
internal enum SpecInstantiationMode {
    /// <summary>
    /// The specification has only static members; no instance creation needed.
    /// </summary>
    Static = 0,

    /// <summary>
    /// The injector creates a new specification instance per request.
    /// </summary>
    Instantiated = 1,
}
```

**Do NOT document the enum type itself** if you're only explaining what the values mean. The value-level summaries are sufficient.

## Markdown Formatting in Remarks

Use markdown formatting within `<remarks>` tags for readability:

- **Section headers**: `## Section Name` (reduces visual clutter compared to bold text)
- **Bullet lists**: Use `-` for unordered lists
- **Bold text**: `**term**` for emphasis within sentences
- **Code inline**: `<c>identifier</c>` for code references
- **Paragraphs**: Separate with blank lines (no `<para>` tags)

**Example with markdown formatting:**

```xml
/// <remarks>
/// Extracts optional <c>FabricationMode</c> for parameters receiving auto-generated factory
/// delegates. Generator analyzes target type constructor/dependencies and creates factory
/// on-demand without explicit <c>[Factory]</c> method.
///
/// ## FabricationMode Options
///
/// - **Transient**: Each factory call creates a new instance. No storage needed.
/// - **Scoped**: First factory call creates instance, subsequent calls return cached instance.
/// - **Container/ContainerScoped**: Container-hierarchy scoping for child injectors.
///
/// ## Validation Constraints
///
/// - Parameter type is <c>Func&lt;T&gt;</c> or compatible delegate type
/// - Target type T has accessible constructor or static factory method
/// - All transitive dependencies for T can be resolved from injector
/// </remarks>
```

## Common Documentation Patterns

### Method with Boolean Parameter

```csharp
/// <summary>
/// Configures whether the injector validates specifications at build time.
/// </summary>
/// <param name="validate">
/// <see langword="true" /> to validate specifications during build; 
/// otherwise, <see langword="false" /> to skip validation.
/// </param>
public void ConfigureValidation(bool validate) { }
```

### Property with Default Value

```csharp
/// <summary>
/// Gets or sets a value that indicates whether this specification is scoped.
/// </summary>
/// <value>
/// <see langword="true" /> if the specification is scoped to a single request; 
/// otherwise, <see langword="false" /> if it's a singleton.
/// The default is <see langword="false" />.
/// </value>
public bool IsScoped { get; set; }
```

### Enum Type (Values Documented Only)

```csharp
internal enum FabricationMode {
    /// <summary>
    /// Each factory call creates a new instance.
    /// </summary>
    Transient = 0,

    /// <summary>
    /// First factory call creates instance; subsequent calls return cached instance within scope.
    /// </summary>
    Scoped = 1,

    /// <summary>
    /// Container-hierarchy scoping for child injectors.
    /// </summary>
    ContainerScoped = 2,
}
```

### Type with Architectural Context

```csharp
/// <summary>
/// Analyzes C# source code during the metadata stage of code generation.
/// </summary>
/// <remarks>
/// This analyzer is the first stage of the five-stage pipeline. It examines 
/// source code for injection attributes and extracts metadata used by 
/// subsequent stages (Core, Linking, Code Generation, and Rendering).
///
/// ## Key Invariants
///
/// - Never throws exceptions from <see cref="Execute"/> method
/// - All diagnostics reported via <see cref="ReportDiagnostic"/>
/// - Thread-safe across multiple compilation units
///
/// ## Usage
///
/// This analyzer is automatically invoked by the source generator. 
/// For testing, use <see cref="AnalyzerTestHarness"/>.
/// </remarks>
internal class MetadataAnalyzer { }
```

## Pipeline Context for Phx.Inject.Generator

When documenting components in **Phx.Inject.Generator**, reference the five-stage pipeline:

1. **Metadata Stage** - Examine and extract metadata from source code
2. **Core Stage** - Build core models from extracted metadata
3. **Linking Stage** - Link models together and resolve references
4. **Code Generation Stage** - Generate C# code from linked models
5. **Rendering Stage** - Output generated code to files

Include the relevant pipeline stage in the type's `<remarks>` section when the component is part of a specific stage.

## Project-Specific Guidelines

### Phx.Inject (Core Library)

Public API must be thoroughly documented:
- Every public type, method, and property
- Design intent and user guidance
- Realistic examples in remarks when non-obvious
- All exceptions thrown directly

**Skip documentation for:**
- Simple auto-generated properties
- Trivial getter/setter patterns

### Phx.Inject.Generator (Source Generator)

Document critical pipeline components:
- All public API
- Internal analyzer/generator classes that users need to understand
- Public extension methods
- Diagnostic descriptor types
- Methods that significantly affect behavior

**Skip documentation for:**
- Private helper methods
- Trivial internal utilities
- One-off utility methods with obvious purpose

## Validation Checklist

Before considering documentation complete:

- [ ] All public types have documentation
- [ ] All public methods/properties have documentation
- [ ] All critical internal members have documentation
- [ ] No redundant comments (comments that merely restate code)
- [ ] No low-value comments (comments without insight)
- [ ] All code references use proper tags:
  - `<see cref="..."/>` for types/members
  - `<paramref name="..."/>` for parameters
  - `<c>...</c>` for inline code and keywords
  - `<see langword="..."/>` for language keywords
- [ ] No HTML formatting tags (`<para>`, `<list>`, `<item>`, `<term>`, `<description>`)
- [ ] Use markdown formatting instead (headers with `##`, lists with `-`, blank lines)
- [ ] Enum values documented; enum type not documented (if only explaining values)
- [ ] Architectural role and usage context are clear
- [ ] Parameter semantics explained (especially non-obvious ones)
- [ ] Important behavioral guarantees stated (e.g., "never returns null", "idempotent")
- [ ] Side effects documented if non-obvious
- [ ] Comments focus on intent ("why") not implementation ("how")

## Decision Tree: Should I Document This?

```
Is this public API?
├─ Yes → ALWAYS document, thoroughly
└─ No
    ├─ Is it critical pipeline functionality?
    │  ├─ Yes → Document the architectural role
    │  └─ No → Continue
    ├─ Would another engineer wonder "why"? (not "what")
    │  ├─ Yes → Document the reason
    │  └─ No → Skip
```

## Common Mistakes to Avoid

| Mistake | ❌ Bad | ✅ Good |
|---------|--------|--------|
| Redundant constant docs | `/// <summary> The string type name. </summary> public const string StringType = "string";` | Remove the comment entirely |
| Enum type + values | Document both enum type and values (redundant) | Document values only with `<summary>` tags |
| HTML formatting | `<para>` tags, `<list type="bullet">`, `<item>`, `<description>` | Markdown: `##`, `-`, blank lines |
| Missing code references | `Parameter x is of type Foo.` | Parameter <paramref name="x"/> is of type <see cref="Foo"/>. |
| Implementation focus | `/// <summary> Sets the internal cache field. </summary>` | `/// <summary> Invalidates cached data. </summary>` (focuses on effect) |
| Missing default values | `/// <value> Sets the timeout period. </value>` | `/// <value> Sets the timeout period. The default is 5000 milliseconds. </value>` |

## InheritDoc Usage

Use `<inheritdoc/>` when:
- Implementing an interface method with identical semantics
- Overriding a virtual method with no behavior change
- Implementing explicit interface members where meaning doesn't change

Do NOT use `<inheritdoc/>` when:
- Behavior is significantly different from base/interface
- Adding important context specific to this implementation
- The inherited documentation is incomplete for your context

Example:
```csharp
public interface IFactory {
    /// <summary>
    /// Creates a new instance of the target type.
    /// </summary>
    /// <returns>A new instance.</returns>
    object Create();
}

public class ConcreteFactory : IFactory {
    /// <inheritdoc />
    public object Create() => new TargetType();
}
```

## Related Files

- [architecture.instructions.md](architecture.instructions.md) - System design and pipeline documentation
- [coding-standards.instructions.md](coding-standards.instructions.md) - C# code formatting and naming
- [code-generation.instructions.md](code-generation.instructions.md) - Source generator implementation patterns
