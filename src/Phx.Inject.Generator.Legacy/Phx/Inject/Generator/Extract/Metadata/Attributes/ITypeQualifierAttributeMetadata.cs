﻿// -----------------------------------------------------------------------------
// <copyright file="ITypeQualifierAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Common.Model;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal interface ITypeQualifierAttributeMetadata : IMetadata {
    IQualifier Qualifier { get; }
}
