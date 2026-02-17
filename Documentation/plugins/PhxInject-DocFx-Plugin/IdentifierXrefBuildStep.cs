// -----------------------------------------------------------------------------
//  <copyright file="IdentifierXrefBuildStep.cs" company="Star Cruise Studios LLC">
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

/// <summary>
///     DocFX build step that converts <c>[identifier]</c> shorthand notation to xref links.
/// </summary>
/// <remarks>
///     This step runs after <see cref="ApiSnippetReplacementBuildStep" /> to ensure snippets are expanded
///     before identifier links are processed. It converts <c>[Identifier]</c> patterns (not followed by parentheses)
///     into xref references based on the configured prefix and normalization rules, while preserving code blocks.
/// </remarks>
[Export("ConceptualDocumentProcessor", typeof(IDocumentBuildStep))]
public sealed partial class IdentifierXrefBuildStep : IDocumentBuildStep {
    [GeneratedRegex("\\[(?<identifier>[A-Za-z_][A-Za-z0-9._ ]*)\\](?!\\()", RegexOptions.Compiled)]
    private static partial Regex IdentifierRegex();
    
    [GeneratedRegex(@"```[\s\S]*?```", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex MarkdownCodeBlockRegex();

    [GeneratedRegex("<code>.*?</code>", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex XmlCodeBlockRegex();
    
    private const string XrefLinkPattern = @"<xref href=""link.{0}?text={1}"" />";

    /// <inheritdoc cref="IDocumentBuildStep.Name"/>
    public string Name => nameof(IdentifierXrefBuildStep);

    /// <inheritdoc cref="IDocumentBuildStep.BuildOrder"/>
    public int BuildOrder => -900;

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

        if (!IdentifierRegex().IsMatch(markdown)) {
            return;
        }

        content["conceptual"] = RewriteIdentifiers(markdown);
    }

    /// <inheritdoc />
    public void Postbuild(ImmutableList<FileModel> models, IHostService host) { }

    private static string RewriteIdentifiers(string markdown) {
        // Temporarily replace code blocks to prevent link conversion inside them.
        var codeBlocks = new List<string>();
        var result = markdown.ReplaceWithCodeBlockPlaceholders(MarkdownCodeBlockRegex(), codeBlocks);
        result = result.ReplaceWithCodeBlockPlaceholders(XmlCodeBlockRegex(), codeBlocks);

        result = IdentifierRegex().Replace(
            result,
            match => {
                var identifier = match.Groups["identifier"].Value;
                var uid = identifier
                    .Replace(" ", ".", StringComparison.Ordinal)
                    .ToLowerInvariant();
                return string.Format(XrefLinkPattern, uid, identifier);
            });

        return result.RestorePlaceholders(codeBlocks);
    }
}

internal static class PlaceholderExtensions {
    private const string CodeBlockPlaceholderFormat = "<<<DOCFX_CODE_BLOCK_{0}>>>";
    
    internal static string ReplaceWithCodeBlockPlaceholders(this string text, Regex regex, List<string> codeBlocks) {
        return regex.Replace(
            text,
            match => {
                codeBlocks.Add(match.Value);
                return string.Format(CodeBlockPlaceholderFormat, codeBlocks.Count - 1);
            });
    }
    
    internal static string RestorePlaceholders(this string text, IReadOnlyList<string> codeBlocks) {
        var result = text;
        for (var i = 0; i < codeBlocks.Count; i++) {
            var codeBlockPlaceholder = string.Format(CodeBlockPlaceholderFormat, i);
            result = result.Replace(codeBlockPlaceholder, codeBlocks[i], StringComparison.Ordinal);
        }

        return result;
    }
}