// -----------------------------------------------------------------------------
// <copyright file="ResultTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using NUnit.Framework;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Util;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests.Diagnostics;

/// <summary>
/// Tests for <see cref="IResult{T}"/>, <see cref="OkResult{T}"/>, and <see cref="ErrorResult{T}"/>.
/// </summary>
public class ResultTests : LoggingTestClass {
    
    [Test]
    public void Ok_WithValue_CreatesOkResult() {
        var value = Given("A value", () => 42);
        
        var result = When("Creating ok result", () => Result.Ok(value));
        
        Then("Result is ok", () => Verify.That(result.IsOk.IsTrue()));
        Then("Diagnostic info is empty", () => Verify.That(result.DiagnosticInfo.Count.IsEqualTo(0)));
    }
    
    [Test]
    public void Ok_WithValueAndDiagnostics_CreatesOkResultWithDiagnostics() {
        var value = Given("A value", () => "test");
        var diagnostic = Given("A diagnostic", () => new DiagnosticInfo(
            DiagnosticType.DebugMessage,
            "Test warning",
            null
        ));
        
        var result = When("Creating ok result with diagnostics", 
            () => Result.Ok(value, diagnostic));
        
        Then("Result is ok", () => Verify.That(result.IsOk.IsTrue()));
        Then("Result contains diagnostic", () => Verify.That(result.DiagnosticInfo.Count.IsEqualTo(1)));
    }
    
    [Test]
    public void Error_WithDiagnostic_CreatesErrorResult() {
        var diagnostic = Given("An error diagnostic", () => new DiagnosticInfo(
            DiagnosticType.InternalError,
            "Test error",
            null
        ));
        
        var result = When("Creating error result", () => Result.Error<int>(diagnostic));
        
        Then("Result is not ok", () => Verify.That(result.IsOk.IsFalse()));
        Then("Result contains diagnostic", () => Verify.That(result.DiagnosticInfo.Count.IsEqualTo(1)));
    }
    
    [Test]
    public void GetValue_OnOkResult_ReturnsValue() {
        var result = Given("An ok result", () => Result.Ok(42));
        var recorder = Given("A diagnostics recorder", () => new TestDiagnosticsRecorder());
        
        var value = When("Getting value", () => result.GetValue(recorder));
        
        Then("Value is correct", () => Verify.That(value.IsEqualTo(42)));
    }
    
    [Test]
    public void GetValue_OnErrorResult_ThrowsException() {
        var result = Given("An error result", () => Result.Error<int>(
            new DiagnosticInfo(DiagnosticType.InternalError, "Error", null)));
        var recorder = Given("A diagnostics recorder", () => new TestDiagnosticsRecorder());
        
        InvalidOperationException? thrownException = null;
        
        When("Getting value from error", () => {
            try {
                result.GetValue(recorder);
            } catch (InvalidOperationException ex) {
                thrownException = ex;
            }
        });
        
        Then("Exception is thrown", () => Verify.That(thrownException.IsNotNull()));
        Then("Exception has appropriate message", () => 
            Verify.That(thrownException!.Message.Length.IsGreaterThan(0)));
    }
    
    [Test]
    public void Map_OnOkResult_AppliesTransformation() {
        var result = Given("An ok result", () => Result.Ok(10));
        
        var mapped = When("Mapping result", 
            () => result.Map(x => Result.Ok(x * 2)));
        
        Then("Mapped result is ok", () => Verify.That(mapped.IsOk.IsTrue()));
        
        var recorder = Given("A diagnostics recorder", () => new TestDiagnosticsRecorder());
        var value = When("Getting mapped value", () => mapped.GetValue(recorder));
        
        Then("Value is transformed", () => Verify.That(value.IsEqualTo(20)));
    }
    
    [Test]
    public void Map_OnErrorResult_PreservesError() {
        var diagnostic = Given("An error diagnostic", () => new DiagnosticInfo(
            DiagnosticType.InternalError, "Error", null));
        var result = Given("An error result", () => Result.Error<int>(diagnostic));
        
        var mapped = When("Mapping error result", 
            () => result.Map(x => Result.Ok(x * 2)));
        
        Then("Mapped result is error", () => Verify.That(mapped.IsOk.IsFalse()));
        Then("Diagnostic is preserved", () => Verify.That(mapped.DiagnosticInfo.Count.IsEqualTo(1)));
    }
    
