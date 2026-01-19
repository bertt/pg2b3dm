using System.Collections.Generic;

namespace B3dm.Tileset.settings;

public class TilingSettings
{
    public Wkx.BoundingBox BoundingBox { get; set; } 

    public bool CreateGltf { get; set; } = true;

    public bool KeepProjection { get; set; } = false;

    public bool SkipCreateTiles { get; set; } = false;

    public int MaxFeaturesPerTile { get; set; } = 1000; 

    public bool UseImplicitTiling { get; set; } = true;

    public List<int> Lods { get; set; }

    /// <summary>
    /// Minimum feature size ratio for size-based tiling (tippecanoe-style).
    /// When > 0, features are assigned to zoom levels based on their size.
    /// Value represents minimum feature diagonal as fraction of tile diagonal (e.g., 0.01 = 1%).
    /// When 0, traditional count-based tiling is used.
    /// </summary>
    public double MinFeatureSizeRatio { get; set; } = 0.0;
}
