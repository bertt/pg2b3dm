# AGENTS.md - pg2b3dm

This document provides guidelines for AI coding agents working on the pg2b3dm codebase.

## Project Overview

**pg2b3dm** is a .NET 8.0 console tool for converting PostGIS 3D geometries to [3D Tiles](https://github.com/AnalyticalGraphicsInc/3d-tiles) format. Generated tiles can be visualized in Cesium JS, QGIS, ArcGIS Pro, and other 3D Tiles viewers.

## Project Structure

```
src/
├── pg2b3dm/              # Main CLI application (entry point)
├── b3dm.tileset/         # Tileset generation library
├── wkb2gltf.core/        # WKB geometry to glTF/b3dm conversion
├── b3dm.tileset.tests/   # Tests for tileset functionality
├── wkb2gltf.core.tests/  # Tests for glTF conversion
└── pg2b3dm.database.tests/ # Database integration tests
```

## Build Commands

```bash
# Build the solution
cd src
dotnet build --configuration Release

# Build specific project
dotnet build src/pg2b3dm/pg2b3dm.csproj

# Create publishable executable
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true
```

## Test Commands

```bash
# Run all tests
cd src
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal

# Run a single test project
dotnet test src/wkb2gltf.core.tests/wkb2gltf.tests.csproj
dotnet test src/b3dm.tileset.tests/b3dm.tileset.tests.csproj
dotnet test src/pg2b3dm.database.tests/pg2b3dm.database.tests.csproj

# Run a single test by name (use --filter)
dotnet test --filter "FullyQualifiedName~GlbCreatorTests.CreateGlbWithDefaultColor"
dotnet test --filter "FullyQualifiedName~TreeSerializerTests"

# Run tests matching a pattern
dotnet test --filter "Name~Shader"
```

## Code Style Guidelines

### EditorConfig Settings (src/.editorconfig)

The project uses EditorConfig for code style enforcement:

- **Indentation**: Soft tabs (spaces)
- **Brace Style**: Allman style for methods and types (braces on new line)
- **Using Directives**: `System.*` first, sorted alphabetically

### Type Declarations

- Prefer `var` for built-in types and when type is apparent
- Use language keywords (`int`, `string`) over framework types (`Int32`, `String`)

```csharp
// Good
var count = 10;
var name = "test";
var geometries = new List<GeometryRecord>();

// Avoid
int count = 10;
List<GeometryRecord> geometries = new List<GeometryRecord>();
```

### Naming Conventions

- **Classes**: PascalCase (`GeometryRepository`, `CesiumTiler`)
- **Methods**: PascalCase (`GetGeometries`, `CreateSubtreeFiles`)
- **Local Variables**: camelCase (`batchId`, `connectionString`)
- **Private Fields**: camelCase, no prefix (`password`, not `_password` or `m_password`)
- **Constants**: PascalCase
- **Parameters**: camelCase

### Namespace Declarations

Use file-scoped namespaces (C# 10+):

```csharp
namespace B3dm.Tileset;

public static class CesiumTiler
{
    // ...
}
```

### Import Organization

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
// Other System.* namespaces...
using B3dm.Tileset;           // Project namespaces
using CommandLine;             // Third-party
using Npgsql;
using SharpGLTF.Schema2;
```

### Method Style

- Prefer block bodies for methods (not expression-bodied)
- No `this.` prefix for fields/methods

```csharp
// Good
public static byte[] GetGlb(List<List<Triangle>> triangles)
{
    var materialCache = new MaterialsCache();
    // ...
    return bytes;
}

// Avoid expression body for complex methods
public static byte[] GetGlb(...) => ...;
```

### Bracing Style

Allman style for methods and types:

```csharp
public class MyClass
{
    public void MyMethod()
    {
        if (condition) {
            // K&R style for control structures is acceptable
        }
        else {
            // ...
        }
    }
}
```

### Error Handling

- Project uses `TreatWarningsAsErrors: true` - all warnings must be resolved
- Use null checks with pattern matching where appropriate
- Prefer early returns for validation

```csharp
if (model.LogicalNodes.Count == 0) {
    return null;
}
```

### Testing Conventions

- Test framework: **NUnit 4.x**
- Test class naming: `{ClassName}Tests` (e.g., `GlbCreatorTests`)
- Test method naming: Descriptive names, often `{Method}_{Scenario}_{Expected}`
- Use `Assert.That()` with constraint model

```csharp
[Test]
public void CreateGlbWithDefaultColor()
{
    // Arrange
    var wkt = "MULTIPOLYGON Z (...)";
    var g = Geometry.Deserialize<WktSerializer>(wkt);
    var triangles = GeometryProcessor.GetTriangles(g, 100);

    // Act
    var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });

    // Assert
    Assert.That(bytes, Is.Not.Null);
    Assert.That(model.LogicalMeshes[0].Primitives.Count, Is.EqualTo(1));
}
```

### Database Access Patterns

- Use `NpgsqlConnection` for PostgreSQL/PostGIS access
- Open connections just before use, close immediately after
- Use parameterized queries or string interpolation for dynamic SQL

```csharp
conn.Open();
var cmd = new NpgsqlCommand(sql, conn);
var reader = cmd.ExecuteReader();
// ... process results
reader.Close();
conn.Close();
```

### Key Dependencies

| Package | Purpose |
|---------|---------|
| CommandLineParser | CLI argument parsing |
| Npgsql | PostgreSQL database access |
| SharpGLTF.Toolkit | glTF file generation |
| SharpGLTF.Ext.3DTiles | 3D Tiles extensions |
| b3dm-tile | b3dm file generation |
| Wkx | WKB geometry parsing |
| Newtonsoft.Json | JSON serialization |
| NUnit | Testing framework |

### Common Patterns

**Geometry Processing Pipeline:**
```csharp
var g = Geometry.Deserialize<WktSerializer>(wkt);
var triangles = GeometryProcessor.GetTriangles(g, batchId);
var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });
```

**File Output:**
```csharp
var json = JsonConvert.SerializeObject(tileset, Formatting.Indented, 
    new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
File.WriteAllText(path, json);
```

## CI/CD

GitHub Actions workflows:
- **main.yml**: Builds and tests on every push/PR
- **release.yml**: Creates releases, publishes to NuGet and Docker Hub on version tags

## Additional Notes

- Target framework: .NET 8.0
- The tool can be installed as a .NET global tool: `dotnet tool install -g pg2b3dm`
- Docker image available: `geodan/pg2b3dm`
- All projects have `TreatWarningsAsErrors=true` - ensure code compiles without warnings
