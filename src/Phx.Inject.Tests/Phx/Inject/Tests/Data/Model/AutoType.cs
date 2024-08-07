// -----------------------------------------------------------------------------
// <copyright file="AutoType.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Model {
    public class AutoType {
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

        internal OuterType(AutoType autoType) {
            AutoType = autoType;
        }
    }
}
