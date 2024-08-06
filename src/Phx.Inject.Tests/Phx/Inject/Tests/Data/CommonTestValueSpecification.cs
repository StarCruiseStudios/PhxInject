// -----------------------------------------------------------------------------
// <copyright file="CommonTestValueSpecification.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data {

    public class TestBuilderObject {
        public int IntValue { get; set; }
    }
    
    [Specification]
    public static class CommonTestValueSpecification {
        public const int IntValue = 10;
        public const int LabelAIntValue = 20;
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
        
        [Builder]
        internal static void BuildTestBuilderObject(TestBuilderObject target, int intValue) {
            target.IntValue = intValue;
        }
        
        [Builder]
        [Label(LabelA)]
        internal static void BuildTestBuilderObjectLabelA(TestBuilderObject target, [Label(LabelA)] int intValue) {
            target.IntValue = intValue;
        }
    }
}
