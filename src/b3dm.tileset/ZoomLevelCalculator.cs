using System;

namespace B3dm.Tileset;

/// <summary>
/// Calculator for determining zoom levels based on feature size.
/// Implements a tippecanoe-style approach where larger features appear at lower zoom levels
/// and smaller features appear at higher zoom levels.
/// </summary>
public static class ZoomLevelCalculator
{
    /// <summary>
    /// Calculates the minimum zoom level at which a feature should appear based on its size.
    /// A feature appears at the lowest zoom level where its size is at least minSizeRatio of the tile size.
    /// </summary>
    /// <param name="featureSize">The diagonal size of the feature's bounding box</param>
    /// <param name="rootTileSize">The diagonal size of the root tile (zoom level 0)</param>
    /// <param name="minSizeRatio">Minimum ratio of feature size to tile size (default 0.01 = 1%)</param>
    /// <returns>The minimum zoom level at which the feature should appear</returns>
    public static int GetMinZoomLevel(double featureSize, double rootTileSize, double minSizeRatio = 0.01)
    {
        if (featureSize <= 0 || rootTileSize <= 0) {
            return 0;
        }

        // At each zoom level, tile size is halved
        // Feature should appear at the lowest level where: featureSize >= tileSizeAtLevel * minSizeRatio
        // tileSizeAtLevel = rootTileSize / 2^level
        // So: featureSize >= (rootTileSize / 2^level) * minSizeRatio
        // 2^level >= (rootTileSize * minSizeRatio) / featureSize
        // level >= log2((rootTileSize * minSizeRatio) / featureSize)
        
        var ratio = (rootTileSize * minSizeRatio) / featureSize;
        
        if (ratio <= 1) {
            return 0; // Feature is large enough for root level
        }
        
        var level = (int)Math.Ceiling(Math.Log2(ratio));
        return Math.Max(0, level);
    }

    /// <summary>
    /// Calculates the tile diagonal size at a given zoom level.
    /// </summary>
    public static double GetTileSizeAtZoom(double rootTileSize, int zoomLevel)
    {
        return rootTileSize / Math.Pow(2, zoomLevel);
    }

    /// <summary>
    /// Determines if a feature should be included at a given zoom level based on its size.
    /// </summary>
    /// <param name="featureSize">The diagonal size of the feature's bounding box</param>
    /// <param name="tileDiagonal">The diagonal size of the tile at the current zoom level</param>
    /// <param name="minSizeRatio">Minimum ratio of feature size to tile size</param>
    /// <returns>True if the feature should be included at this zoom level</returns>
    public static bool ShouldIncludeAtZoom(double featureSize, double tileDiagonal, double minSizeRatio)
    {
        // Feature should appear if its size is at least minSizeRatio of the tile size
        return featureSize >= tileDiagonal * minSizeRatio;
    }

    /// <summary>
    /// Calculates the diagonal of a bounding box.
    /// </summary>
    public static double CalculateDiagonal(double xMin, double yMin, double xMax, double yMax)
    {
        var dx = xMax - xMin;
        var dy = yMax - yMin;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
