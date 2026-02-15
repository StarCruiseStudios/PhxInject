// -----------------------------------------------------------------------------
// <copyright file="DiagnosticsRecorderTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for <see cref="DiagnosticsRecorder"/> and its Capture pattern.
/// </summary>
public class DiagnosticsRecorderTests : LoggingTestClass {
    
    [Test]
    public void Capture_SuccessPath_ReturnsOkResult() {
        var expectedValue = Given("An expected value", () => 42);
        
        var result = When("Capturing with successful function", 
            () => DiagnosticsRecorder.Capture<int>(recorder => expectedValue));
        
        Then("Result is ok", () => Verify.That(result.IsOk.IsTrue()));
        
        var recorder = Given("A test recorder", () => new TestDiagnosticsRecorder());
        var value = When("Getting value", () => result.GetValue(recorder));
        
        Then("Value matches expected", () => Verify.That(value.IsEqualTo(expectedValue)));
    }
    
    [Test]
    public void Capture_SuccessWithWarnings_ReturnsOkResultWithDiagnostics() {
        var expectedValue = Given("An expected value", () => "test");
        var warningMessage = Given("A warning message", () => "Warning message");
        
        var result = When("Capturing with warnings", 
            () => DiagnosticsRecorder.Capture<string>(recorder => {
                recorder.Add(new DiagnosticInfo(DiagnosticType.DebugMessage, warningMessage, null));
                return expectedValue;
            }));
        
        Then("Result is ok", () => Verify.That(result.IsOk.IsTrue()));
        Then("Result contains diagnostic", () => Verify.That(result.DiagnosticInfo.Count.IsEqualTo(1)));
        
        var testRecorder = Given("A test recorder", () => new TestDiagnosticsRecorder());
        var value = When("Getting value", () => result.GetValue(testRecorder));
        
        Then("Value matches expected", () => Verify.That(value.IsEqualTo(expectedValue)));
    }
    
    [Test]
    public void Capture_SuccessWithMultipleDiagnostics_AccumulatesAllDiagnostics() {
        var result = When("Capturing with multiple diagnostics", 
            () => DiagnosticsRecorder.Capture<int>(recorder => {
                recorder.Add(new DiagnosticInfo(DiagnosticType.DebugMessage, "Info 1", null));
                recorder.Add(new DiagnosticInfo(DiagnosticType.DebugMessage, "Info 2", null));
                recorder.Add(new DiagnosticInfo(DiagnosticType.DebugMessage, "Info 3", null));
                return 100;
            }));
        
        Then("Result is ok", () => Verify.That(result.IsOk.IsTrue()));
        Then("Result contains all diagnostics", () => Verify.That(result.DiagnosticInfo.Count.IsEqualTo(3)));
    }
    
    [Test]
    public void Capture_GeneratorException_ConvertsToError() {
        var errorMessage = Given("An error message", () => "Generator error");
        var diagnosticInfo = Given("A diagnostic", 
            () => new DiagnosticInfo(DiagnosticType.InternalError, errorMessage, null));
        
        var result = When("Capturing with GeneratorException", 
            () => DiagnosticsRecorder.Capture<int>(recorder => {
                throw new GeneratorException(diagnosticInfo);
            }));
        
        Then("Result is error", () => Verify.That(result.IsOk.IsFalse()));
        Then("Result contains diagnostic", () => Verify.That(result.DiagnosticInfo.Count.IsEqualTo(1)));
        Then("Diagnostic message matches", () => 
            Verify.That(result.DiagnosticInfo[0].Message.IsEqualTo(errorMessage)));
    }
    
    [Test]
    public void Capture_GeneratorExceptionWithMultipleDiagnostics_PreservesAllDiagnostics() {
        var diagnostics = Given("Multiple diagnostics", () => new EquatableList<DiagnosticInfo>(new[] {
            new DiagnosticInfo(DiagnosticType.InternalError, "Error 1", null),
            new DiagnosticInfo(DiagnosticType.InternalError, "Error 2", null)
        }));
        
        var result = When("Capturing with GeneratorException with multiple diagnostics", 
            () => DiagnosticsRecorder.Capture<string>(recorder => {
                throw new GeneratorException(diagnostics);
            }));
        
        Then("Result is error", () => Verify.That(result.IsOk.IsFalse()));
        Then("Result contains all diagnostics", () => Verify.That(result.DiagnosticInfo.Count.IsEqualTo(2)));
    }
    
