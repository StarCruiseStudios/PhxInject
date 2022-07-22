// -----------------------------------------------------------------------------
//  <copyright file="FabricationModeExtractorTest.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Controllers.Builders {
    using NUnit.Framework;
    using Phx.Inject.Generator.Actions.Definition;
    using Phx.Test;

    public class FabricationModeExtractorTest : LoggingTestClass {
        private static readonly object[] TestCases = {
            new object[] { @"
                namespace Phx.Inject.Test.Data {
                    // A factory with the default fabrication mode.
                    [Specification]
                    internal static class TestSpec { 
                        [Factory]
                        internal static int GetInt() {
                            return 10;
                        }
                    }
                }", FabricationModeDefinition.Recurrent },
            new object[] { @"
                namespace Phx.Inject.Test.Data {
                    // A factory with an explicit recurrent fabrication mode.
                    [Specification]
                    internal static class TestSpec { 
                        [Factory(FabricationMode.Recurrent)]
                        internal static int GetInt() {
                            return 10;
                        }
                    }
                }", FabricationModeDefinition.Recurrent },
            new object[] { @"
                namespace Phx.Inject.Test.Data {
                    // A factory with an explicit scoped fabrication mode.
                    [Specification]
                    internal static class TestSpec { 
                        [Factory(FabricationMode.Scoped)]
                        internal static int GetInt() {
                            return 10;
                        }
                    }
                }", FabricationModeDefinition.Scoped },
            new object[] { @"
                namespace Phx.Inject.Test.Data {
                    // A factory with a named fabrication mode parameter.
                    [Specification]
                    internal static class TestSpec { 
                        [Factory(fabricationMode: FabricationMode.Scoped)]
                        internal static int GetInt() {
                            return 10;
                        }
                    }
                }", FabricationModeDefinition.Scoped },
            new object?[] { @"
                namespace Phx.Inject.Test.Data {
                    // A specification with no factory attribute.
                    [Specification]
                    internal static class TestSpec { 
                        internal static int GetInt() {
                            return 10;
                        }
                    }
                }", null },
            new object?[] { @"
                namespace Phx.Inject.Test.Data {
                    // A specification with non static factory attribute.
                    [Specification]
                    internal static class TestSpec { 
                        [Factory]
                        internal int GetInt() {
                            return 10;
                        }
                    }
                }", null },
        };

        [TestCaseSource(nameof(TestCases))]
        public void FabricationModeIsExtracted(string testCode, FabricationModeDefinition? expected) {
            Given("Code for a Specification.", () => testCode);
            Then("Everthing is awesome!", expected, (e) => true);
        }
    }
}
