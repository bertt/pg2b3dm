using System;
using System.Collections.Generic;
using System.Globalization;
using Npgsql;
using Wkx;

namespace B3dm.Tileset;

public static class FeatureSizeRepository
{
    /// <summary>
    /// Gets features with their bounding box diagonal size for a given tile extent.
    /// Returns list of (featureId, diagonalSize) tuples.
    /// </summary>
    public static List<FeatureSizeInfo> GetFeatureSizesInBox(NpgsqlConnection conn, string geometry_table, string geometry_column, Point from, Point to, string query, int source_epsg, bool keepProjection = false)
    {
        var fromX = from.X.Value.ToString(CultureInfo.InvariantCulture);
        var fromY = from.Y.Value.ToString(CultureInfo.InvariantCulture);
        var toX = to.X.Value.ToString(CultureInfo.InvariantCulture);
        var toY = to.Y.Value.ToString(CultureInfo.InvariantCulture);

        // Calculate diagonal size of the feature's bounding box using ST_Envelope
        // This gives a good approximation of feature size
        var sizeExpression = $@"
            SQRT(
                POWER(ST_XMax(ST_Envelope({geometry_column})) - ST_XMin(ST_Envelope({geometry_column})), 2) +
                POWER(ST_YMax(ST_Envelope({geometry_column})) - ST_YMin(ST_Envelope({geometry_column})), 2)
            )";

        var whereClause = keepProjection ?
            $"ST_Centroid(ST_Envelope({geometry_column})) && ST_MakeEnvelope({fromX}, {fromY}, {toX}, {toY}, {source_epsg})" :
            $"ST_Centroid(ST_Envelope({geometry_column})) && st_transform(ST_MakeEnvelope({fromX}, {fromY}, {toX}, {toY}, 4326), {source_epsg})";

        if (!string.IsNullOrEmpty(query)) {
            whereClause += $" {query}";
        }

        var sql = $"SELECT {sizeExpression} as feature_size FROM {geometry_table} WHERE {whereClause}";

        var result = new List<FeatureSizeInfo>();
        conn.Open();
        var cmd = new NpgsqlCommand(sql, conn);
        var reader = cmd.ExecuteReader();
        
        var index = 0;
        while (reader.Read()) {
            var size = reader.IsDBNull(0) ? 0.0 : reader.GetDouble(0);
            result.Add(new FeatureSizeInfo(index, size));
            index++;
        }
        
        reader.Close();
        conn.Close();
        
        return result;
    }

    /// <summary>
    /// Gets the maximum feature size in a bounding box.
    /// </summary>
    public static double GetMaxFeatureSizeInBox(NpgsqlConnection conn, string geometry_table, string geometry_column, Point from, Point to, string query, int source_epsg, bool keepProjection = false)
    {
        var fromX = from.X.Value.ToString(CultureInfo.InvariantCulture);
        var fromY = from.Y.Value.ToString(CultureInfo.InvariantCulture);
        var toX = to.X.Value.ToString(CultureInfo.InvariantCulture);
        var toY = to.Y.Value.ToString(CultureInfo.InvariantCulture);

        var sizeExpression = $@"
            MAX(SQRT(
                POWER(ST_XMax(ST_Envelope({geometry_column})) - ST_XMin(ST_Envelope({geometry_column})), 2) +
                POWER(ST_YMax(ST_Envelope({geometry_column})) - ST_YMin(ST_Envelope({geometry_column})), 2)
            ))";

        var whereClause = keepProjection ?
            $"ST_Centroid(ST_Envelope({geometry_column})) && ST_MakeEnvelope({fromX}, {fromY}, {toX}, {toY}, {source_epsg})" :
            $"ST_Centroid(ST_Envelope({geometry_column})) && st_transform(ST_MakeEnvelope({fromX}, {fromY}, {toX}, {toY}, 4326), {source_epsg})";

        if (!string.IsNullOrEmpty(query)) {
            whereClause += $" {query}";
        }

        var sql = $"SELECT {sizeExpression} FROM {geometry_table} WHERE {whereClause}";

        conn.Open();
        var cmd = new NpgsqlCommand(sql, conn);
        var reader = cmd.ExecuteReader();
        reader.Read();
        var maxSize = reader.IsDBNull(0) ? 0.0 : reader.GetDouble(0);
        reader.Close();
        conn.Close();

        return maxSize;
    }

    /// <summary>
    /// Gets features grouped by their appropriate zoom level based on size.
    /// Features that are larger than minSizeRatio * tileDiagonal appear at lower zoom levels.
    /// </summary>
    public static FeatureSizeStats GetFeatureSizeStats(NpgsqlConnection conn, string geometry_table, string geometry_column, Point from, Point to, string query, int source_epsg, bool keepProjection, double tileDiagonal, double minSizeRatio)
    {
        var features = GetFeatureSizesInBox(conn, geometry_table, geometry_column, from, to, query, source_epsg, keepProjection);
        
        var stats = new FeatureSizeStats();
        stats.TotalCount = features.Count;
        
        if (features.Count == 0) {
            return stats;
        }

        var threshold = tileDiagonal * minSizeRatio;
        
        foreach (var feature in features) {
            if (feature.Size >= threshold) {
                stats.LargeFeaturesCount++;
            }
            else {
                stats.SmallFeaturesCount++;
            }
            
            if (feature.Size > stats.MaxSize) {
                stats.MaxSize = feature.Size;
            }
        }
        
        return stats;
    }
}

public class FeatureSizeInfo
{
    public int Index { get; set; }
    public double Size { get; set; }

    public FeatureSizeInfo(int index, double size)
    {
        Index = index;
        Size = size;
    }
}

public class FeatureSizeStats
{
    public int TotalCount { get; set; }
    public int LargeFeaturesCount { get; set; }
    public int SmallFeaturesCount { get; set; }
    public double MaxSize { get; set; }
}
