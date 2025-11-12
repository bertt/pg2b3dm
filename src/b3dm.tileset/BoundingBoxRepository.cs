using System.Data;
using Wkx;

namespace B3dm.Tileset;

public static class BoundingBoxRepository
{
    public static (BoundingBox bbox, double zmin, double zmax) GetBoundingBoxForTable(IDbConnection conn, string geometry_table, string geometry_column, bool keepProjection = false, string query = "")
    {
        var select = $"SELECT st_xmin(geom1),st_ymin(geom1), st_xmax(geom1), st_ymax(geom1), st_zmin(geom1), st_zmax(geom1) ";
        var geom = keepProjection?
            $"(select ST_3DExtent({geometry_column})":
            $"(select st_transform(ST_3DExtent({ geometry_column}), 4979)";
        var sqlBounds = $"{select} FROM {geom} as geom1 from {geometry_table} {query}) as t";
        var bbox3d = GetBounds(conn, sqlBounds);
        return bbox3d;
    }

    public static Point GetCenterInWgs84(IDbConnection conn, string geometry_table, string geometry_column, int source_epsg, string query = "")
    {
        // Get the center point of the bounding box transformed to WGS84 (EPSG:4326)
        var sql = $@"
            SELECT 
                ST_X(center_wgs84) as lon,
                ST_Y(center_wgs84) as lat
            FROM (
                SELECT ST_Transform(
                    ST_Centroid(ST_3DExtent({geometry_column})),
                    4326
                ) as center_wgs84
                FROM {geometry_table}
                {query}
            ) as subquery";
        
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var reader = cmd.ExecuteReader();
        reader.Read();
        var lon = reader.GetDouble(0);
        var lat = reader.GetDouble(1);
        var center = new Point(lon, lat);
        reader.Close();
        conn.Close();
        return center;
    }

    private static (BoundingBox, double, double) GetBounds(IDbConnection conn, string sql)
    {
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var reader = cmd.ExecuteReader();
        reader.Read();
        // increase the boundingbox a little to avoid missing geometries at the edges
        var delta = 0.000001;
        var xmin = reader.GetDouble(0)-delta;
        var ymin = reader.GetDouble(1)-delta;
        var xmax = reader.GetDouble(2)+delta;
        var ymax = reader.GetDouble(3)+delta;
        var zmin = reader.GetDouble(4);
        var zmax = reader.GetDouble(5);
        var bbox = new BoundingBox(xmin, ymin, xmax, ymax);
        reader.Close();
        conn.Close();
        return (bbox, zmin, zmax);
    }
}




