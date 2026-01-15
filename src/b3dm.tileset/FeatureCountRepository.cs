using Npgsql;
using Wkx;

namespace B3dm.Tileset;

public static class FeatureCountRepository
{
    public static int CountFeaturesInBox(NpgsqlConnection conn, string geometry_table, string geometry_column, Point from, Point to, string query, int source_epsg, bool keepProjection = false)
    {
        var select = $"COUNT({geometry_column})";
        var where = GeometryRepository.GetWhere(geometry_column, from, to, query, source_epsg, keepProjection);

        var sql = $"SELECT {select} FROM {geometry_table} WHERE {where}";
        conn.Open();
        var cmd = new NpgsqlCommand(sql, conn);
        var reader = cmd.ExecuteReader();
        reader.Read();
        var count = reader.GetInt32(0);
        reader.Close();
        conn.Close();
        return count;
    }

    public static double GetMaxObjectSizeInBox(NpgsqlConnection conn, string geometry_table, string geometry_column, Point from, Point to, string query, int source_epsg, bool keepProjection = false)
    {
        var where = GeometryRepository.GetWhere(geometry_column, from, to, query, source_epsg, keepProjection);
        
        // Calculate max of width and height for each feature, then get the maximum value
        // Width = ST_XMax(envelope) - ST_XMin(envelope)
        // Height = ST_YMax(envelope) - ST_YMin(envelope)
        // Size = GREATEST(width, height)
        var envelope = $"ST_Envelope({geometry_column})";
        var width = $"ST_XMax({envelope}) - ST_XMin({envelope})";
        var height = $"ST_YMax({envelope}) - ST_YMin({envelope})";
        var objectSize = $"GREATEST({width}, {height})";
        var select = $"SELECT COALESCE(MAX({objectSize}), 0)";
        var sql = $"{select} FROM {geometry_table} WHERE {where}";
        
        conn.Open();
        var cmd = new NpgsqlCommand(sql, conn);
        var reader = cmd.ExecuteReader();
        reader.Read();
        var maxSize = reader.GetDouble(0);
        reader.Close();
        conn.Close();
        return maxSize;
    }
}
