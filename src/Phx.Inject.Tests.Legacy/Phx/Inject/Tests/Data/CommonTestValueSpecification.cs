// -----------------------------------------------------------------------------
// <copyright file="CommonTestValueSpecification.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Tests.Data.Model;

namespace Phx.Inject.Tests.Data;

[Specification]
public static class UnusedSpecification {
    [Factory]
    internal static int GetInt() {
        return 12;
    }
}

[Specification]
[Link(typeof(IntLeaf), typeof(ILeaf))]
[Link(typeof(ILeaf), typeof(ILeaf), OutputQualifier = typeof(QualifierAAttribute))]
[Link(typeof(ILeaf), typeof(ILeaf), InputQualifier = typeof(QualifierAAttribute), OutputLabel = LabelA)]
[Link(typeof(ILeaf), typeof(ILeaf), InputLabel = LabelA, OutputLabel = LabelB)]
public static class CommonTestValueSpecification {
    public const int IntValue = 10;
    public const int LabelAIntValue = 20;
    public const int QualifierAIntValue = 20;
    public const string LabelAStringValue = "LabelAStringValue";
    public const string LabelA = "LabelA";
    public const string LabelB = "LabelB";

    [Factory]
    internal static int GetInt() {
        return IntValue;
    }

    [Factory]
    [Label(LabelA)]
    internal static int GetIntLabelA() {
        return LabelAIntValue;
    }

    [Factory]
    [Label(LabelA)]
    internal static string GetStringLabelA() {
        return LabelAStringValue;
    }

    [Factory]
    [QualifierA]
    internal static int GetIntQualifierA() {
        return QualifierAIntValue;
    }

    [Builder]
    internal static void BuildTestBuilderObject(TestBuilderObject target, int intValue) {
        target.IntValue = intValue;
    }

    [Builder]
    [Label(LabelA)]
    internal static void BuildTestBuilderObjectLabelA(TestBuilderObject target, [Label(LabelA)] int intValue) {
        target.IntValue = intValue;
    }

    [Factory]
    internal static TestGenericObject<int> GetGenericObject(int value) {
        return new TestGenericObject<int>(value);
    }

    [Factory]
    internal static OuterType GetOuterType(
        AutoType value,
        AutoTypeWithRequiredProperties autoTypeWithRequiredProperties) {
        return new OuterType(value, autoTypeWithRequiredProperties);
    }
}
