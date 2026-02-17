// -----------------------------------------------------------------------------
//  <copyright file="MetadataIndex.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See https://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Text.Json;
using YamlDotNet.RepresentationModel;

namespace PhxInject.DocFx.Plugins;

/// <summary>
///     Provides access to DocFX metadata YAML files via a manifest-based index.
/// </summary>
internal sealed class MetadataIndex {
    private readonly string metadataRoot;
    private readonly Dictionary<string, string> manifest;
    private readonly Dictionary<string, MetadataItem> cache = new(StringComparer.Ordinal);

    private sealed record MetadataLocation(string MetadataRoot, string ManifestPath);

    /// <summary>
    ///     Initializes a new instance of the <see cref="MetadataIndex" /> class.
    /// </summary>
    /// <param name="metadataRoot"> The root directory containing metadata YAML files. </param>
    /// <param name="manifest"> The manifest dictionary mapping UIDs to relative file paths. </param>
    public MetadataIndex(string metadataRoot, Dictionary<string, string> manifest) {
        this.metadataRoot = metadataRoot;
        this.manifest = manifest;
    }

    /// <summary>
    ///     Loads the metadata index based on the provided options.
    /// </summary>
    /// <param name="options"> The metadata resolution options. </param>
    /// <returns> A new <see cref="MetadataIndex" /> instance. </returns>
    public static MetadataIndex Load(DocFxPluginOptions.MetadataOptions options) {
        var location = ResolveLocation(options);
        var manifest = LoadManifest(location.ManifestPath);

        return new MetadataIndex(location.MetadataRoot, manifest);
    }

    /// <summary>
    ///     Gets the metadata item for the specified UID.
    /// </summary>
    /// <param name="uid"> The unique identifier for the item. </param>
    /// <returns> The <see cref="MetadataItem" /> for the specified UID. </returns>
    /// <exception cref="InvalidOperationException"> Thrown if the UID is not found or the file does not exist. </exception>
    public MetadataItem GetItem(string uid) {
        if (cache.TryGetValue(uid, out var cached)) {
            return cached;
        }

        if (!manifest.TryGetValue(uid, out var relativePath) || string.IsNullOrWhiteSpace(relativePath)) {
            throw new InvalidOperationException($"UID '{uid}' was not found in metadata manifest.");
        }

        var yamlPath = Path.Combine(metadataRoot, relativePath);
        if (!File.Exists(yamlPath)) {
            throw new InvalidOperationException($"Metadata file '{yamlPath}' was not found for UID '{uid}'.");
        }

        var item = ParseMetadataItem(yamlPath, uid);
        cache[uid] = item;
        return item;
    }

    private static MetadataLocation ResolveLocation(DocFxPluginOptions.MetadataOptions options) {
        if (!string.IsNullOrWhiteSpace(options.ManifestPath)) {
            var manifestPath = ResolvePath(options.BaseDirectory, options.ManifestPath);
            var root = Path.GetDirectoryName(manifestPath)
                ?? throw new InvalidOperationException("Manifest path has no directory component.");

            return new MetadataLocation(root, manifestPath);
        }

        if (!string.IsNullOrWhiteSpace(options.MetadataRoot)) {
            var metadataRoot = ResolvePath(options.BaseDirectory, options.MetadataRoot);
            var manifestPath = Path.Combine(metadataRoot, options.ManifestFileName);

            return new MetadataLocation(metadataRoot, manifestPath);
        }

        foreach (var candidate in options.CandidateRoots) {
            var metadataRoot = ResolvePath(options.BaseDirectory, candidate);
            var manifestPath = Path.Combine(metadataRoot, options.ManifestFileName);
            if (File.Exists(manifestPath)) {
                return new MetadataLocation(metadataRoot, manifestPath);
            }
        }

        throw new InvalidOperationException(
            "DocFX metadata manifest not found. Update the DocFxPluginOptions metadata configuration.");
    }

    private static Dictionary<string, string> LoadManifest(string manifestPath) {
        if (!File.Exists(manifestPath)) {
            throw new InvalidOperationException($"DocFX metadata manifest not found: {manifestPath}");
        }

        var manifestText = File.ReadAllText(manifestPath);
        var manifest = JsonSerializer.Deserialize<Dictionary<string, string>>(manifestText)
            ?? throw new InvalidOperationException($"Unable to parse manifest file: {manifestPath}");

        return manifest;
    }

    private static string ResolvePath(string baseDirectory, string path) {
        return Path.IsPathRooted(path)
            ? path
            : Path.GetFullPath(Path.Combine(baseDirectory, path));
    }

    private static MetadataItem ParseMetadataItem(string yamlPath, string uid) {
        using var reader = File.OpenText(yamlPath);
        var stream = new YamlStream();
        stream.Load(reader);

        var root = (YamlMappingNode)stream.Documents[0].RootNode;
        if (!root.Children.TryGetValue(new YamlScalarNode("items"), out var itemsNode) || itemsNode is not YamlSequenceNode items) {
            throw new InvalidOperationException($"No 'items' section found in '{yamlPath}'.");
        }

        foreach (var node in items) {
            if (node is not YamlMappingNode mapping) {
                continue;
            }

            var currentUid = GetScalar(mapping, "uid");
            if (!string.Equals(currentUid, uid, StringComparison.Ordinal)) {
                continue;
            }

            return new MetadataItem(
                GetScalar(mapping, "summary"),
                GetScalar(mapping, "remarks"),
                GetSyntaxContent(mapping),
                GetScalarSequence(mapping, "example"),
                GetMappedSequenceValues(mapping, "seealso", "linkId"),
                GetScalarSequence(mapping, "inheritance"),
                GetMappedSequenceValues(mapping, "attributes", "type"));
        }

        throw new InvalidOperationException($"UID '{uid}' was not found in '{yamlPath}'.");
    }

    private static string GetScalar(YamlMappingNode mapping, string key) {
        if (!mapping.Children.TryGetValue(new YamlScalarNode(key), out var value)) {
            return string.Empty;
        }

        return value switch {
            YamlScalarNode scalar => scalar.Value ?? string.Empty,
            _ => string.Empty
        };
    }

    private static string GetSyntaxContent(YamlMappingNode mapping) {
        if (!mapping.Children.TryGetValue(new YamlScalarNode("syntax"), out var syntaxNode) || syntaxNode is not YamlMappingNode syntax) {
            return string.Empty;
        }

        return GetScalar(syntax, "content");
    }

    private static IReadOnlyList<string> GetScalarSequence(YamlMappingNode mapping, string key) {
        if (!mapping.Children.TryGetValue(new YamlScalarNode(key), out var sequenceNode) || sequenceNode is not YamlSequenceNode sequence) {
            return [];
        }

        return sequence
            .OfType<YamlScalarNode>()
            .Select(static scalar => scalar.Value ?? string.Empty)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
    }

    private static IReadOnlyList<string> GetMappedSequenceValues(YamlMappingNode mapping, string key, string childKey) {
        if (!mapping.Children.TryGetValue(new YamlScalarNode(key), out var sequenceNode) || sequenceNode is not YamlSequenceNode sequence) {
            return [];
        }

        var values = new List<string>();
        foreach (var node in sequence) {
            if (node is not YamlMappingNode childMap) {
                continue;
            }

            var value = GetScalar(childMap, childKey);
            if (!string.IsNullOrWhiteSpace(value)) {
                values.Add(value);
            }
        }

        return values;
    }
}
