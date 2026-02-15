// -----------------------------------------------------------------------------
// <copyright file="InjectionUtilTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using NUnit.Framework;
using Phx.Inject;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests;

/// <summary>
/// Tests for <see cref="InjectionUtil"/> collection combiner methods.
/// </summary>
public class InjectionUtilTests : LoggingTestClass {
    
    #region List Combination Tests
    
    [Test]
    public void CombineLists_WithEmptyLists_ReturnsEmptyList() {
        var list1 = Given("Empty list 1", () => new List<int>().AsReadOnly());
        var list2 = Given("Empty list 2", () => new List<int>().AsReadOnly());
        
        var result = When("Combining lists", () => InjectionUtil.Combine(list1, list2));
        
        Then("Result is empty", () => Verify.That(result.Count.IsEqualTo(0)));
    }
    
    [Test]
    public void CombineLists_WithSingleList_ReturnsSameElements() {
        var list = Given("A list", () => new List<int> { 1, 2, 3 }.AsReadOnly());
        
        var result = When("Combining single list", () => InjectionUtil.Combine(list));
        
        Then("Result contains all elements", () => {
            Verify.That(result.Count.IsEqualTo(3));
            Verify.That(result[0].IsEqualTo(1));
            Verify.That(result[1].IsEqualTo(2));
            Verify.That(result[2].IsEqualTo(3));
        });
    }
    
    [Test]
    public void CombineLists_WithMultipleLists_CombinesInOrder() {
        var list1 = Given("First list", () => new List<int> { 1, 2 }.AsReadOnly());
        var list2 = Given("Second list", () => new List<int> { 3, 4 }.AsReadOnly());
        var list3 = Given("Third list", () => new List<int> { 5 }.AsReadOnly());
        
        var result = When("Combining lists", () => InjectionUtil.Combine(list1, list2, list3));
        
        Then("Result contains all elements in order", () => {
            Verify.That(result.Count.IsEqualTo(5));
            Verify.That(result[0].IsEqualTo(1));
            Verify.That(result[1].IsEqualTo(2));
            Verify.That(result[2].IsEqualTo(3));
            Verify.That(result[3].IsEqualTo(4));
            Verify.That(result[4].IsEqualTo(5));
        });
    }
    
    [Test]
    public void CombineLists_AllowsDuplicates() {
        var list1 = Given("First list", () => new List<int> { 1, 2 }.AsReadOnly());
        var list2 = Given("Second list with duplicate", () => new List<int> { 2, 3 }.AsReadOnly());
        
        var result = When("Combining lists with duplicates", () => InjectionUtil.Combine(list1, list2));
        
        Then("Result contains all elements including duplicates", () => {
            Verify.That(result.Count.IsEqualTo(4));
            Verify.That(result.Any(x => x == 2).IsTrue());
        });
    }
    
    #endregion
    
    #region Set Combination Tests
    
    [Test]
    public void CombineSets_WithEmptySets_ReturnsEmptySet() {
        var set1 = Given("Empty set 1", () => new HashSet<int>() as ISet<int>);
        var set2 = Given("Empty set 2", () => new HashSet<int>() as ISet<int>);
        
        var result = When("Combining sets", () => InjectionUtil.Combine(set1, set2));
        
        Then("Result is empty", () => Verify.That(result.Count.IsEqualTo(0)));
    }
    
    [Test]
    public void CombineSets_WithDisjointSets_CombinesSuccessfully() {
        var set1 = Given("First set", () => new HashSet<int> { 1, 2 } as ISet<int>);
        var set2 = Given("Second set", () => new HashSet<int> { 3, 4 } as ISet<int>);
        
        var result = When("Combining disjoint sets", () => InjectionUtil.Combine(set1, set2));
        
        Then("Result contains all elements", () => {
            Verify.That(result.Count.IsEqualTo(4));
            Verify.That(result.Any(x => x == 1).IsTrue());
            Verify.That(result.Any(x => x == 2).IsTrue());
            Verify.That(result.Any(x => x == 3).IsTrue());
            Verify.That(result.Any(x => x == 4).IsTrue());
        });
    }
    
    [Test]
    public void CombineSets_WithDuplicates_ThrowsException() {
        var set1 = Given("First set", () => new HashSet<int> { 1, 2 } as ISet<int>);
        var set2 = Given("Second set with duplicate", () => new HashSet<int> { 2, 3 } as ISet<int>);
        
        var exception = When("Combining sets with duplicates", () => {
            try {
                InjectionUtil.Combine(set1, set2);
                return null;
            } catch (InvalidOperationException ex) {
                return ex;
            }
        });
        
        Then("Exception is thrown", () => Verify.That(exception.IsNotNull()));
        Then("Exception mentions duplicate", 
            () => Verify.That(exception!.Message.ToLower().Contains("duplicate").IsTrue()));
    }
    
    #endregion
    
    #region ReadOnlySet Combination Tests
    
