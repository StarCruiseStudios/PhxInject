// -----------------------------------------------------------------------------
// <copyright file="EquatableListTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using NUnit.Framework;
using Phx.Inject.Generator.Incremental.Util;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests.Util;

/// <summary>
/// Tests for <see cref="EquatableList{T}"/> structural equality and immutability.
/// </summary>
public class EquatableListTests : LoggingTestClass {
    
    [Test]
    public void Create_WithItems_ReturnsListWithItems() {
        var items = Given("A list of integers", () => new[] { 1, 2, 3 });
        
        var list = When("Creating an equatable list", () => EquatableList<int>.Create(items));
        
        Then("List contains all items", () => Verify.That(list.Count.IsEqualTo(3)));
        Then("First item is correct", () => Verify.That(list[0].IsEqualTo(1)));
        Then("Second item is correct", () => Verify.That(list[1].IsEqualTo(2)));
        Then("Third item is correct", () => Verify.That(list[2].IsEqualTo(3)));
    }
    
    [Test]
    public void Create_WithParams_ReturnsListWithItems() {
        var list = When("Creating list with params", () => EquatableList<string>.Create("a", "b", "c"));
        
        Then("List contains all items", () => Verify.That(list.Count.IsEqualTo(3)));
        Then("Items are in order", () => {
            Verify.That(list[0].IsEqualTo("a"));
            Verify.That(list[1].IsEqualTo("b"));
            Verify.That(list[2].IsEqualTo("c"));
        });
    }
    
    [Test]
    public void Empty_ReturnsEmptyList() {
        var list = When("Getting empty list", () => EquatableList<int>.Empty);
        
        Then("List is empty", () => Verify.That(list.Count.IsEqualTo(0)));
    }
    
    [Test]
    public void Equals_SameContent_ReturnsTrue() {
        var list1 = Given("First list", () => EquatableList<int>.Create(1, 2, 3));
        var list2 = Given("Second list with same content", () => EquatableList<int>.Create(1, 2, 3));
        
        var areEqual = When("Comparing for equality", () => list1.Equals(list2));
        
        Then("Lists are equal", () => Verify.That(areEqual.IsTrue()));
    }
    
    [Test]
    public void Equals_DifferentContent_ReturnsFalse() {
        var list1 = Given("First list", () => EquatableList<int>.Create(1, 2, 3));
        var list2 = Given("Second list with different content", () => EquatableList<int>.Create(1, 2, 4));
        
        var areEqual = When("Comparing for equality", () => list1.Equals(list2));
        
        Then("Lists are not equal", () => Verify.That(areEqual.IsFalse()));
    }
    
    [Test]
    public void Equals_DifferentLength_ReturnsFalse() {
        var list1 = Given("First list", () => EquatableList<int>.Create(1, 2, 3));
        var list2 = Given("Second list with different length", () => EquatableList<int>.Create(1, 2));
        
        var areEqual = When("Comparing for equality", () => list1.Equals(list2));
        
        Then("Lists are not equal", () => Verify.That(areEqual.IsFalse()));
    }
    
    [Test]
    public void Equals_BothEmpty_ReturnsTrue() {
        var list1 = Given("First empty list", () => EquatableList<int>.Empty);
        var list2 = Given("Second empty list", () => EquatableList<int>.Empty);
        
        var areEqual = When("Comparing for equality", () => list1.Equals(list2));
        
        Then("Lists are equal", () => Verify.That(areEqual.IsTrue()));
    }
    
    [Test]
    public void Equals_SameReference_ReturnsTrue() {
        var list = Given("A list", () => EquatableList<int>.Create(1, 2, 3));
        
        var areEqual = When("Comparing to itself", () => list.Equals(list));
        
        Then("List equals itself", () => Verify.That(areEqual.IsTrue()));
    }
    
    [Test]
    public void Equals_WithNull_ReturnsFalse() {
        var list = Given("A list", () => EquatableList<int>.Create(1, 2, 3));
        
        var areEqual = When("Comparing to null", () => list.Equals(null));
        
        Then("List does not equal null", () => Verify.That(areEqual.IsFalse()));
    }
    
    [Test]
    public void GetHashCode_SameContent_ReturnsSameHash() {
        var list1 = Given("First list", () => EquatableList<int>.Create(1, 2, 3));
        var list2 = Given("Second list with same content", () => EquatableList<int>.Create(1, 2, 3));
        
        var hash1 = When("Getting hash of first list", () => list1.GetHashCode());
        var hash2 = When("Getting hash of second list", () => list2.GetHashCode());
        
        Then("Hashes are equal", () => Verify.That(hash1.IsEqualTo(hash2)));
    }
    
    [Test]
    public void GetHashCode_DifferentContent_ReturnsDifferentHash() {
        var list1 = Given("First list", () => EquatableList<int>.Create(1, 2, 3));
        var list2 = Given("Second list with different content", () => EquatableList<int>.Create(1, 2, 4));
        
        var hash1 = When("Getting hash of first list", () => list1.GetHashCode());
        var hash2 = When("Getting hash of second list", () => list2.GetHashCode());
        
        Then("Hashes are different", () => Verify.That(hash1.IsNotEqualTo(hash2)));
    }
    
    [Test]
    public void GetEnumerator_ReturnsAllItems() {
        var list = Given("A list", () => EquatableList<string>.Create("a", "b", "c"));
        
        var enumeratedItems = When("Enumerating list", () => {
            var items = new List<string>();
            foreach (var item in list) {
                items.Add(item);
            }
            return items;
        });
        
        Then("All items enumerated in order", () => {
            Verify.That(enumeratedItems.Count.IsEqualTo(3));
            Verify.That(enumeratedItems[0].IsEqualTo("a"));
            Verify.That(enumeratedItems[1].IsEqualTo("b"));
            Verify.That(enumeratedItems[2].IsEqualTo("c"));
        });
    }
    
    [Test]
    public void ToEquatableList_ConvertsEnumerable() {
        var source = Given("A source enumerable", () => new[] { 1, 2, 3 }.AsEnumerable());
        
        var list = When("Converting to equatable list", () => source.ToEquatableList());
        
        Then("Conversion succeeds", () => Verify.That(list.IsNotNull()));
        Then("List contains all items", () => Verify.That(list.Count.IsEqualTo(3)));
    }
    
    [Test]
    public void Indexer_ReturnsCorrectItem() {
        var list = Given("A list", () => EquatableList<string>.Create("first", "second", "third"));
        
        Then("First index returns first item", () => Verify.That(list[0].IsEqualTo("first")));
        Then("Second index returns second item", () => Verify.That(list[1].IsEqualTo("second")));
        Then("Third index returns third item", () => Verify.That(list[2].IsEqualTo("third")));
    }
    
    [Test]
    public void Count_ReturnsCorrectCount() {
        var emptyList = Given("An empty list", () => EquatableList<int>.Empty);
        var oneItemList = Given("A list with one item", () => EquatableList<int>.Create(42));
        var threeItemList = Given("A list with three items", () => EquatableList<int>.Create(1, 2, 3));
        
        Then("Empty list count is zero", () => Verify.That(emptyList.Count.IsEqualTo(0)));
        Then("One item list count is one", () => Verify.That(oneItemList.Count.IsEqualTo(1)));
        Then("Three item list count is three", () => Verify.That(threeItemList.Count.IsEqualTo(3)));
    }
}
