using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace pg2b3dm.ui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _host = "localhost";

    [ObservableProperty]
    private string _port = "5432";

    [ObservableProperty]
    private string _database = Environment.UserName;

    [ObservableProperty]
    private string _username = Environment.UserName;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _geometryTable = string.Empty;

    [ObservableProperty]
    private string _geometryColumn = "geom";

    [ObservableProperty]
    private string _attributeColumns = string.Empty;

    [ObservableProperty]
    private string _query = string.Empty;

    [ObservableProperty]
    private string _output = "output";

    [ObservableProperty]
    private string _copyright = string.Empty;

    [ObservableProperty]
    private string _defaultColor = "#FFFFFF";

    [ObservableProperty]
    private string _defaultMetallicRoughness = "#008000";

    [ObservableProperty]
    private bool _doubleSided = true;

    [ObservableProperty]
    private string _defaultAlphaMode = "OPAQUE";

    [ObservableProperty]
    private bool _createGltf = true;

    [ObservableProperty]
    private string _radiusColumn = string.Empty;

    [ObservableProperty]
    private string _shadersColumn = string.Empty;

    [ObservableProperty]
    private string _tilesetVersion = string.Empty;

    [ObservableProperty]
    private int _maxFeaturesPerTile = 1000;

    [ObservableProperty]
    private string _lodColumn = string.Empty;

    [ObservableProperty]
    private double _geometricError = 2000;

    [ObservableProperty]
    private double _geometricErrorFactor = 2;

    [ObservableProperty]
    private bool _useImplicitTiling = true;

    [ObservableProperty]
    private bool _addOutlines = false;

    [ObservableProperty]
    private string _refinement = "ADD";

    [ObservableProperty]
    private bool _skipCreateTiles = false;

    [ObservableProperty]
    private bool _keepProjection = false;

    [ObservableProperty]
    private string _subdivision = "QUADTREE";

    [ObservableProperty]
    private string _outputLog = string.Empty;

    [ObservableProperty]
    private bool _isRunning = false;

    public List<string> AlphaModes { get; } = new() { "OPAQUE", "BLEND", "MASK" };
    public List<string> RefinementTypes { get; } = new() { "ADD", "REPLACE" };
    public List<string> SubdivisionSchemes { get; } = new() { "QUADTREE", "OCTREE" };

    [RelayCommand]
    private async Task RunPg2B3dm()
    {
        if (IsRunning)
            return;

        if (string.IsNullOrWhiteSpace(GeometryTable))
        {
            AppendLog("Error: Geometry table is required.");
            return;
        }

        IsRunning = true;
        OutputLog = string.Empty;

        try
        {
            await Task.Run(() => ExecutePg2B3dm());
        }
        catch (Exception ex)
        {
            AppendLog($"Error: {ex.Message}");
            AppendLog($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            IsRunning = false;
        }
    }

    private void ExecutePg2B3dm()
    {
        try
        {
            // Build the command-line arguments
            var args = new StringBuilder();
            args.Append($"-h {Host} ");
            args.Append($"-p {Port} ");
            args.Append($"-d {Database} ");
            args.Append($"-U {Username} ");
            args.Append($"-t {GeometryTable} ");
            args.Append($"-c {GeometryColumn} ");
            args.Append($"-o {Output} ");

            if (!string.IsNullOrWhiteSpace(AttributeColumns))
                args.Append($"-a {AttributeColumns} ");

            if (!string.IsNullOrWhiteSpace(Query))
                args.Append($"-q \"{Query}\" ");

            if (!string.IsNullOrWhiteSpace(Copyright))
                args.Append($"--copyright \"{Copyright}\" ");

            args.Append($"--default_color {DefaultColor} ");
            args.Append($"--default_metallic_roughness {DefaultMetallicRoughness} ");
            args.Append($"--double_sided {DoubleSided.ToString().ToLower()} ");
            args.Append($"--default_alpha_mode {DefaultAlphaMode} ");
            args.Append($"--create_gltf {CreateGltf.ToString().ToLower()} ");

            if (!string.IsNullOrWhiteSpace(RadiusColumn))
                args.Append($"--radiuscolumn {RadiusColumn} ");

            if (!string.IsNullOrWhiteSpace(ShadersColumn))
                args.Append($"--shaderscolumn {ShadersColumn} ");

            if (!string.IsNullOrWhiteSpace(TilesetVersion))
                args.Append($"--tileset_version {TilesetVersion} ");

            args.Append($"--max_features_per_tile {MaxFeaturesPerTile} ");

            if (!string.IsNullOrWhiteSpace(LodColumn))
                args.Append($"-l {LodColumn} ");

            args.Append($"-g {GeometricError} ");
            args.Append($"--geometricerrorfactor {GeometricErrorFactor} ");
            args.Append($"--use_implicit_tiling {UseImplicitTiling.ToString().ToLower()} ");
            args.Append($"--add_outlines {AddOutlines.ToString().ToLower()} ");
            args.Append($"-r {Refinement} ");
            args.Append($"--skip_create_tiles {SkipCreateTiles.ToString().ToLower()} ");
            args.Append($"--keep_projection {KeepProjection.ToString().ToLower()} ");
            args.Append($"--subdivision {Subdivision} ");

            AppendLog("Starting pg2b3dm...");
            AppendLog($"Command: pg2b3dm {args}");
            AppendLog("");

            // Set password environment variable if provided
            if (!string.IsNullOrWhiteSpace(Password))
            {
                Environment.SetEnvironmentVariable("PGPASSWORD", Password);
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project ../pg2b3dm/pg2b3dm.csproj -- {args}",
                WorkingDirectory = Path.GetDirectoryName(typeof(MainWindowViewModel).Assembly.Location) ?? Directory.GetCurrentDirectory(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    AppendLog(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    AppendLog($"ERROR: {e.Data}");
                }
            };

            process.Start();
            
            // If password is provided, send it to stdin for the password prompt
            if (!string.IsNullOrWhiteSpace(Password))
            {
                process.StandardInput.WriteLine(Password);
                process.StandardInput.Flush();
            }
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            process.WaitForExit();

            // Clean up password from environment
            if (!string.IsNullOrWhiteSpace(Password))
            {
                Environment.SetEnvironmentVariable("PGPASSWORD", null);
            }

            AppendLog("");
            if (process.ExitCode == 0)
            {
                AppendLog("Process completed successfully!");
            }
            else
            {
                AppendLog($"Process exited with code {process.ExitCode}");
            }
        }
        catch (Exception ex)
        {
            AppendLog($"Error during processing: {ex.Message}");
            if (ex.InnerException != null)
            {
                AppendLog($"Inner error: {ex.InnerException.Message}");
            }
        }
    }

    private void AppendLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        OutputLog += $"[{timestamp}] {message}{Environment.NewLine}";
    }
}
