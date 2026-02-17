// -----------------------------------------------------------------------------
//  <copyright file="ApiSnippetReplacementBuildStep.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See https://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using System.Composition;
using System.Text.RegularExpressions;
using Docfx.Plugins;

namespace PhxInject.DocFx.Plugins;


internal sealed record ListIndexRange(int Start, int? End) {
    public static bool TryParse(string input, out ListIndexRange? range) {
        range = null;
        if (int.TryParse(input, out var index)) {
            range = new ListIndexRange(index, index);
            return true;
        }

        if (input.StartsWith("..") && input.Length > 2 && int.TryParse(input[2..], out var count)) {
            range = new ListIndexRange(0, count);
            return true;
        }

        return false;
    }
}

internal sealed record ApiSnippetArgs(string Uid, string Field, string? Tag, ListIndexRange? ListIndex) {
    private static readonly ImmutableHashSet<string> SupportedFields =
        ImmutableHashSet.Create(
            StringComparer.OrdinalIgnoreCase,
            "summary",
            "remarks",
            "example"
        );
    
    public static ApiSnippetArgs Parse(string args) {
        // Find the bracket position to avoid parsing dots inside bracket notation
        var bracketPosition = args.IndexOf('[');
        var searchRange = bracketPosition >= 0 ? args[..bracketPosition] : args;

        var lastDot = searchRange.LastIndexOf('.');
        if (lastDot <= 0 || lastDot == searchRange.Length - 1) {
            throw new InvalidOperationException(
                $"Invalid api-snippet directive '{args}'. Use <uid>.<field>, <uid>.<field>[tag], " +
                "<uid>.<field>[n], or <uid>.<field>[..n].");
        }

        var uid = searchRange[..lastDot].Trim();
        var remainder = args[(lastDot + 1)..].Trim();

        var field = remainder;
        string? tag = null;
        ListIndexRange? listIndex = null;
    
        var fieldBracketIndex = remainder.IndexOf('[');
        if (fieldBracketIndex > 0) {
            field = remainder[..fieldBracketIndex].Trim();
            var closeBracket = remainder.IndexOf(']', fieldBracketIndex);
            if (closeBracket > fieldBracketIndex + 1) {
                var bracketed = remainder[(fieldBracketIndex + 1)..closeBracket].Trim();
                if (!ListIndexRange.TryParse(bracketed, out listIndex)) {
                    tag = bracketed;
                }
            }
        }

        field = field.Trim().ToLowerInvariant();

        if (!SupportedFields.Contains(field)) {
            throw new InvalidOperationException(
                $"Unsupported api-snippet field '{field}'. Supported fields: {string.Join(", ", SupportedFields)}.");
        }

        return new ApiSnippetArgs(uid, field, tag, listIndex);
    }
}

/// <summary>
///     DocFX build step that replaces <c>:::api-snippet</c> directives with content from DocFX metadata files.
/// </summary>
[Export("ConceptualDocumentProcessor", typeof(IDocumentBuildStep))]
public sealed partial class ApiSnippetReplacementBuildStep : IDocumentBuildStep {
    [GeneratedRegex(@"<!-- \.\.\. -->", RegexOptions.Compiled)]
    private static partial Regex SectionDividerRegex();
    
