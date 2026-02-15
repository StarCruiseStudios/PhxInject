# Agent Skills

This directory contains Agent Skills that enhance GitHub Copilot's ability to perform specialized tasks in this repository. These skills are automatically loaded by Copilot coding agent, GitHub Copilot CLI, and VS Code Insiders when relevant.

## Available Skills

### NuGet Manager
**Location:** [`nuget-manager/`](nuget-manager/)  
**When to use:** Managing NuGet package dependencies (adding, removing, or updating packages)

Ensures consistent and safe NuGet package management using `dotnet` CLI commands. Enforces proper workflows for version updates with verification and restoration steps.

### Refactor
**Location:** [`refactor/`](refactor/)  
**When to use:** Improving code structure and maintainability

Provides surgical refactoring patterns for C# code including:
- Extracting methods and classes
- Eliminating code smells
- Breaking down large classes
- Improving type safety
- Applying design patterns

### Git Commit
**Location:** [`git-commit/`](git-commit/)  
**When to use:** Creating standardized git commits

Implements Conventional Commits specification with:
- Auto-detection of commit type and scope from diffs
- Intelligent file staging
- Standardized commit message generation
- Git safety protocols

### Microsoft Docs
**Location:** [`microsoft-docs/`](microsoft-docs/)  
**When to use:** Searching Microsoft documentation

Research skill for the Microsoft technology ecosystem including:
- learn.microsoft.com content (Azure, .NET, M365, etc.)
- .NET Aspire documentation
- VS Code documentation  
- GitHub documentation
- Agent Framework documentation

### Microsoft Code Reference
**Location:** [`microsoft-code-reference/`](microsoft-code-reference/)  
**When to use:** Looking up .NET/Azure API references

Validates and finds working code examples for Microsoft SDKs:
- API method and class lookups
- Working code samples from official docs
- Error troubleshooting
- SDK version verification

### Agentic Eval
**Location:** [`agentic-eval/`](agentic-eval/)  
**When to use:** Implementing agent evaluation and iterative refinement

Provides patterns for self-improvement through evaluation:
- Reflection loops for self-critique
- Evaluator-optimizer pipelines
- Test-driven code refinement
- Rubric-based evaluation
- LLM-as-judge patterns

### Excalidraw Diagram Generator
**Location:** [`excalidraw-diagram-generator/`](excalidraw-diagram-generator/)  
**When to use:** Creating visual diagrams from descriptions

Generates Excalidraw diagrams for visualization:
- Flowcharts and process diagrams
- Class diagrams (C# focused)
- Architecture and system diagrams
- Sequence diagrams
- Relationship diagrams
- Mind maps

## How Skills Work

When performing tasks, Copilot automatically decides when to use these skills based on:
1. Your prompt/request
2. The skill's description
3. Current context

When a skill is selected, its `SKILL.md` file is injected into Copilot's context, providing detailed instructions for that specific task.

## Skill Structure

Each skill is organized as:
```
skill-name/
  SKILL.md          # Main skill instructions (required)
  *.* (optional)    # Additional scripts or resources
```

## Creating New Skills

To add a new skill:

1. Create a new subdirectory under `.github/skills/`
2. Name it with lowercase and hyphens (e.g., `my-new-skill`)
3. Create a `SKILL.md` file with:
   - YAML frontmatter (name, description, optional license)
   - Markdown body with instructions and examples

Example:
```markdown
---
name: my-skill
description: Brief description when Copilot should use this skill
---

# My Skill

Instructions for Copilot to follow...
```

## Reference

- [Agent Skills Documentation](https://docs.github.com/en/copilot/concepts/agents/about-agent-skills)
- [Awesome Copilot Skills Collection](https://github.com/github/awesome-copilot/tree/main/skills)
- [Agent Skills Standard](https://github.com/agentskills/agentskills)
