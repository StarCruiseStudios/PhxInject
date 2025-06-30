// -----------------------------------------------------------------------------
//  <copyright file="IsExternalInit.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices;

/// <summary>
///     Reserved to be used by the compiler for tracking metadata. This class should not be used by
///     developers in source code.
/// </summary>
/// <remarks>
///     C# 9 added support for the init and record keywords. When using C# 9 with target frameworks
///     prior to .NET 5.0, using these new features is not possible because the compiler is missing the
///     IsExternalInit class. This problem can be solved by re-declaring the IsExternalInit class as an
///     internal class in your own project. The compiler will use this custom class definition and
///     allow you to use both the init keywords and records in any project.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal static class IsExternalInit { }
