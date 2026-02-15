# GitHub Copilot Instructions for PhxInject

This repository uses GitHub Copilot with custom instructions to maintain code quality and consistency across all PhxInject projects.

## Core Instruction Files

All instruction files follow the `.github/instructions/*.instructions.md` naming convention for automatic discovery by GitHub Copilot.

### Architecture and Design

- **[architecture.instructions.md](instructions/architecture.instructions.md)** - System design overview, component relationships, five-stage generator pipeline, design patterns, and system invariants

### Code Quality Standards

- **[coding-standards.instructions.md](instructions/coding-standards.instructions.md)** - C# coding conventions, naming patterns, error handling, nullability, collections, and formatting standards
- **[code-generation.instructions.md](instructions/code-generation.instructions.md)** - Source generator architecture, incremental generator patterns, generated code standards, and performance optimization
- **[documentation.instructions.md](instructions/documentation.instructions.md)** - XML documentation guidelines, when to document, formatting standards, and examples

### Development Practices

- **[testing.instructions.md](instructions/testing.instructions.md)** - Unit testing, integration testing, snapshot testing for generated code, diagnostic validation, and minimum coverage requirements
- **[testing-phxinject.instructions.md](instructions/testing-phxinject.instructions.md)** - PhxInject-specific testing patterns using Phx.Test orchestration and PhxValidation assertions
- **[code-review.instructions.md](instructions/code-review.instructions.md)** - Code review checklist, priority system, and review standards
- **[performance.instructions.md](instructions/performance.instructions.md)** - Performance optimization patterns for source generators and generated code
- **[security.instructions.md](instructions/security.instructions.md)** - Security guidelines for source generators and dependency injection frameworks

### Process and Workflow

- **[github-actions.instructions.md](instructions/github-actions.instructions.md)** - CI/CD pipeline configuration, build automation, testing workflows, and deployment patterns
- **[taming-copilot.instructions.md](instructions/taming-copilot.instructions.md)** - Guidelines for controlling Copilot behavior and ensuring minimal, surgical code changes
- **[context-engineering.instructions.md](instructions/context-engineering.instructions.md)** - Best practices for structuring code to maximize Copilot effectiveness

## Project-Specific Instructions

Some projects have additional context-specific instruction files:

- **[src/Phx.Inject/.agents/AGENTS.md](../src/Phx.Inject/.agents/AGENTS.md)** - Core public API guidance
- **[src/Phx.Inject.Generator/.agents/AGENTS.md](../src/Phx.Inject.Generator/.agents/AGENTS.md)** - Source generator implementation details

## Skills Directory

The `.github/skills/` directory contains reusable domain-specific skills:

- **agentic-eval** - Evaluation and validation patterns
- **excalidraw-diagram-generator** - Diagram generation from natural language
- **git-commit** - Conventional commit message analysis and generation
- **microsoft-code-reference** - Microsoft API reference lookup
- **microsoft-docs** - Query official Microsoft documentation
- **nuget-manager** - NuGet package management
- **refactor** - Surgical code refactoring patterns

See [.github/skills/README.md](skills/README.md) for detailed skill descriptions.

## Implementation Workflow

When working on PhxInject code, follow this workflow:

### 1. Understand the Context

- Read [architecture.instructions.md](instructions/architecture.instructions.md) to understand system design
- Identify which pipeline stage your work affects (Metadata → Core → Linking → Code Generation → Rendering)
- Review project-specific instructions if working in a specific project

### 2. Follow Standards

- Apply [coding-standards.instructions.md](instructions/coding-standards.instructions.md) for all C# code
- Follow [code-generation.instructions.md](instructions/code-generation.instructions.md) for generator work
- Document using [documentation.instructions.md](instructions/documentation.instructions.md) guidelines
- Review [taming-copilot.instructions.md](instructions/taming-copilot.instructions.md) to ensure minimal changes

### 3. Write Tests

- Create tests following [testing.instructions.md](instructions/testing.instructions.md)
- Target minimum 85% code coverage
- Include diagnostic validation tests for error cases
- Add snapshot tests for generated code changes

### 4. Validate Performance

- Check [performance.instructions.md](instructions/performance.instructions.md) for generator optimization patterns
- Ensure incremental generator patterns are used correctly
- Avoid expensive operations (enumerating all symbols, repeated semantic analysis)

### 5. Security Review

- Review [security.instructions.md](instructions/security.instructions.md) for source generator security concerns
- Validate input at generation time, not runtime
- Ensure generated code doesn't introduce vulnerabilities

### 6. Code Review

- Self-review using [code-review.instructions.md](instructions/code-review.instructions.md) checklist
- Address all 🔴 Critical items before submitting
- Document any 🟡 Important items that are deferred

## Key Principles

1. **Zero Runtime Overhead**: All dependency resolution at compile time
2. **Compile-Time Validation**: Detect all errors at build time, not runtime
3. **Incremental by Default**: Use Roslyn incremental APIs for performance
4. **Fail Fast with Diagnostics**: Never throw exceptions to the compiler; report diagnostics instead
5. **Readable Generated Code**: Generated code must be human-readable and debuggable
6. **Type Safety**: Leverage C# type system to prevent misconfigurations

## Technologies

- **C# 14** with nullable reference types enabled
- **.NET 8+** for all projects
- **Roslyn Incremental Source Generators** for code generation
- **Domain-Driven Design** principles throughout
- **SOLID** design patterns

## Getting Help

When Copilot needs clarification:

1. Reference specific instruction files in your questions
2. Identify which pipeline stage is relevant
3. Quote specific sections from instruction files
4. Provide context about what you're trying to achieve

## Questions?

- For architecture questions: See [architecture.instructions.md](instructions/architecture.instructions.md)
- For coding questions: See [coding-standards.instructions.md](instructions/coding-standards.instructions.md)
- For testing questions: See [testing.instructions.md](instructions/testing.instructions.md)
- For performance questions: See [performance.instructions.md](instructions/performance.instructions.md)
