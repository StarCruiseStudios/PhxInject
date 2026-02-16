// -----------------------------------------------------------------------------
//  <copyright file="ApiSnippetBuildStep.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See https://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

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

    private static readonly string[] SupportedTags =
    [
        "ApiDoc"
    ];

    private static readonly Lazy<MetadataIndex> Metadata = new(LoadMetadataIndex);

    private sealed record ListIndexRange(int Start, int? End);

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

        var rewritten = markdown;
        
        if (markdown.Contains(":::api-snippet", StringComparison.Ordinal))
        {
            rewritten = RewriteMarkdown(markdown, model.File);
        }

        // Convert [Identifier] links to [Identifier](xref:Identifier) references
        rewritten = AutoLinkIdentifiers(rewritten);
        
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
            var snippet = RenderSnippet(item, parsedSpec.Field, parsedSpec.Tag, parsedSpec.ListIndex);

            if (!string.IsNullOrWhiteSpace(snippet))
            {
                output.AddRange(snippet.Split(["\r\n", "\n"], StringSplitOptions.None));
                output.Add(string.Empty);
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

    private static string AutoLinkIdentifiers(string markdown)
    {
        // Temporarily replace code blocks to prevent link conversion inside them
        var codeBlocks = new List<string>();
        var placeholder = "<<<CODE_BLOCK_{0}>>>";
        
        var result = markdown;
        
        // Extract markdown ``` code blocks
        var markdownCodeRegex = new Regex(@"```[\s\S]*?```", RegexOptions.Compiled | RegexOptions.Singleline);
        var markdownMatches = markdownCodeRegex.Matches(markdown);
        foreach (Match match in markdownMatches)
        {
            codeBlocks.Add(match.Value);
            var codeBlockPlaceholder = string.Format(placeholder, codeBlocks.Count - 1);
            result = result.Replace(match.Value, codeBlockPlaceholder, StringComparison.Ordinal);
        }
        
        // Extract XML <code> blocks
        var xmlCodeRegex = new Regex(@"<code>.*?</code>", RegexOptions.Compiled | RegexOptions.Singleline);
        var xmlMatches = xmlCodeRegex.Matches(result);
        var offset = 0;
        foreach (Match match in xmlMatches)
        {
            codeBlocks.Add(match.Value);
            var codeBlockPlaceholder = string.Format(placeholder, codeBlocks.Count - 1);
            result = result.Replace(match.Value, codeBlockPlaceholder, StringComparison.Ordinal);
        }
        
        // Process identifier links in the non-code content
        result = IdentifierLinkRegex().Replace(
            result,
            match =>
            {
                var identifier = match.Groups["identifier"].Value;
                var uid = "link." + identifier.ToLower().Replace(" ", ".");
                return $"<xref href=\"{uid}?text={identifier}\" />";
            });
        
        // Restore code blocks
        for (var i = 0; i < codeBlocks.Count; i++)
        {
            var codeBlockPlaceholder = string.Format(placeholder, i);
            var codeBlock = codeBlocks[i];
            result = result.Replace(codeBlockPlaceholder, codeBlock, StringComparison.Ordinal);
        }
        
        return result;
    }

    private static (string Uid, string Field, string? Tag, ListIndexRange? ListIndex) ParseSpec(string spec)
    {
        // Find the bracket position to avoid parsing dots inside bracket notation
        var bracketPosition = spec.IndexOf('[');
        var searchRange = bracketPosition >= 0 ? spec[..bracketPosition] : spec;
        
        var lastDot = searchRange.LastIndexOf('.');
        if (lastDot <= 0 || lastDot == searchRange.Length - 1)
        {
            throw new InvalidOperationException($"Invalid api-snippet directive '{spec}'. Use <uid>.<field>, <uid>.<field>[tag], <uid>.<field>[n], or <uid>.<field>[..n].");
        }

        var uid = searchRange[..lastDot].Trim();
        var remainder = spec[(lastDot + 1)..].Trim().ToLowerInvariant();

        string field;
        string? tag = null;
        ListIndexRange? listIndex = null;
        var fieldBracketIndex = remainder.IndexOf('[');
        
        if (fieldBracketIndex > 0)
        {
            field = remainder[..fieldBracketIndex].Trim();
            var closeBracket = remainder.IndexOf(']', fieldBracketIndex);
            if (closeBracket > fieldBracketIndex + 1)
            {
                var bracketed = remainder[(fieldBracketIndex + 1)..closeBracket].Trim();
                
                // Check if it's a list index/range (numeric or .. range syntax)
                if (IsListIndexOrRange(bracketed))
                {
                    listIndex = ParseListIndexRange(bracketed);
                }
                else
                {
                    // It's a tag
                    tag = bracketed;
                }
            }
        }
        else
        {
            field = remainder;
        }

        if (!SupportedFields.Contains(field, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                $"Unsupported api-snippet field '{field}'. Supported fields: {string.Join(", ", SupportedFields)}.");
        }

        return (uid, field, tag, listIndex);
    }

    private static bool IsListIndexOrRange(string bracketed)
    {
        // Single numeric index: "0", "5", etc.
        if (int.TryParse(bracketed, out _))
        {
            return true;
        }

        // Up-to range syntax: "..3", "..10", etc.
        if (bracketed.StartsWith("..") && bracketed.Length > 2)
        {
            return int.TryParse(bracketed[2..], out _);
        }

        return false;
    }

    private static ListIndexRange ParseListIndexRange(string bracketed)
    {
        // Single index
        if (int.TryParse(bracketed, out var index))
        {
            return new ListIndexRange(index, index);
        }

        // Up-to range: "..n"
        if (bracketed.StartsWith("..") && bracketed.Length > 2)
        {
            if (int.TryParse(bracketed[2..], out var count))
            {
                return new ListIndexRange(0, count);
            }
        }

        throw new InvalidOperationException($"Invalid list index syntax: [{bracketed}]. Use [n] for single element or [..n] for up to n elements.");
    }

    private static string RenderSnippet(MetadataItem item, string field, string? tag = null, ListIndexRange? listIndex = null)
    {
        // Apply list indexing before rendering (if applicable)
        var content = field switch
        {
            "summary" => NormalizeXrefs(item.Summary).TrimEnd(),
            "remarks" => RenderList(ApplyListIndexing(SplitRemarksIntoSections(item.Remarks), listIndex).Select(NormalizeXrefs)),
            "syntax" => RenderSyntax(item.SyntaxContent),
            "example" => RenderList(ApplyListIndexing(item.Example, listIndex).Select(NormalizeXrefs)),
            "seealso" => RenderList(ApplyListIndexing(item.SeeAlso, listIndex).Select(link => $"- <xref:{link}>")),
            "inheritance" => RenderList(ApplyListIndexing(item.Inheritance, listIndex).Select(link => $"- <xref:{link}>")),
            "attributes" => RenderList(ApplyListIndexing(item.Attributes, listIndex).Select(link => $"- <xref:{link}>")),
            _ => throw new InvalidOperationException($"Unsupported field '{field}'.")
        };

        // If a tag is specified, extract only the tagged section
        if (!string.IsNullOrWhiteSpace(tag))
        {
            content = ExtractTaggedSection(content, tag);
        }

        return content;
    }

    private static IReadOnlyList<T> ApplyListIndexing<T>(IReadOnlyList<T> list, ListIndexRange? range) where T : class
    {
        if (range == null)
        {
            return list;
        }

        if (range.Start == range.End)
        {
            // Single element access - throw error if out of bounds
            if (range.Start < 0 || range.Start >= list.Count)
            {
                throw new InvalidOperationException($"List index {range.Start} is out of bounds (list has {list.Count} elements).");
            }
            return [list[range.Start]];
        }
        else
        {
            // Up-to range access - clamp to available elements
            var count = range.End ?? list.Count;
            var actual = Math.Min(count, list.Count);
            
            if (actual <= 0)
            {
                return [];
            }

            return list.Take(actual).ToList();
        }
    }

    private static IReadOnlyList<string> SplitRemarksIntoSections(string remarks)
    {
        if (string.IsNullOrWhiteSpace(remarks))
        {
            return [];
        }

        // Split remarks by section dividers (<!-- ... -->) to identify section boundaries
        var sections = new List<string>();
        var pattern = @"<!--\s*\.\.\.\s*-->";
        var commentMatches = Regex.Matches(remarks, pattern);

        if (commentMatches.Count == 0)
        {
            // No section dividers found, return the whole remarks as a single section
            return [remarks.Trim()];
        }

        var lastIndex = 0;
        foreach (Match match in commentMatches)
        {
            // Get content before this divider (if any)
            if (match.Index > lastIndex)
            {
                var before = remarks[lastIndex..match.Index].Trim();
                if (!string.IsNullOrWhiteSpace(before))
                {
                    sections.Add(before);
                }
            }

            lastIndex = match.Index + match.Length;
        }

        // Add any remaining content after the last divider
        if (lastIndex < remarks.Length)
        {
            var remaining = remarks[lastIndex..].Trim();
            if (!string.IsNullOrWhiteSpace(remaining))
            {
                sections.Add(remaining);
            }
        }

        return sections.Count == 0 ? [remarks.Trim()] : sections;
    }
    private static string ExtractTaggedSection(string content, string tag)
    {
        // Parse the tag to check for a label (e.g., "ApiDoc:Injector")
        var (tagName, label) = ParseTagWithLabel(tag);
        
        // Look for <!-- tagName --> or <!-- tagName:label --> and extract content between them
        // Pattern matches: <!-- ApiDoc --> or <!-- ApiDoc:Injector --> etc.
        string pattern;
        if (string.IsNullOrWhiteSpace(label))
        {
            // Match any ApiDoc tag regardless of label
            pattern = $@"<!--\s*{Regex.Escape(tagName)}(?::[^-]*)?\s*-->(.*?)(?=<!--|$)";
        }
        else
        {
            // Match only ApiDoc tags with the specific label
            pattern = $@"<!--\s*{Regex.Escape(tagName)}:{Regex.Escape(label)}\s*-->(.*?)(?=<!--|$)";
        }

        var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var matches = regex.Matches(content);
        
        if (matches.Count == 0)
        {
            return string.Empty;
        }

        // Combine all matching sections
        var sections = new List<string>();
        foreach (Match match in matches)
        {
            var section = match.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(section))
            {
                sections.Add(section);
            }
        }

        return sections.Count == 0 ? string.Empty : string.Join(Environment.NewLine, sections);
    }

    private static (string TagName, string? Label) ParseTagWithLabel(string tag)
    {
        var colonIndex = tag.IndexOf(':');
        if (colonIndex > 0 && colonIndex < tag.Length - 1)
        {
            return (tag[..colonIndex].Trim(), tag[(colonIndex + 1)..].Trim());
        }

        return (tag, null);
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

    [GeneratedRegex("\\[(?<identifier>[A-Za-z_][A-Za-z0-9._ ]*)\\](?!\\()", RegexOptions.Compiled)]
    private static partial Regex IdentifierLinkRegex();

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
