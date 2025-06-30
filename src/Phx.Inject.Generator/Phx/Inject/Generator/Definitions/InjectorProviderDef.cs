// -----------------------------------------------------------------------------
//  <copyright file="InjectorProviderDef.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Model;

namespace Phx.Inject.Generator.Definitions;

internal record InjectorProviderDef(
    QualifiedTypeModel ProvidedType,
    string InjectorProviderMethodName,
    SpecContainerFactoryInvocationDef SpecContainerFactoryInvocation,
    Location Location
) : IDefinition;
