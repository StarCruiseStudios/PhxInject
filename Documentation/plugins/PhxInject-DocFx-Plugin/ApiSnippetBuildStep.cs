using System.Collections.Immutable;
using System.Composition;
using System.Text.Json;
using System.Text.RegularExpressions;
using Docfx.Common;
using Docfx.Plugins;
using YamlDotNet.RepresentationModel;

namespace PhxInject.DocFx.Plugins;

[Export("ConceptualDocumentProcessor", typeof(IDocumentBuildStep))]
public sealed partial class ApiSnippetBuildStep : IDocumentBuildStep
{
    private static readonly string[] SupportedFields =
    [
        "summary",
        "remarks",
        "syntax",
        "example",
        "seealso",
        "inheritance",
        "attributes"
    ];

    private static readonly Lazy<MetadataIndex> Metadata = new(LoadMetadataIndex);

    public string Name => nameof(ApiSnippetBuildStep);

    public int BuildOrder => -1000;

    public IEnumerable<FileModel> Prebuild(ImmutableList<FileModel> models, IHostService host)
    {
        return models;
    }

    public void Build(FileModel model, IHostService host)
    {
        if (model.Content is not IDictionary<string, object> content)
        {
            return;
        }

        if (!content.TryGetValue("conceptual", out var conceptualObject) || conceptualObject is not string markdown)
        {
            return;
        }

        if (!markdown.Contains(":::api-snippet", StringComparison.Ordinal))
        {
            return;
        }

        var rewritten = RewriteMarkdown(markdown, model.File);
        content["conceptual"] = rewritten;
    }

    public void Postbuild(ImmutableList<FileModel> models, IHostService host)
    {
    }

    private static string RewriteMarkdown(string markdown, string file)
    {
        var lines = markdown.Split(["\r\n", "\n"], StringSplitOptions.None);
        var output = new List<string>(lines.Length + 32);

        var index = 0;
        while (index < lines.Length)
        {
            var line = lines[index];
            var directive = ApiSnippetDirectiveRegex().Match(line);
            if (!directive.Success)
            {
                output.Add(line);
                index++;
                continue;
            }

            var rawSpec = directive.Groups["spec"].Value.Trim();
            var parsedSpec = ParseSpec(rawSpec);
            var item = Metadata.Value.GetItem(parsedSpec.Uid);
            var snippet = RenderSnippet(item, parsedSpec.Field);

            if (!string.IsNullOrWhiteSpace(snippet))
            {
                output.AddRange(snippet.Split(["\r\n", "\n"], StringSplitOptions.None));
            }

            index++;

            if (index < lines.Length && string.IsNullOrWhiteSpace(lines[index].Trim(':', ' ', '\t')))
            {
                index++;
            }

            if (index < lines.Length && ApiSnippetStartRegex().IsMatch(lines[index]))
            {
                index++;
                while (index < lines.Length && !ApiSnippetEndRegex().IsMatch(lines[index]))
                {
                    index++;
                }

                if (index < lines.Length)
                {
                    index++;
                }
            }
        }

        return string.Join(Environment.NewLine, output);
    }

    private static (string Uid, string Field) ParseSpec(string spec)
    {
        var lastDot = spec.LastIndexOf('.');
        if (lastDot <= 0 || lastDot == spec.Length - 1)
        {
            throw new InvalidOperationException($"Invalid api-snippet directive '{spec}'. Use <uid>.<field>.");
        }

        var uid = spec[..lastDot].Trim();
        var field = spec[(lastDot + 1)..].Trim().ToLowerInvariant();

        if (!SupportedFields.Contains(field, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                $"Unsupported api-snippet field '{field}'. Supported fields: {string.Join(", ", SupportedFields)}.");
        }

        return (uid, field);
    }

    private static string RenderSnippet(MetadataItem item, string field)
    {
        return field switch
        {
            "summary" => NormalizeXrefs(item.Summary).TrimEnd(),
            "remarks" => NormalizeXrefs(item.Remarks).TrimEnd(),
            "syntax" => RenderSyntax(item.SyntaxContent),
            "example" => RenderList(item.Example.Select(NormalizeXrefs)),
            "seealso" => RenderList(item.SeeAlso.Select(link => $"- <xref:{link}>")),
            "inheritance" => RenderList(item.Inheritance.Select(link => $"- <xref:{link}>")),
            "attributes" => RenderList(item.Attributes.Select(link => $"- <xref:{link}>")),
            _ => throw new InvalidOperationException($"Unsupported field '{field}'.")
        };
    }

    private static string RenderSyntax(string syntaxContent)
    {
        if (string.IsNullOrWhiteSpace(syntaxContent))
        {
            return string.Empty;
        }

        return string.Join(
            Environment.NewLine,
            [
                "```csharp",
                syntaxContent.TrimEnd(),
                "```"
            ]);
    }

    private static string RenderList(IEnumerable<string> lines)
    {
        var filtered = lines.Where(static line => !string.IsNullOrWhiteSpace(line)).ToArray();
        return filtered.Length == 0 ? string.Empty : string.Join(Environment.NewLine, filtered);
    }

    private static string NormalizeXrefs(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return XrefRegex().Replace(
            text,
            match =>
            {
                var uid = match.Groups["uid"].Value;
                var inner = match.Groups["inner"].Value;

                if (string.IsNullOrWhiteSpace(inner))
                {
                    return $"<xref:{uid}>";
                }

                return $"[{inner}](xref:{uid})";
            });
    }

