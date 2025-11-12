using NUnit.Framework;
using B3dm.Tileset;

namespace B3dm.Tileset.Tests;

public class ProjectionUnitCheckerTests
{
    [Test]
    public void TestWGS84UsesDegreesNotMeters()
    {
        // EPSG:4326 (WGS84) uses degrees, should return false
        // Note: This test doesn't require database as we check known codes
        var result = ProjectionUnitChecker.IsProjectionInMeters(null, 4326);
        Assert.That(result, Is.False);
    }

    [Test]
    public void TestEPSG4979UsesDegreesNotMeters()
    {
        // EPSG:4979 (WGS84 3D) uses degrees, should return false
        var result = ProjectionUnitChecker.IsProjectionInMeters(null, 4979);
        Assert.That(result, Is.False);
    }
}
