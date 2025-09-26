# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Loqui is a C# class generation library that provides functionality like copy masks, defaulting masks, serialization, interface generation, and other tedious bits of class and interface definitions. It targets .NET 8, .NET 9, and .NET Standard 2.0.

## Project Structure

The solution consists of three main projects:

- **Loqui** (`Loqui/`): Core runtime library containing base classes, interfaces, masks, and utilities that generated code depends on
- **Loqui.Generation** (`Loqui.Generation/`): Code generation engine using Roslyn analyzers to generate C# classes from XML schema definitions
- **Loqui.Tests** (`Loqui.Tests/`): xUnit test suite for validating the library functionality

## Development Commands

### Building
```bash
# Clean and restore packages
dotnet clean -c Release && dotnet nuget locals all --clear
dotnet restore

# Build the solution
dotnet build -c Release --no-restore

# Build with package generation disabled (for CI)
dotnet build -c Release --no-restore /p:GeneratePackageOnBuild=false
```

### Testing
```bash
# Run all tests
dotnet test -c Release --no-build

# Run tests for a specific project
dotnet test Loqui.Tests -c Release
```

### Packaging
```bash
# Pack all projects with symbols
dotnet pack -c Release --no-build --no-restore -o out --include-symbols -p:SymbolPackageFormat=snupkg
```

## Code Generation Architecture

The library uses a two-tier architecture:

1. **Schema Definition**: Classes are defined using XML schemas (see `LoquiSource.xsd`)
2. **Code Generation**: The `Loqui.Generation` project processes these schemas using Roslyn to generate:
   - Data classes with copy constructors
   - Interface definitions
   - Mask classes for selective operations
   - Translation/serialization support

Key generation components:
- `ObjectGeneration.cs`: Main orchestrator for class generation
- `ProtocolGeneration.cs`: Manages protocol-level generation
- `Fields/`: Field-specific generation logic
- `Modules/`: Pluggable generation modules (masks, translation, etc.)

## Configuration

- **Central Package Management**: Uses Directory.Packages.props for NuGet version management
- **Multi-targeting**: Projects target net8.0, net9.0, and netstandard2.0
- **Nullable Reference Types**: Enabled in core library, disabled in generation project
- **Code Analysis**: Custom diagnostic rules defined in .editorconfig

## Releases

### Release Process
- Create release tags using semantic versioning format: `<major>.<minor>.<patch>`
- Always include the patch number, even if it's zero (e.g., `3.1.0`, not `3.1`)
- **Do not prefix with `v`** (e.g., use `3.1.0`, not `v3.1.0`)
- This format is required for GitVersion compatibility

#### Creating GitHub Release Drafts
1. Find the last release tag: `git tag --sort=-version:refname`
2. Get commits since last release: `git log --oneline <last-tag>..HEAD`
3. Construct release notes by categorizing commits:
   - **Enhancements**: New features, performance improvements, major changes
   - **Bug Fixes**: Bug fixes and corrections
   - **Testing & Documentation**: Test additions, documentation updates
4. Create draft release: `gh release create <version> --draft --title "<version>" --notes "<release-notes>"`
5. Include full changelog link: `**Full Changelog**: https://github.com/Noggog/Loqui/compare/<last-tag>...<new-tag>`

## Important Notes

- Generated packages are output to `../nupkg` directory
- Pre-build targets clear local NuGet cache to ensure clean package testing
- Uses GitVersion for automatic version management
- Source link and symbol packages are configured for debugging support