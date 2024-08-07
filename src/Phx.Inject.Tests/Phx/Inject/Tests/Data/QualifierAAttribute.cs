﻿// -----------------------------------------------------------------------------
// <copyright file="QualifierAAttribute.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data {
    [Qualifier]
    [AttributeUsage(QualifierAttribute.Usage)]
    internal class QualifierAAttribute : Attribute { }
}