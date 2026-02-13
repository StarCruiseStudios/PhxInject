# Phx.Inject.Generator API Documentation

This directory contains auto-generated API documentation for the Phx.Inject.Generator package.

## Generation

The API documentation is automatically generated during the build process using [DocFX](https://dotnet.github.io/docfx/). The documentation is generated from XML documentation comments in the source code.

## Files

- `*.yml` - YAML files containing structured API documentation for each type
- `toc.yml` - Table of contents for the API documentation
- `.manifest` - Manifest file used by DocFX

## Building the Documentation Site

To generate the full HTML documentation site:

```bash
dotnet docfx build docfx.json
```

This will generate a static website in `Documentation/API/_site/` that can be hosted or viewed locally.

## Viewing the Documentation

You can serve the documentation locally:

```bash
dotnet docfx docfx.json --serve
```

Then open your browser to `http://localhost:8080` to view the documentation.