    [Test]
    public void CombineReadOnlySet_WithEmptyEnumerables_ReturnsEmptySet() {
        var enum1 = Given("Empty enumerable 1", () => new List<int>().AsEnumerable());
        var enum2 = Given("Empty enumerable 2", () => new List<int>().AsEnumerable());
        
        var result = When("Combining enumerables", 
            () => InjectionUtil.CombineReadOnlySet(enum1, enum2));
        
        Then("Result is empty", () => Verify.That(result.Count.IsEqualTo(0)));
    }
    
    [Test]
    public void CombineReadOnlySet_WithDisjointEnumerables_CombinesSuccessfully() {
        var enum1 = Given("First enumerable", () => new List<int> { 1, 2 }.AsEnumerable());
        var enum2 = Given("Second enumerable", () => new List<int> { 3, 4 }.AsEnumerable());
        
        var result = When("Combining disjoint enumerables", 
            () => InjectionUtil.CombineReadOnlySet(enum1, enum2));
        
        Then("Result contains all unique elements", () => {
            Verify.That(result.Count.IsEqualTo(4));
            Verify.That(result.Any(x => x == 1).IsTrue());
            Verify.That(result.Any(x => x == 2).IsTrue());
            Verify.That(result.Any(x => x == 3).IsTrue());
            Verify.That(result.Any(x => x == 4).IsTrue());
        });
    }
    
    [Test]
    public void CombineReadOnlySet_WithDuplicates_ThrowsException() {
        var enum1 = Given("First enumerable", () => new List<int> { 1, 2 }.AsEnumerable());
        var enum2 = Given("Second enumerable with duplicate", 
            () => new List<int> { 2, 3 }.AsEnumerable());
        
        var exception = When("Combining enumerables with duplicates", () => {
            try {
                InjectionUtil.CombineReadOnlySet(enum1, enum2);
                return null;
            } catch (InvalidOperationException ex) {
                return ex;
            }
        });
        
        Then("Exception is thrown", () => Verify.That(exception.IsNotNull()));
        Then("Exception mentions duplicate value", 
            () => Verify.That(exception!.Message.ToLower().Contains("duplicate").IsTrue()));
    }
    
    #endregion
    
    #region Dictionary Combination Tests
    
    [Test]
    public void CombineDictionaries_WithEmptyDictionaries_ReturnsEmptyDictionary() {
        var dict1 = Given("Empty dict 1", 
            () => new Dictionary<string, int>().AsReadOnly());
        var dict2 = Given("Empty dict 2", 
            () => new Dictionary<string, int>().AsReadOnly());
        
        var result = When("Combining dictionaries", 
            () => InjectionUtil.Combine(dict1, dict2));
        
        Then("Result is empty", () => Verify.That(result.Count.IsEqualTo(0)));
    }
    
    [Test]
    public void CombineDictionaries_WithDisjointKeys_CombinesSuccessfully() {
        var dict1 = Given("First dictionary", 
            () => new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 }.AsReadOnly());
        var dict2 = Given("Second dictionary", 
            () => new Dictionary<string, int> { ["c"] = 3, ["d"] = 4 }.AsReadOnly());
        
        var result = When("Combining dictionaries with disjoint keys", 
            () => InjectionUtil.Combine(dict1, dict2));
        
        Then("Result contains all key-value pairs", () => {
            Verify.That(result.Count.IsEqualTo(4));
            Verify.That(result["a"].IsEqualTo(1));
            Verify.That(result["b"].IsEqualTo(2));
            Verify.That(result["c"].IsEqualTo(3));
            Verify.That(result["d"].IsEqualTo(4));
        });
    }
    
    [Test]
    public void CombineDictionaries_WithDuplicateKeys_ThrowsException() {
        var dict1 = Given("First dictionary", 
            () => new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 }.AsReadOnly());
        var dict2 = Given("Second dictionary with duplicate key", 
            () => new Dictionary<string, int> { ["b"] = 99, ["c"] = 3 }.AsReadOnly());
        
        var exception = When("Combining dictionaries with duplicate keys", () => {
            try {
                InjectionUtil.Combine(dict1, dict2);
                return null;
            } catch (InvalidOperationException ex) {
                return ex;
            }
        });
        
        Then("Exception is thrown", () => Verify.That(exception.IsNotNull()));
        Then("Exception mentions duplicate key", 
            () => Verify.That(exception!.Message.ToLower().Contains("duplicate").IsTrue()));
    }
    
    [Test]
    public void CombineDictionaries_WithMultipleDictionaries_CombinesAll() {
        var dict1 = Given("First dictionary", 
            () => new Dictionary<string, int> { ["a"] = 1 }.AsReadOnly());
        var dict2 = Given("Second dictionary", 
            () => new Dictionary<string, int> { ["b"] = 2 }.AsReadOnly());
        var dict3 = Given("Third dictionary", 
            () => new Dictionary<string, int> { ["c"] = 3 }.AsReadOnly());
        
        var result = When("Combining three dictionaries", 
            () => InjectionUtil.Combine(dict1, dict2, dict3));
        
        Then("Result contains all entries", () => {
            Verify.That(result.Count.IsEqualTo(3));
            Verify.That(result["a"].IsEqualTo(1));
            Verify.That(result["b"].IsEqualTo(2));
            Verify.That(result["c"].IsEqualTo(3));
        });
    }
    
    #endregion
}