    [Test]
    public void Capture_UnexpectedException_ConvertsToInternalError() {
        var exceptionMessage = Given("An exception message", () => "Unexpected error");
        
        var result = When("Capturing with unexpected exception", 
            () => DiagnosticsRecorder.Capture<int>(recorder => {
                throw new InvalidOperationException(exceptionMessage);
            }));
        
        Then("Result is error", () => Verify.That(result.IsOk.IsFalse()));
        Then("Result contains diagnostic", () => Verify.That(result.DiagnosticInfo.Count.IsEqualTo(1)));
        Then("Diagnostic is InternalError type", () => 
            Verify.That((result.DiagnosticInfo[0].Type.Id == DiagnosticType.InternalError.Id).IsTrue()));
        Then("Diagnostic message includes exception type", () => 
            Verify.That(result.DiagnosticInfo[0].Message.Contains("InvalidOperationException").IsTrue()));
        Then("Diagnostic message includes exception message", () => 
            Verify.That(result.DiagnosticInfo[0].Message.Contains(exceptionMessage).IsTrue()));
    }
    
    [Test]
    public void Capture_WithDiagnosticsBeforeException_PreservesDiagnostics() {
        var warningMessage = Given("A warning message", () => "Warning before error");
        
        var result = When("Capturing with diagnostics before exception", 
            () => DiagnosticsRecorder.Capture<int>(recorder => {
                recorder.Add(new DiagnosticInfo(DiagnosticType.DebugMessage, warningMessage, null));
                throw new InvalidOperationException("Unexpected");
            }));
        
        Then("Result is error", () => Verify.That(result.IsOk.IsFalse()));
        Then("Result contains both diagnostics", () => Verify.That(result.DiagnosticInfo.Count.IsEqualTo(2)));
    }
    
    [Test]
    public void Add_SingleDiagnostic_AddsToDiagnosticsList() {
        var diagnostic = Given("A diagnostic", 
            () => new DiagnosticInfo(DiagnosticType.DebugMessage, "Test", null));
        
        var result = When("Adding diagnostic via Capture", () => 
            DiagnosticsRecorder.Capture<int>(recorder => {
                recorder.Add(diagnostic);
                return 42;
            }));
        
        Then("Result contains diagnostic", () => 
            Verify.That(result.DiagnosticInfo.Count.IsEqualTo(1)));
        Then("Diagnostic message matches", () => 
            Verify.That(result.DiagnosticInfo[0].Message.IsEqualTo("Test")));
    }
    
    [Test]
    public void Add_MultipleDiagnostics_AddsAllToDiagnosticsList() {
        var diagnostics = Given("Multiple diagnostics", () => new[] {
            new DiagnosticInfo(DiagnosticType.DebugMessage, "Test 1", null),
            new DiagnosticInfo(DiagnosticType.DebugMessage, "Test 2", null),
            new DiagnosticInfo(DiagnosticType.DebugMessage, "Test 3", null)
        });
        
        var result = When("Adding multiple diagnostics via Capture", () => 
            DiagnosticsRecorder.Capture<int>(recorder => {
                recorder.Add(diagnostics);
                return 42;
            }));
        
        Then("All diagnostics are added", () => 
            Verify.That(result.DiagnosticInfo.Count.IsEqualTo(3)));
    }
    
    [Test]
    public void Capture_ThreadSafety_IsolatesRecorderPerInvocation() {
        var result1 = When("First capture", 
            () => DiagnosticsRecorder.Capture<int>(recorder => {
                recorder.Add(new DiagnosticInfo(DiagnosticType.DebugMessage, "First", null));
                return 1;
            }));
        
        var result2 = When("Second capture", 
            () => DiagnosticsRecorder.Capture<int>(recorder => {
                recorder.Add(new DiagnosticInfo(DiagnosticType.DebugMessage, "Second", null));
                recorder.Add(new DiagnosticInfo(DiagnosticType.DebugMessage, "Second again", null));
                return 2;
            }));
        
        Then("First result has one diagnostic", () => 
            Verify.That(result1.DiagnosticInfo.Count.IsEqualTo(1)));
        Then("Second result has two diagnostics", () => 
            Verify.That(result2.DiagnosticInfo.Count.IsEqualTo(2)));
        Then("Results are isolated", () => 
            Verify.That(result1.DiagnosticInfo[0].Message.IsNotEqualTo(result2.DiagnosticInfo[0].Message)));
    }
}