    private static MetadataIndex LoadMetadataIndex()
    {
        var metadataRoot = Path.GetFullPath(Path.Combine(EnvironmentContext.BaseDirectory, "../src/Phx.Inject.Generator/bin/docs"));
        var manifestPath = Path.Combine(metadataRoot, ".manifest");

        if (!File.Exists(manifestPath))
        {
            throw new InvalidOperationException($"DocFX metadata manifest not found: {manifestPath}");
        }

        var manifestText = File.ReadAllText(manifestPath);
        var manifest = JsonSerializer.Deserialize<Dictionary<string, string>>(manifestText)
            ?? throw new InvalidOperationException($"Unable to parse manifest file: {manifestPath}");

        return new MetadataIndex(metadataRoot, manifest);
    }

    [GeneratedRegex("^\\s*:::\\s*api-snippet\\s+(?<spec>.+?)\\s*$", RegexOptions.Compiled)]
    private static partial Regex ApiSnippetDirectiveRegex();

    [GeneratedRegex("^\\s*<!--\\s*api-snippet:start\\s+.*-->\\s*$", RegexOptions.Compiled)]
    private static partial Regex ApiSnippetStartRegex();

    [GeneratedRegex("^\\s*<!--\\s*api-snippet:end\\s*-->\\s*$", RegexOptions.Compiled)]
    private static partial Regex ApiSnippetEndRegex();

    [GeneratedRegex("<xref\\s+href=\"(?<uid>[^\"]+)\"[^>]*>(?<inner>.*?)</xref>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex XrefRegex();

    private sealed class MetadataIndex
    {
        private readonly string metadataRoot;
        private readonly Dictionary<string, string> manifest;
        private readonly Dictionary<string, MetadataItem> cache = new(StringComparer.Ordinal);

        public MetadataIndex(string metadataRoot, Dictionary<string, string> manifest)
        {
            this.metadataRoot = metadataRoot;
            this.manifest = manifest;
        }

        public MetadataItem GetItem(string uid)
        {
            if (cache.TryGetValue(uid, out var cached))
            {
                return cached;
            }

            if (!manifest.TryGetValue(uid, out var relativePath) || string.IsNullOrWhiteSpace(relativePath))
            {
                throw new InvalidOperationException($"UID '{uid}' was not found in metadata manifest.");
            }

            var yamlPath = Path.Combine(metadataRoot, relativePath);
            if (!File.Exists(yamlPath))
            {
                throw new InvalidOperationException($"Metadata file '{yamlPath}' was not found for UID '{uid}'.");
            }

            var item = ParseMetadataItem(yamlPath, uid);
            cache[uid] = item;
            return item;
        }

        private static MetadataItem ParseMetadataItem(string yamlPath, string uid)
        {
            using var reader = File.OpenText(yamlPath);
            var stream = new YamlStream();
            stream.Load(reader);

            var root = (YamlMappingNode)stream.Documents[0].RootNode;
            if (!root.Children.TryGetValue(new YamlScalarNode("items"), out var itemsNode) || itemsNode is not YamlSequenceNode items)
            {
                throw new InvalidOperationException($"No 'items' section found in '{yamlPath}'.");
            }

            foreach (var node in items)
            {
                if (node is not YamlMappingNode mapping)
                {
                    continue;
                }

                var currentUid = GetScalar(mapping, "uid");
                if (!string.Equals(currentUid, uid, StringComparison.Ordinal))
                {
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

        private static string GetScalar(YamlMappingNode mapping, string key)
        {
            if (!mapping.Children.TryGetValue(new YamlScalarNode(key), out var value))
            {
                return string.Empty;
            }

            return value switch
            {
                YamlScalarNode scalar => scalar.Value ?? string.Empty,
                _ => string.Empty
            };
        }

        private static string GetSyntaxContent(YamlMappingNode mapping)
        {
            if (!mapping.Children.TryGetValue(new YamlScalarNode("syntax"), out var syntaxNode) || syntaxNode is not YamlMappingNode syntax)
            {
                return string.Empty;
            }

            return GetScalar(syntax, "content");
        }

        private static IReadOnlyList<string> GetScalarSequence(YamlMappingNode mapping, string key)
        {
            if (!mapping.Children.TryGetValue(new YamlScalarNode(key), out var sequenceNode) || sequenceNode is not YamlSequenceNode sequence)
            {
                return [];
            }

            return sequence
                .OfType<YamlScalarNode>()
                .Select(static scalar => scalar.Value ?? string.Empty)
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .ToArray();
        }

        private static IReadOnlyList<string> GetMappedSequenceValues(YamlMappingNode mapping, string key, string childKey)
        {
            if (!mapping.Children.TryGetValue(new YamlScalarNode(key), out var sequenceNode) || sequenceNode is not YamlSequenceNode sequence)
            {
                return [];
            }

            var values = new List<string>();
            foreach (var node in sequence)
            {
                if (node is not YamlMappingNode childMap)
                {
                    continue;
                }

                var value = GetScalar(childMap, childKey);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    values.Add(value);
                }
            }

            return values;
        }
    }

    private sealed record MetadataItem(
        string Summary,
        string Remarks,
        string SyntaxContent,
        IReadOnlyList<string> Example,
        IReadOnlyList<string> SeeAlso,
        IReadOnlyList<string> Inheritance,
        IReadOnlyList<string> Attributes);
}
