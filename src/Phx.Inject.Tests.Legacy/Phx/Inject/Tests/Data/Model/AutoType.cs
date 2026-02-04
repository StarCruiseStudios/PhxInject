// -----------------------------------------------------------------------------
// <copyright file="AutoType.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Model;

public class AutoBuilderType {
    public int Value { get; private set; }

    [Builder]
    public static void Build(AutoBuilderType builder, int value) {
        builder.Value = value;
    }

    [Builder]
    [Label(nameof(AutoBuilderType))]
    public static void BuildCustomLabel(AutoBuilderType builder, int value) {
        builder.Value = value + value;
    }
}

public interface IAutoType {
    int Value { get; }
    AutoTypeWithFabricationMode AutoTypeWithFabricationMode { get; }
}

[Link(typeof(AutoType), typeof(IAutoType))]
public class AutoType : IAutoType {
    public int Value { get; }

    public AutoTypeWithFabricationMode AutoTypeWithFabricationMode { get; }
    public AutoType(int value, AutoTypeWithFabricationMode autoTypeWithFabricationMode) {
        Value = value;
        AutoTypeWithFabricationMode = autoTypeWithFabricationMode;
    }
}

[Factory(FabricationMode.Scoped)]
public class AutoTypeWithFabricationMode {
    public int X { get; }

    public AutoTypeWithFabricationMode(int x) {
        X = x;
    }
}

public class OuterType {
    internal AutoType AutoType { get; }
    internal AutoTypeWithRequiredProperties AutoTypeWithRequiredProperties { get; }

    internal OuterType(AutoType autoType, AutoTypeWithRequiredProperties autoTypeWithRequiredProperties) {
        AutoType = autoType;
        AutoTypeWithRequiredProperties = autoTypeWithRequiredProperties;
    }
}

public record class AutoTypeWithRequiredProperties(IAutoType autoType) {
    public IAutoType AutoType { get; } = autoType;
    public required int X { get; init; }
    public required AutoTypeWithFabricationMode Y { get; init; }
}
