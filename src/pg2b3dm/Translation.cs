using System;
using Wkb2Gltf;
using Wkx;

namespace pg2b3dm;
public static class Translation
{
    public static double[] ToEcef(Point center_wgs84)
    {
        double[] translation;
        var v3 = SpatialConverter.GeodeticToEcef((double)center_wgs84.X, (double)center_wgs84.Y, 0);
        translation = [v3.X, v3.Y, v3.Z];

        return translation;
    }

    /// <summary>
    /// Computes the full ENU (East-North-Up) to ECEF transformation matrix.
    /// This matrix transforms local coordinates (in meters) to ECEF coordinates.
    /// Returns a 4x4 matrix in column-major order as used by 3D Tiles/glTF.
    /// </summary>
    public static double[] GetEnuToEcefTransform(Point center_wgs84)
    {
        var lon = (double)center_wgs84.X;
        var lat = (double)center_wgs84.Y;
        
        // Convert to radians
        var lonRad = lon * Math.PI / 180.0;
        var latRad = lat * Math.PI / 180.0;
        
        // Get ECEF position of center
        var ecefPos = SpatialConverter.GeodeticToEcef(lon, lat, 0);
        
        // Compute ENU to ECEF rotation matrix
        var sinLon = Math.Sin(lonRad);
        var cosLon = Math.Cos(lonRad);
        var sinLat = Math.Sin(latRad);
        var cosLat = Math.Cos(latRad);
        
        // 4x4 transformation matrix in column-major order
        // This matrix transforms ENU coordinates to ECEF
        var transform = new double[16]
        {
            // Column 0 (East direction in ECEF)
            -sinLon,
            cosLon,
            0.0,
            0.0,
            
            // Column 1 (North direction in ECEF)
            -sinLat * cosLon,
            -sinLat * sinLon,
            cosLat,
            0.0,
            
            // Column 2 (Up direction in ECEF)
            cosLat * cosLon,
            cosLat * sinLon,
            sinLat,
            0.0,
            
            // Column 3 (Translation - ECEF position)
            ecefPos.X,
            ecefPos.Y,
            ecefPos.Z,
            1.0
        };
        
        return transform;
    }
}
