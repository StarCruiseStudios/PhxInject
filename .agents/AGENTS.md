# Agent Instructions for PhxInject

This project uses code generation and AI assistants for development tasks. Follow the guidelines defined in this directory.

**Start here**: Read files in order to understand context before implementing changes.

## Core Guidelines (Apply to All Projects)

- **[Architecture Guide](architecture.md)** - System design, component relationships, and design patterns (start here for context)
- **[Coding Standards](coding-standards.md)** - C# code style, conventions, and quality guidelines
- **[Code Generation Practices](code-generation.md)** - Standards for source generators and generated code
- **[Testing Standards](testing.md)** - Testing strategy (to be defined)
- **[Documentation Standards](documentation.md)** - XML documentation and code comment guidelines

## Project-Specific Instructions

### [Phx.Inject - Core Library](../src/Phx.Inject/.agents/AGENTS.md)

The compiled public API and attributes for dependency injection:
- API design patterns
- Attribute contracts
- Public API surface rules

**Key file**: [api-design.md](../src/Phx.Inject/.agents/api-design.md) - Attribute design principles and naming conventions

### [Phx.Inject.Generator - Source Generator](../src/Phx.Inject.Generator/.agents/AGENTS.md)

Roslyn source generator that produces injection code at compile time:
- Generator pipeline architecture
- Code generation templates
- Symbol analysis and metadata extraction
- Diagnostic reporting

**Key file**: [generator-pipeline.md](../src/Phx.Inject.Generator/.agents/generator-pipeline.md) - Detailed five-stage pipeline description

## Quick Navigation

**For adding user-facing features**: Start with [Phx.Inject Instructions](../src/Phx.Inject/.agents/AGENTS.md)

**For modifying the generator**: Start with [Architecture Guide](architecture.md), then [Generator Instructions](../src/Phx.Inject.Generator/.agents/AGENTS.md)

**For writing tests**: See [Testing Standards](testing.md)

**For documentation**: See [Documentation Standards](documentation.md)

## Implementation Workflow

1. **Understand the context**: Read [Architecture Guide](architecture.md) to understand system design
2. **Plan your approach**: Identify which project(s) your work affects
3. **Read project instructions**: Refer to project-specific AGENTS.md files
4. **Follow standards**: Apply relevant standards from above
5. **Implement with validation**: Use the validation checklists in each guide
6. **Keep agent instructions updated**: When you modify architecture, design patterns, examples, or conventions, update the relevant agent instruction files to reflect those changes. This ensures the instructions remain a source of truth for future agents and developers.
