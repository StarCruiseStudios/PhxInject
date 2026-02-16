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
///     into <c>&lt;xref href="link.identifier"&gt;</c> references, while preserving code blocks.
/// </remarks>
[Export("ConceptualDocumentProcessor", typeof(IDocumentBuildStep))]
public sealed partial class IdentifierXrefBuildStep : IDocumentBuildStep
{
    /// <inheritdoc />
    public string Name => nameof(IdentifierXrefBuildStep);

    /// <inheritdoc />
    public int BuildOrder => -900;

    /// <inheritdoc />
    public IEnumerable<FileModel> Prebuild(ImmutableList<FileModel> models, IHostService host)
    {
        return models;
    }

    /// <inheritdoc />
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

        var rewritten = AutoLinkIdentifiers(markdown);
        content["conceptual"] = rewritten;
    }

    /// <inheritdoc />
    public void Postbuild(ImmutableList<FileModel> models, IHostService host)
    {
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

    [GeneratedRegex("\\[(?<identifier>[A-Za-z_][A-Za-z0-9._ ]*)\\](?!\\()", RegexOptions.Compiled)]
    private static partial Regex IdentifierLinkRegex();
}
