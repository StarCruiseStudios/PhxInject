// -----------------------------------------------------------------------------
//  <copyright file="DocFxPluginOptions.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See https://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace PhxInject.DocFx.Plugins;

/// <summary>
///     Provides configuration for the DocFX plugin build steps.
/// </summary>
internal sealed class DocFxPluginOptions {
    private const string DefaultSnippetDirective = "api-snippet";
    private const string DefaultManifestFileName = ".manifest";
    private const string DefaultIdentifierPattern = "\\[(?<identifier>[A-Za-z_][A-Za-z0-9._ ]*)\\](?!\\()";
    private const string DefaultLinkPrefix = "link.";
    private const string DefaultSpaceReplacement = ".";

    private static readonly ImmutableHashSet<string> DefaultSnippetFields =
        ImmutableHashSet.Create(
            StringComparer.OrdinalIgnoreCase,
            "summary",
            "remarks",
            "syntax",
            "example",
            "seealso",
            "inheritance",
            "attributes");

    private DocFxPluginOptions(
        MetadataOptions metadata,
        SnippetOptions snippet,
        IdentifierXrefOptions identifierXref) {
        Metadata = metadata;
        Snippet = snippet;
        IdentifierXref = identifierXref;
    }

    /// <summary>
    ///     Gets the metadata configuration used by the snippet build step.
    /// </summary>
    public MetadataOptions Metadata { get; }

    /// <summary>
    ///     Gets the snippet directive configuration.
    /// </summary>
    public SnippetOptions Snippet { get; }

    /// <summary>
    ///     Gets the identifier xref configuration.
    /// </summary>
    public IdentifierXrefOptions IdentifierXref { get; }

    /// <summary>
    ///     Loads plugin configuration using the default, hardcoded values.
    /// </summary>
    public static DocFxPluginOptions Load() {
        return CreateDefault();
    }

    private static DocFxPluginOptions CreateDefault() {
        var baseDirectory = Directory.GetCurrentDirectory();
        var metadata = MetadataOptions.CreateDefault(baseDirectory);
        var snippet = SnippetOptions.CreateDefault();
        var identifierXref = IdentifierXrefOptions.CreateDefault();

        return new DocFxPluginOptions(metadata, snippet, identifierXref);
    }

    /// <summary>
    ///     Describes metadata input locations.
    /// </summary>
    internal sealed record MetadataOptions(
        string BaseDirectory,
        string? MetadataRoot,
        string? ManifestPath,
        string ManifestFileName,
        ImmutableArray<string> CandidateRoots) {
        public static MetadataOptions CreateDefault(string baseDirectory) {
            var candidates = ImmutableArray.CreateBuilder<string>();
            candidates.Add(baseDirectory);
            candidates.Add(Path.Combine(baseDirectory, "api"));
            candidates.Add(Path.GetFullPath(Path.Combine(baseDirectory, "..", "api")));

            var current = baseDirectory;
            for (var depth = 0; depth <= 6; depth++) {
                candidates.Add(Path.GetFullPath(Path.Combine(current, "src", "Phx.Inject.Generator", "bin", "docs")));
                current = Path.GetFullPath(Path.Combine(current, ".."));
            }

            return new MetadataOptions(
                baseDirectory,
                null,
                null,
                DefaultManifestFileName,
                candidates.ToImmutable());
        }
    }

    /// <summary>
    ///     Describes the snippet directive behavior.
    /// </summary>
    internal sealed record SnippetOptions(
        string DirectiveName,
        ImmutableHashSet<string> SupportedFields,
        Regex DirectiveRegex,
        Regex StartBlockRegex,
        Regex EndBlockRegex,
        Regex? RemarksSectionDividerRegex) {
        public static SnippetOptions CreateDefault() {
            var directivePattern = $"^\\s*:::\\s*{Regex.Escape(DefaultSnippetDirective)}\\s+(?<spec>.+?)\\s*$";
            var startPattern = $"^\\s*<!--\\s*{Regex.Escape(DefaultSnippetDirective)}:start\\s+.*-->\\s*$";
            var endPattern = $"^\\s*<!--\\s*{Regex.Escape(DefaultSnippetDirective)}:end\\s*-->\\s*$";

            var regexOptions = RegexOptions.Compiled | RegexOptions.Multiline;

            return new SnippetOptions(
                DefaultSnippetDirective,
                DefaultSnippetFields,
                new Regex(directivePattern, regexOptions),
                new Regex(startPattern, regexOptions),
                new Regex(endPattern, regexOptions),
                null);
        }
    }

    /// <summary>
    ///     Describes identifier-to-xref conversion behavior.
    /// </summary>
    internal sealed record IdentifierXrefOptions(
        Regex IdentifierRegex,
        string LinkPrefix,
        string SpaceReplacement,
        IdentifierCase IdentifierCase) {
        public static IdentifierXrefOptions CreateDefault() {
            return new IdentifierXrefOptions(
                new Regex(DefaultIdentifierPattern, RegexOptions.Compiled),
                DefaultLinkPrefix,
                DefaultSpaceReplacement,
                IdentifierCase.Lower);
        }
    }

    internal enum IdentifierCase {
        Preserve,
        Lower,
        Upper
    }

}
