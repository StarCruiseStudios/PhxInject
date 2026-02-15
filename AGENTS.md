# Agent Instructions for PhxInject

This repository uses AI code agents and GitHub Copilot to assist with development tasks.

## GitHub Copilot Instructions

**Start here**: [GitHub Copilot Instructions](.github/copilot-instructions.md)

All instruction files follow the `.github/instructions/*.instructions.md` naming convention for automatic discovery by GitHub Copilot.

### Core Guidelines

#### Architecture and Design
- **[Architecture Guide](.github/instructions/architecture.instructions.md)** - System design, five-stage pipeline, design patterns

#### Code Quality Standards
- **[Coding Standards](.github/instructions/coding-standards.instructions.md)** - C# coding conventions, naming, error handling
- **[Code Generation](.github/instructions/code-generation.instructions.md)** - Source generator patterns and standards
- **[Documentation](.github/instructions/documentation.instructions.md)** - XML documentation guidelines

#### Development Practices
- **[Testing Standards](.github/instructions/testing.instructions.md)** - Unit, integration, and snapshot testing
- **[Code Review](.github/instructions/code-review.instructions.md)** - Review checklist and standards
- **[Performance](.github/instructions/performance.instructions.md)** - Optimization patterns for generators
- **[Security](.github/instructions/security.instructions.md)** - Security guidelines for source generators

#### Process and Workflow
- **[GitHub Actions](.github/instructions/github-actions.instructions.md)** - CI/CD pipeline configuration
- **[Taming Copilot](.github/instructions/taming-copilot.instructions.md)** - Controlling Copilot behavior
- **[Context Engineering](.github/instructions/context-engineering.instructions.md)** - Structuring code for Copilot

### Project-Specific Instructions

Some projects have additional context-specific instruction files:
- **[Phx.Inject Library](src/Phx.Inject/.agents/AGENTS.md)** - Core public API guidance
- **[Phx.Inject.Generator](src/Phx.Inject.Generator/.agents/AGENTS.md)** - Source generator implementation

### Skills Directory

The `.github/skills/` directory contains reusable domain-specific skills. See [.github/skills/README.md](.github/skills/README.md) for details.

## Legacy Agent Instructions

**Note**: The `.agents/` directory structure has been deprecated in favor of the standard `.github/instructions/` structure. All instructions have been migrated to the new location with the `.instructions.md` suffix for automatic GitHub Copilot discovery.
