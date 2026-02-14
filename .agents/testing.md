# Testing Standards for PhxInject

Standards for unit and integration tests across all test projects. All new code must include tests before completion.

**STATUS**: Testing strategy to be detailed in collaboration with development team. For now, follow these basic guidelines:

## Current Guidelines

### General Principles

- All new code must include unit tests
- Tests validate both the generator pipeline logic and generated code output
- Use NUnit test framework (existing standard)
- Test failures should indicate clear, actionable problems

### Test Organization

Tests are organized by pipeline stage and component:

- **Stage 1 (Metadata)**: Tests for metadata extraction from user code
- **Stage 2 (Core)**: Tests for domain model transformation
- **Stage 3 (Linking)**: Tests for dependency resolution and linking logic
- **Stage 4 (Code Generation)**: Tests for template generation
- **Stage 5 (Rendering)**: Tests for C# code output

### Test Data

- Use test fixtures and builders to create test data
- Avoid hardcoded duplication of test setup
- Make test intent clear through naming

### Coverage Expectations

- Critical pipeline logic: aim for comprehensive coverage
- Generator components: unit tests for individual functions
- Generated code: validation tests that code compiles and works
- Error cases: verify error diagnostics are reported correctly

## To Be Detailed

The following will be defined in collaboration with the development team:

- Detailed test patterns for each pipeline stage
- Test data builder patterns and fixtures
- Snapshot testing approach for generated code
- Performance/regression testing strategy
- Integration test workflows
- Legacy version testing approach
