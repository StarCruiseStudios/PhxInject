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
    private const string CodeBlockPlaceholderFormat = "<<<DOCFX_CODE_BLOCK_{0}>>>";

    private static readonly DocFxPluginOptions Options = DocFxPluginOptions.Load();
    private static readonly Regex MarkdownCodeBlockRegex = new(
        @"```[\s\S]*?```",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex XmlCodeBlockRegex = new(
        @"<code>.*?</code>",
        RegexOptions.Compiled | RegexOptions.Singleline);

    /// <inheritdoc />
    public string Name => nameof(IdentifierXrefBuildStep);

    /// <inheritdoc />
    public int BuildOrder => -900;

    /// <inheritdoc />
    public IEnumerable<FileModel> Prebuild(ImmutableList<FileModel> models, IHostService host) {
        return models;
    }

    /// <inheritdoc />
    public void Build(FileModel model, IHostService host) {
        if (model.Content is not IDictionary<string, object> content) {
            return;
        }

        if (!content.TryGetValue("conceptual", out var conceptualObject) || conceptualObject is not string markdown) {
            return;
        }

        if (!Options.IdentifierXref.IdentifierRegex.IsMatch(markdown)) {
            return;
        }

        var rewritten = AutoLinkIdentifiers(markdown);
        content["conceptual"] = rewritten;
    }

    /// <inheritdoc />
    public void Postbuild(ImmutableList<FileModel> models, IHostService host) {
    }

    private static string AutoLinkIdentifiers(string markdown) {
        // Temporarily replace code blocks to prevent link conversion inside them.
        var codeBlocks = new List<string>();
        var result = ReplaceWithPlaceholders(markdown, MarkdownCodeBlockRegex, codeBlocks);
        result = ReplaceWithPlaceholders(result, XmlCodeBlockRegex, codeBlocks);

        result = Options.IdentifierXref.IdentifierRegex.Replace(
            result,
            match => {
                var identifier = match.Groups["identifier"].Value;
                var normalized = NormalizeIdentifier(identifier, Options.IdentifierXref);
                var uid = Options.IdentifierXref.LinkPrefix + normalized;
                return $"<xref href=\"{uid}?text={identifier}\" />";
            });

        return RestorePlaceholders(result, codeBlocks);
    }

    private static string ReplaceWithPlaceholders(string text, Regex regex, List<string> codeBlocks) {
        return regex.Replace(
            text,
            match => {
                codeBlocks.Add(match.Value);
                return string.Format(CodeBlockPlaceholderFormat, codeBlocks.Count - 1);
            });
    }

    private static string RestorePlaceholders(string text, IReadOnlyList<string> codeBlocks) {
        var result = text;
        for (var i = 0; i < codeBlocks.Count; i++) {
            var codeBlockPlaceholder = string.Format(CodeBlockPlaceholderFormat, i);
            result = result.Replace(codeBlockPlaceholder, codeBlocks[i], StringComparison.Ordinal);
        }

        return result;
    }

    private static string NormalizeIdentifier(string identifier, DocFxPluginOptions.IdentifierXrefOptions options) {
        var normalized = identifier;

        if (!string.IsNullOrEmpty(options.SpaceReplacement)) {
            normalized = normalized.Replace(" ", options.SpaceReplacement, StringComparison.Ordinal);
        }

        return options.IdentifierCase switch {
            DocFxPluginOptions.IdentifierCase.Lower => normalized.ToLowerInvariant(),
            DocFxPluginOptions.IdentifierCase.Upper => normalized.ToUpperInvariant(),
            _ => normalized
        };
    }
}
