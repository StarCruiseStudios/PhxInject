// -----------------------------------------------------------------------------
// <copyright file="LocationInfoTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using Phx.Inject.Generator.Incremental.Util;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests.Util;

/// <summary>
/// Tests for <see cref="LocationInfo"/> record.
/// </summary>
public class LocationInfoTests : LoggingTestClass {
    
    [Test]
    public void Constructor_WithFilePathTextSpanLineSpan_CreatesInstance() {
        var filePath = Given("A file path", () => "TestFile.cs");
        var textSpan = Given("A text span", () => new TextSpan(10, 20));
        var lineSpan = Given("A line span", () => new LinePositionSpan(
            new LinePosition(1, 10),
            new LinePosition(1, 30)
        ));
        
        var locationInfo = When("Creating location info", 
            () => new LocationInfo(filePath, textSpan, lineSpan));
        
        Then("FilePath is set", () => Verify.That(locationInfo.FilePath.IsEqualTo(filePath)));
        Then("TextSpan is set", () => Verify.That((locationInfo.TextSpan == textSpan).IsTrue()));
        Then("LineSpan is set", () => Verify.That((locationInfo.LineSpan == lineSpan).IsTrue()));
    }
    
    [Test]
    public void ToLocation_ConvertsToRoslynLocation() {
        var filePath = Given("A file path", () => "Source.cs");
        var textSpan = Given("A text span", () => new TextSpan(5, 15));
        var lineSpan = Given("A line span", () => new LinePositionSpan(
            new LinePosition(2, 5),
            new LinePosition(2, 20)
        ));
        var locationInfo = Given("A location info", 
            () => new LocationInfo(filePath, textSpan, lineSpan));
        
        var location = When("Converting to Location", () => locationInfo.ToLocation());
        
        Then("Location is not null", () => Verify.That(location.IsNotNull()));
        Then("Location has correct file path", () => 
            Verify.That(location.GetLineSpan().Path.IsEqualTo(filePath)));
        Then("Location has correct source span", () => 
            Verify.That((location.SourceSpan == textSpan).IsTrue()));
        Then("Location has correct line span", () => 
            Verify.That((location.GetLineSpan().Span == lineSpan).IsTrue()));
    }
    
    [Test]
    public void CreateFrom_SyntaxNode_ExtractsLocationInfo() {
        var sourceCode = Given("Source code", () => "class TestClass { }");
        var syntaxTree = Given("A syntax tree", () => CSharpSyntaxTree.ParseText(sourceCode));
        var root = Given("Syntax root", () => syntaxTree.GetRoot());
        var classNode = Given("Class declaration node", 
            () => root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>().First());
        
        var locationInfo = When("Creating from syntax node", 
            () => LocationInfo.CreateFrom(classNode));
        
        Then("LocationInfo is not null", () => Verify.That(locationInfo.IsNotNull()));
        Then("FilePath is empty for in-memory tree", () => 
            Verify.That(locationInfo!.FilePath.IsEqualTo("")));
        Then("TextSpan matches node span", () => 
            Verify.That((locationInfo!.TextSpan == classNode.Span).IsTrue()));
    }
    
    [Test]
    public void CreateFrom_LocationWithSourceTree_ExtractsLocationInfo() {
        var filePath = Given("A file path", () => "MyFile.cs");
        var sourceCode = Given("Source code", () => "public class MyClass { }");
        var syntaxTree = Given("A syntax tree with path", 
            () => CSharpSyntaxTree.ParseText(sourceCode, path: filePath));
        var location = Given("A Roslyn location", () => Location.Create(syntaxTree, new TextSpan(0, 10)));
        
        var locationInfo = When("Creating from location", 
            () => LocationInfo.CreateFrom(location));
        
        Then("LocationInfo is not null", () => Verify.That(locationInfo.IsNotNull()));
        Then("FilePath matches", () => Verify.That(locationInfo!.FilePath.IsEqualTo(filePath)));
        Then("TextSpan matches", () => Verify.That((locationInfo!.TextSpan == location.SourceSpan).IsTrue()));
    }
    
    [Test]
    public void CreateFrom_LocationWithoutSourceTree_ReturnsNull() {
        var location = Given("A location without source tree", () => Location.None);
        
        var locationInfo = When("Creating from location without tree", 
            () => LocationInfo.CreateFrom(location));
        
        Then("LocationInfo is null", () => Verify.That(locationInfo.IsNull()));
    }
    
    [Test]
    public void Equality_SameValues_AreEqual() {
        var filePath = Given("A file path", () => "Test.cs");
        var textSpan = Given("A text span", () => new TextSpan(0, 10));
        var lineSpan = Given("A line span", () => new LinePositionSpan(
            new LinePosition(0, 0),
            new LinePosition(0, 10)
        ));
        
        var location1 = Given("First location info", 
            () => new LocationInfo(filePath, textSpan, lineSpan));
        var location2 = Given("Second location info with same values", 
            () => new LocationInfo(filePath, textSpan, lineSpan));
        
        var areEqual = When("Comparing for equality", () => location1.Equals(location2));
        
        Then("Locations are equal", () => Verify.That(areEqual.IsTrue()));
    }
    
    [Test]
    public void Equality_DifferentFilePath_AreNotEqual() {
        var textSpan = Given("A text span", () => new TextSpan(0, 10));
        var lineSpan = Given("A line span", () => new LinePositionSpan(
            new LinePosition(0, 0),
            new LinePosition(0, 10)
        ));
        
        var location1 = Given("First location info", 
            () => new LocationInfo("File1.cs", textSpan, lineSpan));
        var location2 = Given("Second location info with different path", 
            () => new LocationInfo("File2.cs", textSpan, lineSpan));
        
        var areEqual = When("Comparing for equality", () => location1.Equals(location2));
        
        Then("Locations are not equal", () => Verify.That(areEqual.IsFalse()));
    }
    
    [Test]
    public void GetHashCode_SameValues_ReturnsSameHash() {
        var filePath = Given("A file path", () => "Test.cs");
        var textSpan = Given("A text span", () => new TextSpan(5, 10));
        var lineSpan = Given("A line span", () => new LinePositionSpan(
            new LinePosition(1, 5),
            new LinePosition(1, 15)
        ));
        
        var location1 = Given("First location info", 
            () => new LocationInfo(filePath, textSpan, lineSpan));
        var location2 = Given("Second location info with same values", 
            () => new LocationInfo(filePath, textSpan, lineSpan));
        
        var hash1 = When("Getting hash of first location", () => location1.GetHashCode());
        var hash2 = When("Getting hash of second location", () => location2.GetHashCode());
        
        Then("Hashes are equal", () => Verify.That(hash1.IsEqualTo(hash2)));
    }
}
