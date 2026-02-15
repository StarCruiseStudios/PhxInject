---
name: agentic-eval
description: |
  Patterns and techniques for evaluating and improving AI agent outputs. Use this skill when:
  - Implementing self-critique and reflection loops
  - Building evaluator-optimizer pipelines for quality-critical generation
  - Creating test-driven code refinement workflows
  - Designing rubric-based or LLM-as-judge evaluation systems
  - Adding iterative improvement to agent outputs (code, reports, analysis)
  - Measuring and improving agent response quality
---

# Agentic Evaluation Patterns

Patterns for self-improvement through iterative evaluation and refinement.

## Overview

Evaluation patterns enable agents to assess and improve their own outputs, moving beyond single-shot generation to iterative refinement loops.

```
Generate → Evaluate → Critique → Refine → Output
    ↑                              │
    └──────────────────────────────┘
```

## When to Use

- **Quality-critical generation**: Code, reports, analysis requiring high accuracy
- **Tasks with clear evaluation criteria**: Defined success metrics exist
- **Content requiring specific standards**: Style guides, compliance, formatting

---

## Pattern 1: Basic Reflection

Agent evaluates and improves its own output through self-critique.

```csharp
public async Task<string> ReflectAndRefine(
    string task, 
    List<string> criteria, 
    int maxIterations = 3)
{
    var output = await GenerateOutput(task);
    
    for (int i = 0; i < maxIterations; i++)
    {
        // Self-critique
        var critique = await EvaluateOutput(output, criteria);
        
        if (critique.AllPass)
            return output;
        
        // Refine based on critique
        output = await ImproveOutput(output, critique.FailedCriteria);
    }
    
    return output;
}
```

**Key insight**: Use structured output for reliable parsing of critique results.

---

## Pattern 2: Evaluator-Optimizer

Separate generation and evaluation into distinct components for clearer responsibilities.

```csharp
public class EvaluatorOptimizer
{
    private readonly double _scoreThreshold;

    public EvaluatorOptimizer(double scoreThreshold = 0.8)
    {
        _scoreThreshold = scoreThreshold;
    }

    public async Task<string> Generate(string task)
    {
        // Generate initial output
        return await LLM($"Complete: {task}");
    }

    public async Task<EvaluationResult> Evaluate(string output, string task)
    {
        var result = await LLM($@"
            Evaluate output for task: {task}
            Output: {output}
            Return JSON: {{
                ""overall_score"": 0-1, 
                ""dimensions"": {{
                    ""accuracy"": ...,
                    ""clarity"": ...
                }}
            }}");
        return JsonSerializer.Deserialize<EvaluationResult>(result);
    }

    public async Task<string> Optimize(string output, EvaluationResult feedback)
    {
        return await LLM($"Improve based on feedback: {feedback}\nOutput: {output}");
    }

    public async Task<string> Run(string task, int maxIterations = 3)
    {
        var output = await Generate(task);
        
        for (int i = 0; i < maxIterations; i++)
        {
            var evaluation = await Evaluate(output, task);
            if (evaluation.OverallScore >= _scoreThreshold)
                break;
            output = await Optimize(output, evaluation);
        }
        
        return output;
    }
}
```

---

## Pattern 3: Code-Specific Reflection

Test-driven refinement loop for code generation.

```csharp
public class CodeReflector
{
    public async Task<string> ReflectAndFix(string specification, int maxIterations = 3)
    {
        var code = await GenerateCode(specification);
        var tests = await GenerateTests(specification, code);
        
        for (int i = 0; i < maxIterations; i++)
        {
            var result = await RunTests(code, tests);
            if (result.Success)
                return code;
                
            code = await FixCode(code, result.Error);
        }
        
        return code;
    }

    private async Task<string> GenerateCode(string spec)
    {
        return await LLM($"Write C# code for: {spec}");
    }

    private async Task<string> GenerateTests(string spec, string code)
    {
        return await LLM($"Generate xUnit tests for: {spec}\nCode: {code}");
    }

    private async Task<TestResult> RunTests(string code, string tests)
    {
        // Compile and run tests
        return await TestRunner.Execute(code, tests);
    }

    private async Task<string> FixCode(string code, string error)
    {
        return await LLM($"Fix error: {error}\nCode: {code}");
    }
}
```

---

## Evaluation Strategies

### Outcome-Based
Evaluate whether output achieves the expected result.

```csharp
public async Task<bool> EvaluateOutcome(string task, string output, string expected)
{
    var evaluation = await LLM($@"
        Does output achieve expected outcome?
        Task: {task}
        Expected: {expected}
        Output: {output}
        Answer: YES or NO");
    
    return evaluation.Trim().Equals("YES", StringComparison.OrdinalIgnoreCase);
}
```

### LLM-as-Judge
Use LLM to compare and rank outputs.

```csharp
public async Task<string> LLMJudge(string outputA, string outputB, string criteria)
{
    return await LLM($@"
        Compare outputs A and B for {criteria}.
        Which is better and why?
        
        Output A: {outputA}
        Output B: {outputB}");
}
```

### Rubric-Based
Score outputs against weighted dimensions.

```csharp
public class RubricEvaluator
{
    private readonly Dictionary<string, double> _rubric = new()
    {
        ["accuracy"] = 0.4,
        ["clarity"] = 0.3,
        ["completeness"] = 0.3
    };

    public async Task<double> EvaluateWithRubric(string output)
    {
        var dimensions = string.Join(", ", _rubric.Keys);
        var scoresJson = await LLM($@"
            Rate 1-5 for each dimension: {dimensions}
            Output: {output}
            Return as JSON: {{""accuracy"": 4, ""clarity"": 5, ...}}");
        
        var scores = JsonSerializer.Deserialize<Dictionary<string, int>>(scoresJson);
        
        return _rubric.Sum(kvp => scores[kvp.Key] * kvp.Value) / 5.0;
    }
}
```

---

## Best Practices

| Practice | Rationale |
|----------|-----------|
| **Clear criteria** | Define specific, measurable evaluation criteria upfront |
| **Iteration limits** | Set max iterations (3-5) to prevent infinite loops |
| **Convergence check** | Stop if output score isn't improving between iterations |
| **Log history** | Keep full trajectory for debugging and analysis |
| **Structured output** | Use JSON for reliable parsing of evaluation results |

---

## Quick Start Checklist

```markdown
## Evaluation Implementation Checklist

### Setup
- [ ] Define evaluation criteria/rubric
- [ ] Set score threshold for "good enough"
- [ ] Configure max iterations (default: 3)

### Implementation
- [ ] Implement Generate() function
- [ ] Implement Evaluate() function with structured output
- [ ] Implement Optimize() function
- [ ] Wire up the refinement loop

### Safety
- [ ] Add convergence detection
- [ ] Log all iterations for debugging
- [ ] Handle evaluation parse failures gracefully
```
