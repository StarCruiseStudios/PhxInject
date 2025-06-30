// -----------------------------------------------------------------------------
// <copyright file="MultiBindTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using NUnit.Framework;
using Phx.Inject.Tests.Data.Model;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests;

#region injector

[Specification]
public static class MultiBindSpecification {
    [Factory]
    [Partial]
    internal static List<ILeaf> GetListLeaf1() {
        return new List<ILeaf> {
            new IntLeaf(10)
        };
    }

    [Factory]
    [Partial]
    internal static List<ILeaf> GetListLeaf2() {
        return new List<ILeaf> {
            new IntLeaf(20)
        };
    }

    [Factory]
    [Partial]
    internal static HashSet<ILeaf> GetSetLeaf1() {
        return new HashSet<ILeaf> {
            new IntLeaf(30)
        };
    }

    [Factory]
    [Partial]
    internal static HashSet<ILeaf> GetSetLeaf2() {
        return new HashSet<ILeaf> {
            new IntLeaf(40)
        };
    }

    [Factory]
    [Partial]
    internal static Dictionary<string, ILeaf> GetDictLeaf1() {
        return new Dictionary<string, ILeaf> {
            {
                "key1", new IntLeaf(50)
            }
        };
    }

    [Factory]
    [Partial]
    internal static Dictionary<string, ILeaf> GetDictLeaf2() {
        return new Dictionary<string, ILeaf> {
            {
                "key2", new IntLeaf(60)
            }
        };
    }
}

[Injector(typeof(MultiBindSpecification))]
public interface IMultiBindInjector {
    List<ILeaf> GetLeafList();
    HashSet<ILeaf> GetLeafSet();
    Dictionary<string, ILeaf> GetLeafDict();

    Factory<List<ILeaf>> GetLeafListRuntimeFactory();
    Factory<HashSet<ILeaf>> GetLeafSetRuntimeFactory();
    Factory<Dictionary<string, ILeaf>> GetLeafDictRuntimeFactory();
}

#endregion injector

public class MultiBindTests : LoggingTestClass {
    [Test]
    public void MultiBindListIsInjected() {
        IMultiBindInjector injector = Given("A test injector.", () => new GeneratedMultiBindInjector());

        IReadOnlyList<ILeaf> list = When("Getting a multibind list", () => injector.GetLeafList());

        Then("The list contains the expected values",
            () => {
                Verify.That(list.Count.IsEqualTo(2));
                Verify.That(list[0].IsType<IntLeaf>());
                Verify.That((list[0] as IntLeaf)!.Value.IsEqualTo(10));
                Verify.That(list[1].IsType<IntLeaf>());
                Verify.That((list[1] as IntLeaf)!.Value.IsEqualTo(20));
            });
    }

    [Test]
    public void MultiBindSetIsInjected() {
        IMultiBindInjector injector = Given("A test injector.", () => new GeneratedMultiBindInjector());

        IReadOnlySet<ILeaf> set = When("Getting a multibind set", () => injector.GetLeafSet());

        Then("The list contains the expected values",
            () => {
                Verify.That(set.Count.IsEqualTo(2));
                Verify.That(set.Any(leaf => (leaf as IntLeaf)?.Value == 30).IsTrue());
                Verify.That(set.Any(leaf => (leaf as IntLeaf)?.Value == 40).IsTrue());
            });
    }

    [Test]
    public void MultiBindDictIsInjected() {
        IMultiBindInjector injector = Given("A test injector.", () => new GeneratedMultiBindInjector());

        IReadOnlyDictionary<string, ILeaf> dict = When("Getting a multibind list", () => injector.GetLeafDict());

        Then("The list contains the expected values",
            () => {
                Verify.That(dict.Count.IsEqualTo(2));
                Verify.That(dict["key1"].IsType<IntLeaf>());
                Verify.That((dict["key1"] as IntLeaf)!.Value.IsEqualTo(50));
                Verify.That(dict["key2"].IsType<IntLeaf>());
                Verify.That((dict["key2"] as IntLeaf)!.Value.IsEqualTo(60));
            });
    }
    [Test]
    public void MultiBindListFactoryIsInjected() {
        IMultiBindInjector injector = Given("A test injector.", () => new GeneratedMultiBindInjector());

        Factory<List<ILeaf>> factory = When("Getting a multibind list", () => injector.GetLeafListRuntimeFactory());
        IReadOnlyList<ILeaf> list = factory.Create();

        Then("The list contains the expected values",
            () => {
                Verify.That(list.Count.IsEqualTo(2));
                Verify.That(list[0].IsType<IntLeaf>());
                Verify.That((list[0] as IntLeaf)!.Value.IsEqualTo(10));
                Verify.That(list[1].IsType<IntLeaf>());
                Verify.That((list[1] as IntLeaf)!.Value.IsEqualTo(20));
            });
    }

    [Test]
    public void MultiBindSetFactoryIsInjected() {
        IMultiBindInjector injector = Given("A test injector.", () => new GeneratedMultiBindInjector());

        Factory<HashSet<ILeaf>> factory = When("Getting a multibind set", () => injector.GetLeafSetRuntimeFactory());
        IReadOnlySet<ILeaf> set = factory.Create();

        Then("The list contains the expected values",
            () => {
                Verify.That(set.Count.IsEqualTo(2));
                Verify.That(set.Any(leaf => (leaf as IntLeaf)?.Value == 30).IsTrue());
                Verify.That(set.Any(leaf => (leaf as IntLeaf)?.Value == 40).IsTrue());
            });
    }

    [Test]
    public void MultiBindDictFactoryIsInjected() {
        IMultiBindInjector injector = Given("A test injector.", () => new GeneratedMultiBindInjector());

        Factory<Dictionary<string, ILeaf>> factory = When("Getting a multibind list",
            () => injector.GetLeafDictRuntimeFactory());
        IReadOnlyDictionary<string, ILeaf> dict = factory.Create();

        Then("The list contains the expected values",
            () => {
                Verify.That(dict.Count.IsEqualTo(2));
                Verify.That(dict["key1"].IsType<IntLeaf>());
                Verify.That((dict["key1"] as IntLeaf)!.Value.IsEqualTo(50));
                Verify.That(dict["key2"].IsType<IntLeaf>());
                Verify.That((dict["key2"] as IntLeaf)!.Value.IsEqualTo(60));
            });
    }
}
