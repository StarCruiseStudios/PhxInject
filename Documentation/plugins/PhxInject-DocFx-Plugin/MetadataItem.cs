// -----------------------------------------------------------------------------
//  <copyright file="MetadataItem.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See https://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace PhxInject.DocFx.Plugins;

/// <summary>
///     Represents metadata extracted from a DocFX YAML file for a single API item.
/// </summary>
/// <param name="Summary"> The summary documentation for the item. </param>
/// <param name="Remarks"> The remarks documentation for the item. </param>
/// <param name="Example"> The example documentation sections for the item. </param>
internal sealed record MetadataItem(
    string Summary,
    string Remarks,
    IReadOnlyList<string> Example);
