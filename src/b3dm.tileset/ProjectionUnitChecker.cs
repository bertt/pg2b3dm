using System;
using System.Data;
using Npgsql;

namespace B3dm.Tileset;

public static class ProjectionUnitChecker
{
    /// <summary>
    /// Checks if the given SRID uses meters as its unit of measure
    /// Returns true if the projection uses meters, false otherwise (e.g., degrees)
    /// </summary>
    public static bool IsProjectionInMeters(IDbConnection conn, int srid)
    {
        // EPSG:4326 and EPSG:4979 use degrees, not meters
        if (srid == 4326 || srid == 4979)
        {
            return false;
        }

        // Query the spatial_ref_sys table to check the projection unit
        // We look for 'UNIT["metre"' or 'UNIT["meter"' in the srtext or proj4text
        var sql = @"
            SELECT 
                CASE 
                    WHEN srtext ILIKE '%UNIT[""metre%' OR srtext ILIKE '%UNIT[""meter%' 
                         OR proj4text LIKE '%+units=m %' OR proj4text LIKE '%+units=m+%'
                    THEN true
                    ELSE false
                END as uses_meters
            FROM spatial_ref_sys 
            WHERE srid = @srid";

        try
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                var param = cmd.CreateParameter();
                param.ParameterName = "@srid";
                param.Value = srid;
                cmd.Parameters.Add(param);

                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    return (bool)result;
                }
            }
        }
        catch
        {
            // If we can't determine the unit, assume it's not in meters
            return false;
        }

        return false;
    }
}
