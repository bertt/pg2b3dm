# pg2b3dm UI

A graphical user interface for the pg2b3dm tool built with Avalonia UI.

## Overview

pg2b3dm UI provides a user-friendly interface to configure and run pg2b3dm, a tool for converting 3D geometries from PostGIS to 3D Tiles.

## Features

- **Complete Configuration**: Access all pg2b3dm command-line options through an intuitive interface
- **Password Masking**: Securely enter database passwords with masked input
- **Real-time Output Logs**: View pg2b3dm output in real-time with timestamped log entries
- **Organized Settings**: Settings are grouped into logical sections:
  - Database Connection
  - Geometry Settings
  - Advanced Options (expandable)

## Building and Running

### Prerequisites

- .NET 8.0 SDK or later
- All dependencies for pg2b3dm (see main README)

### Building

```bash
cd src/pg2b3dm.ui
dotnet build
```

### Running

```bash
cd src/pg2b3dm.ui
dotnet run
```

## Using the UI

1. **Database Connection**:
   - Enter your PostgreSQL connection details
   - Host, Port, Database, Username
   - Password (masked with ● symbols for security)
   - Leave password empty if using trusted authentication

2. **Geometry Settings**:
   - **Table** (required): Specify the database table (e.g., `schema.table`)
   - **Geometry Column**: Column containing the geometry data (default: `geom`)
   - **Attributes**: Comma-separated list of attribute columns to include
   - **Query**: Optional WHERE clause to filter data
   - **Output Path**: Directory where tiles will be created (default: `output`)

3. **Advanced Options** (expandable):
   - Material settings (colors, metallic/roughness)
   - Tiling options (implicit tiling, subdivision scheme)
   - Performance settings (max features per tile, geometric error)
   - Various checkboxes for additional features

4. **Running**:
   - Click "Run pg2b3dm" to start the conversion process
   - Monitor progress in the Output Log panel
   - The button will show "Running..." while processing

## Configuration Options

All pg2b3dm command-line options are available in the UI:

- Database connection parameters
- Geometry column and table selection
- Attribute columns
- Query filtering
- Output directory
- Color settings
- Material properties (metallic/roughness, alpha mode)
- Tiling parameters (geometric error, subdivision scheme)
- Advanced features (LOD, outlines, implicit tiling)

## Output Log

The output log displays:
- Timestamped messages
- Progress updates from pg2b3dm
- Error messages
- Completion status

The log uses a dark theme with monospace font for easy reading.

## Technical Details

- Built with Avalonia UI 11.3.8
- Uses MVVM pattern with CommunityToolkit.Mvvm
- Runs pg2b3dm via dotnet run command
- Cross-platform compatible (Windows, macOS, Linux)

## Password Security

The password field uses a masked input (showing ● symbols) to protect sensitive information. The password is:
- Not stored in any configuration file
- Only used when creating the database connection
- Cleared from memory after the process completes
- Can be left empty for trusted authentication or when using environment variables