    [GeneratedRegex(@"<!--\s*(?<tag>\S+?)(?:\:(?<label>\S+?))?\s*-->(?<section>.*?)(?=<!--|$)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TagLabelRegex();
    
    [GeneratedRegex($"^\\s*:::\\s*(?<directive>\\S+?)\\s+(?<args>.+?)\\s*$", RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex DirectiveRegex();
    
    [GeneratedRegex(
        "<xref\\s+href=\"(?<uid>[^\"]+)\"[^>]*>(?<inner>.*?)</xref>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex XrefRegex();
    
    private static readonly ImmutableHashSet<string> SupportedDirectives =
        ImmutableHashSet.Create(
            StringComparer.OrdinalIgnoreCase,
            "api-snippet"
        );
    
    private static readonly string[] LineSeparators = ["\r\n", "\n"];
    
    private static readonly Lazy<MetadataIndex> Metadata = new(MetadataIndex.Load);

    /// <inheritdoc cref="IDocumentBuildStep.Name"/>
    public string Name => nameof(ApiSnippetReplacementBuildStep);

    /// <inheritdoc cref="IDocumentBuildStep.BuildOrder"/>
    public int BuildOrder => -1000;

    /// <inheritdoc cref="IDocumentBuildStep.Prebuild"/>
    public IEnumerable<FileModel> Prebuild(ImmutableList<FileModel> models, IHostService host) => models;

    /// <inheritdoc cref="IDocumentBuildStep.Build"/>
    public void Build(FileModel model, IHostService host) {
        if (model.Content is not IDictionary<string, object> content) {
            return;
        }

        if (!content.TryGetValue("conceptual", out var conceptualObject) || conceptualObject is not string markdown) {
            return;
        }

        if (!DirectiveRegex().IsMatch(markdown)) {
            return;
        }

        content["conceptual"] = RewriteMarkdown(markdown);
    }

    /// <inheritdoc />
    public void Postbuild(ImmutableList<FileModel> models, IHostService host) { }

    private static string RewriteMarkdown(string markdown) {
        var output = new List<string>();
        foreach (var line in markdown.Split(LineSeparators, StringSplitOptions.None)) {
            var directiveMatch = DirectiveRegex().Match(line);
            var directive = directiveMatch.Success
                ? directiveMatch.Groups["directive"].Value.Trim().ToLowerInvariant()
                : null;
            if (directive == null || !SupportedDirectives.Contains(directive)) {
                output.Add(line);
                continue;
            }
            
            var rawArgs = directiveMatch.Groups["args"].Value.Trim();

            switch (directive) {
                case "api-snippet":
                    var snippet = ProcessApiSnippet(rawArgs);
                    if (snippet != null) {
                        output.AddRange(snippet.Split(LineSeparators, StringSplitOptions.None));
                        output.Add(string.Empty);
                    }

                    break;
                default:
                    throw new InvalidOperationException($"Unsupported directive '{directive}'.");
            }
        }

        return string.Join(Environment.NewLine, output);
    }

    private static string? ProcessApiSnippet(string rawArgs) {
        var args = ApiSnippetArgs.Parse(rawArgs);
        var item = Metadata.Value.GetItem(args.Uid);
        var content = args.Field switch {
            "summary" => NormalizeXrefs(item.Summary).TrimEnd(),
            "remarks" => NormalizeXrefs(RenderList(ApplyListIndexing(FilterByTag(SplitIntoSections(item.Remarks), args.Tag), args.ListIndex))),
            "example" => NormalizeXrefs(RenderList(ApplyListIndexing(FilterByTag(item.Example, args.Tag), args.ListIndex))),
            _ => throw new InvalidOperationException($"Unsupported field '{args.Field}'.")
        };

        return string.IsNullOrWhiteSpace(content) ? null : content;
    }
    
    private static IReadOnlyList<T> ApplyListIndexing<T>(IReadOnlyList<T> list, ListIndexRange? range) {
        if (range == null) {
            return list;
        }

        if (range.Start == range.End) {
            // Single element access - throw error if out of bounds
            if (range.Start < 0 || range.Start >= list.Count) {
                throw new InvalidOperationException(
                    $"List index {range.Start} is out of bounds (list has {list.Count} elements).");
            }
            return [list[range.Start]];
        } else {
            // Up-to range access - clamp to available elements
            var count = range.End ?? list.Count;
            var actual = Math.Min(count, list.Count);

            if (actual <= 0) {
                return [];
            }

            return list.Take(actual).ToList();
        }
    }

    private static IReadOnlyList<string> SplitIntoSections(string remarks) {
        if (string.IsNullOrWhiteSpace(remarks)) {
            return [];
        }

        var dividerRegex = SectionDividerRegex();
        
        var sections = new List<string>();
        var commentMatches = dividerRegex.Matches(remarks);

        var lastIndex = 0;
        foreach (Match match in commentMatches) {
            if (match.Index > lastIndex) {
                var before = remarks[lastIndex..match.Index].Trim();
                if (!string.IsNullOrWhiteSpace(before)) {
                    sections.Add(before);
                }
            }

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < remarks.Length) {
            var remaining = remarks[lastIndex..].Trim();
            if (!string.IsNullOrWhiteSpace(remaining)) {
                sections.Add(remaining);
            }
        }

        return sections.Count == 0 ? [remarks.Trim()] : sections;
    }

    private static IReadOnlyList<string> FilterByTag(IReadOnlyList<string> content, string? tagArg) {
        if (tagArg == null) return content;

        var tagSplit = tagArg.Split(':');
        var tag = tagSplit[0].Trim().ToLowerInvariant();
        var label = tagSplit.Length > 1 ? tagSplit[1].Trim().ToLowerInvariant() : null;

        var output = new List<string>();
        foreach (var line in content) {
            var lineMatch = TagLabelRegex().Match(line);
            if (lineMatch.Success) {
                var sectionTag = lineMatch.Groups["tag"].Value.Trim().ToLowerInvariant();
                var sectionLabel = lineMatch.Groups["label"].Success ? lineMatch.Groups["label"].Value.Trim().ToLowerInvariant() : string.Empty;
                var sectionContent = lineMatch.Groups["content"].Value;

                Console.WriteLine($"{tag} - {label}: {sectionTag} {sectionLabel}: {sectionContent}");
                if (sectionTag == tag && (label == null || sectionLabel == label)) {
                    output.Add(line);
                }
            }
        }

        return output;
    }

    private static string RenderList(IEnumerable<string> lines) {
        var filtered = lines.Where(static line => !string.IsNullOrWhiteSpace(line)).ToArray();
        return filtered.Length == 0 ? string.Empty : string.Join(Environment.NewLine, filtered);
    }

    private static string NormalizeXrefs(string? text) {
        if (string.IsNullOrWhiteSpace(text)) {
            return string.Empty;
        }

        return XrefRegex().Replace(
            text,
            match => {
                var uid = match.Groups["uid"].Value;
                var inner = match.Groups["inner"].Value;

                if (string.IsNullOrWhiteSpace(inner)) {
                    return $"<xref:{uid}>";
                }

                return $"[{inner}](xref:{uid})";
            });
    }
}