    [Test]
    public void MapError_OnErrorResult_ChangesType() {
        var diagnostic = Given("An error diagnostic", () => new DiagnosticInfo(
            DiagnosticType.InternalError, "Error", null));
        var result = Given("An error result of int", () => Result.Error<int>(diagnostic));
        
        var mapped = When("Mapping to string type", () => result.MapError<string>());
        
        Then("Mapped result is error", () => Verify.That(mapped.IsOk.IsFalse()));
        Then("Diagnostic is preserved", () => Verify.That(mapped.DiagnosticInfo.Count.IsEqualTo(1)));
    }
    
    [Test]
    public void MapError_OnOkResult_ThrowsException() {
        var result = Given("An ok result", () => Result.Ok(42));
        
        InvalidOperationException? thrownException = null;
        
        When("Mapping error on ok result", () => {
            try {
                result.MapError<string>();
            } catch (InvalidOperationException ex) {
                thrownException = ex;
            }
        });
        
        Then("Exception is thrown", () => Verify.That(thrownException.IsNotNull()));
        Then("Exception has appropriate message", () => 
            Verify.That(thrownException!.Message.Length.IsGreaterThan(0)));
    }
    
    [Test]
    public void OrNull_OnOkResult_ReturnsValue() {
        var result = Given("An ok result", () => Result.Ok("test"));
        var recorder = Given("A diagnostics recorder", () => new TestDiagnosticsRecorder());
        
        var value = When("Getting value or null", () => result.OrNull(recorder));
        
        Then("Value is returned", () => Verify.That(value.IsNotNull()));
        Then("Value is correct", () => Verify.That(value.IsEqualTo("test")));
    }
    
    [Test]
    public void OrNull_OnErrorResult_ReturnsNull() {
        var result = Given("An error result", () => Result.Error<string>(
            new DiagnosticInfo(DiagnosticType.InternalError, "Error", null)));
        var recorder = Given("A diagnostics recorder", () => new TestDiagnosticsRecorder());
        
        var value = When("Getting value or null", () => result.OrNull(recorder));
        
        Then("Null is returned", () => Verify.That(value.IsNull()));
    }
    
    [Test]
    public void OrElse_OnErrorResult_ReturnsDefault() {
        var result = Given("An error result", () => Result.Error<int>(
            new DiagnosticInfo(DiagnosticType.InternalError, "Error", null)));
        var recorder = Given("A diagnostics recorder", () => new TestDiagnosticsRecorder());
        
        var value = When("Getting value or else", () => result.OrElse(recorder, () => 99));
        
        Then("Default value is returned", () => Verify.That(value.IsEqualTo(99)));
    }
    
    [Test]
    public void TryGetValue_OnOkResult_ReturnsTrue() {
        var result = Given("An ok result", () => Result.Ok(42));
        var recorder = Given("A diagnostics recorder", () => new TestDiagnosticsRecorder());
        
        var success = When("Trying to get value", () => result.TryGetValue(recorder, out var value));
        
        Then("Success is true", () => Verify.That(success.IsTrue()));
    }
    
    [Test]
    public void TryGetValue_OnErrorResult_ReturnsFalse() {
        var result = Given("An error result", () => Result.Error<int>(
            new DiagnosticInfo(DiagnosticType.InternalError, "Error", null)));
        var recorder = Given("A diagnostics recorder", () => new TestDiagnosticsRecorder());
        
        var success = When("Trying to get value", () => result.TryGetValue(recorder, out var value));
        
        Then("Success is false", () => Verify.That(success.IsFalse()));
    }
}

/// <summary>
/// Test implementation of IDiagnosticsRecorder.
/// </summary>
internal class TestDiagnosticsRecorder : IDiagnosticsRecorder {
    private readonly List<DiagnosticInfo> diagnostics = new();
    
    public void Add(DiagnosticInfo diagnostic) => diagnostics.Add(diagnostic);
    public void Add(IEnumerable<DiagnosticInfo> diagnostics) => this.diagnostics.AddRange(diagnostics);
    public EquatableList<DiagnosticInfo> GetDiagnostics() => diagnostics.ToEquatableList();
}
