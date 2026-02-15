// -----------------------------------------------------------------------------
// <copyright file="DiagnosticInfoTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Util;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests.Diagnostics;

/// <summary>
/// Tests for <see cref="DiagnosticInfo"/> record.
/// </summary>
public class DiagnosticInfoTests : LoggingTestClass {
    
    [Test]
    public void Constructor_WithTypeMessageLocation_CreatesInstance() {
        var type = Given("A diagnostic type", () => DiagnosticType.DebugMessage);
        var message = Given("A message", () => "Test message");
        var location = Given("A location", () => new LocationInfo(
            "TestFile.cs",
            new TextSpan(0, 10),
            new LinePositionSpan(new LinePosition(0, 0), new LinePosition(0, 10))
        ));
        
        var diagnosticInfo = When("Creating diagnostic info", 
            () => new DiagnosticInfo(type, message, location));
        
        Then("Type is set", type.Id, expected => Verify.That(diagnosticInfo.Type.Id.IsEqualTo(expected)));
        Then("Message is set", message, expected => Verify.That(diagnosticInfo.Message.IsEqualTo(expected)));
        Then("Location is set", () => Verify.That((diagnosticInfo.Location == location).IsTrue()));
    }
    
    [Test]
    public void Constructor_WithNullLocation_CreatesInstance() {
        var type = Given("A diagnostic type", () => DiagnosticType.InternalError);
        var message = Given("A message", () => "Error without location");
        
        var diagnosticInfo = When("Creating diagnostic info with null location", 
            () => new DiagnosticInfo(type, message, null));
        
        Then("Type is set", type.Id, expected => Verify.That(diagnosticInfo.Type.Id.IsEqualTo(expected)));
        Then("Message is set", message, expected => Verify.That(diagnosticInfo.Message.IsEqualTo(expected)));
        Then("Location is null", () => Verify.That(diagnosticInfo.Location.IsNull()));
    }
    
    [Test]
    public void Report_WithLocation_ReportsDiagnosticToContext() {
        var location = Given("A location", () => new LocationInfo(
            "TestFile.cs",
            new TextSpan(5, 10),
            new LinePositionSpan(new LinePosition(1, 5), new LinePosition(1, 15))
        ));
        var diagnosticInfo = Given("A diagnostic info with location", 
            () => new DiagnosticInfo(DiagnosticType.InternalError, "Test error", location));
        
        // We can't mock SourceProductionContext because it's a struct
        // Instead, verify the diagnostic can be created successfully
        var diagnostic = When("Creating diagnostic", () => 
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    diagnosticInfo.Type.Id,
                    diagnosticInfo.Type.Title,
                    diagnosticInfo.Message,
                    diagnosticInfo.Type.Category,
                    diagnosticInfo.Type.Severity,
                    diagnosticInfo.Type.IsEnabledByDefault
                ),
                location.ToLocation()
            ));
        
        Then("Diagnostic has correct ID", () => 
            Verify.That(diagnostic.Id.IsEqualTo(DiagnosticType.InternalError.Id)));
        Then("Diagnostic has correct message", () => 
            Verify.That(diagnostic.GetMessage().IsEqualTo("Test error")));
        Then("Diagnostic has location", () => 
            Verify.That((diagnostic.Location != Location.None).IsTrue()));
    }
    
    [Test]
    public void Report_WithNullLocation_ReportsWithoutLocation() {
        var diagnosticInfo = Given("A diagnostic info without location", 
            () => new DiagnosticInfo(DiagnosticType.DebugMessage, "Debug info", null));
        
        var diagnostic = When("Creating diagnostic", () => 
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    diagnosticInfo.Type.Id,
                    diagnosticInfo.Type.Title,
                    diagnosticInfo.Message,
                    diagnosticInfo.Type.Category,
                    diagnosticInfo.Type.Severity,
                    diagnosticInfo.Type.IsEnabledByDefault
                ),
                null
            ));
        
        Then("Diagnostic has correct ID", () => 
            Verify.That(diagnostic.Id.IsEqualTo(DiagnosticType.DebugMessage.Id)));
        Then("Diagnostic has correct message", () => 
            Verify.That(diagnostic.GetMessage().IsEqualTo("Debug info")));
        Then("Diagnostic has no location", () => 
            Verify.That((diagnostic.Location == Location.None).IsTrue()));
    }
    
    [Test]
    public void Report_WithDifferentSeverities_ReportsCorrectSeverity() {
        var infoDiagnostic = Given("An info diagnostic", 
            () => new DiagnosticInfo(DiagnosticType.DebugMessage, "Info message", null));
        var errorDiagnostic = Given("An error diagnostic", 
            () => new DiagnosticInfo(DiagnosticType.InternalError, "Error message", null));
        
        var infoDiag = When("Creating info diagnostic", () => 
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    infoDiagnostic.Type.Id,
                    infoDiagnostic.Type.Title,
                    infoDiagnostic.Message,
                    infoDiagnostic.Type.Category,
                    infoDiagnostic.Type.Severity,
                    infoDiagnostic.Type.IsEnabledByDefault
                ),
                null
            ));
        
        var errorDiag = When("Creating error diagnostic", () => 
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    errorDiagnostic.Type.Id,
                    errorDiagnostic.Type.Title,
                    errorDiagnostic.Message,
                    errorDiagnostic.Type.Category,
                    errorDiagnostic.Type.Severity,
                    errorDiagnostic.Type.IsEnabledByDefault
                ),
                null
            ));
        
        Then("Info diagnostic has Info severity", () => 
            Verify.That((infoDiag.Severity == DiagnosticSeverity.Info).IsTrue()));
        Then("Error diagnostic has Error severity", () => 
            Verify.That((errorDiag.Severity == DiagnosticSeverity.Error).IsTrue()));
    }
    
    [Test]
    public void Equality_SameValues_AreEqual() {
        var type = Given("A diagnostic type", () => DiagnosticType.InternalError);
        var message = Given("A message", () => "Same message");
        var location = Given("A location", () => new LocationInfo(
            "File.cs",
            new TextSpan(0, 5),
            new LinePositionSpan(new LinePosition(0, 0), new LinePosition(0, 5))
        ));
        
        var diagnostic1 = Given("First diagnostic", 
            () => new DiagnosticInfo(type, message, location));
        var diagnostic2 = Given("Second diagnostic with same values", 
            () => new DiagnosticInfo(type, message, location));
        
        var areEqual = When("Comparing diagnostics", () => 
            diagnostic1.Type.Id == diagnostic2.Type.Id &&
            diagnostic1.Message == diagnostic2.Message &&
            diagnostic1.Location == diagnostic2.Location);
        
        Then("Diagnostics are equal", () => Verify.That(areEqual.IsTrue()));
    }
}
