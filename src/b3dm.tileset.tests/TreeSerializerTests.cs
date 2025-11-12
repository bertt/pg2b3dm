using System.Collections.Generic;
using B3dm.Tileset.Extensions;
using NUnit.Framework;
using subtree;
using Wkx;

namespace B3dm.Tileset.Tests;

public class TreeSerializerTests
{
    [Test]
    public void SerializeToImplicitTilingTestWithLocalProjection()
    {
        // arrange
        var translation = new double[] { 842500.24914489756, 6518515.9952387661, 0 };
        var bbox = new double[] { 841974.33514389757, 6517985.9342377661, 843026.16314589756, 6519046.056239767, 155.337, 208.582 };
        var crs = "EPSG:5698";
        // act
        var json = TreeSerializer.ToImplicitTileset(translation, bbox, 500, 2, crs: crs, keepProjection: true);
        // assert
        Assert.That(json != null, Is.True);
        Assert.That(json.asset.crs , Is.EqualTo(crs));
        // In case of location projection, we use boundingVolume box instead of region
        Assert.That(json.root.boundingVolume.box, Is.Not.Null);
        Assert.That(json.root.boundingVolume.region, Is.Null);

        var box = json.root.boundingVolume.box;
        Assert.That(box.Length, Is.EqualTo(12));
        Assert.That(box[0], Is.EqualTo(0));
        Assert.That(box[1], Is.EqualTo(0));

        // The z value is the center of the z range (in this case)
        var zv = bbox[4] + 0.5 * (bbox[5] - bbox[4]);
        Assert.That(box[2], Is.EqualTo(zv));

        // X half box size
        Assert.That(box[3], Is.EqualTo(0.5 * (bbox[2] - bbox[0])));
        Assert.That(box[4], Is.EqualTo(0));
        Assert.That(box[5], Is.EqualTo(0));

        // y half box size
        Assert.That(box[6], Is.EqualTo(0));
        Assert.That(box[7], Is.EqualTo(0.5 * (bbox[3] - bbox[1])));
        Assert.That(box[8], Is.EqualTo(0));

        // z half box size
        Assert.That(box[9], Is.EqualTo(0));
        Assert.That(box[10], Is.EqualTo(0));
        Assert.That(box[11], Is.EqualTo(0.5 * (bbox[5] - bbox[4]))); ;
    }

    [Test]
    public void SerializeToImplicitTilingTestWithGlobalProjection()
    {
        // arrange
        var translation = new double[] { 842500.24914489756, 6518515.9952387661, 0 };
        var bbox = new double[] { 841974.33514389757, 6517985.9342377661, 843026.16314589756, 6519046.056239767, 155.337, 208.582 };
        var crs = "EPSG:5698";
        // act
        var json = TreeSerializer.ToImplicitTileset(translation, bbox, 500, 2, crs: crs, keepProjection: false);
        // assert
        Assert.That(json != null, Is.True);
        Assert.That(json.asset.crs, Is.EqualTo(crs));
        // In case of global projection, we use region instead of region
        Assert.That(json.root.boundingVolume.box, Is.Null);
        Assert.That(json.root.boundingVolume.region, Is.Not.Null);

        var region = json.root.boundingVolume.region;
        Assert.That(region.Length, Is.EqualTo(6));
    }

    [Test]
    public void SerializeToImplicitTilingTestWithEcefTransform()
    {
        // arrange - Create a full ENU to ECEF transformation matrix (16 elements)
        // This represents a proper transformation including rotation and translation
        var fullTransform = new double[] {
            -0.5, 0.5, 0.707, 0.0,      // Column 0 (East in ECEF)
            -0.5, -0.5, 0.707, 0.0,     // Column 1 (North in ECEF)
            0.707, 0.707, 0.0, 0.0,     // Column 2 (Up in ECEF)
            3771793.97, 319574.77, 5087988.98, 1.0  // Column 3 (Translation)
        };
        var bbox = new double[] { 153000, 463000, 154000, 464000, 0, 100 };
        
        // act - useEcefTransform=true means: use box bounding volume and full ECEF transform matrix
        var json = TreeSerializer.ToImplicitTileset(fullTransform, bbox, 500, 2, crs: "", keepProjection: false, useEcefTransform: true);
        
        // assert
        Assert.That(json != null, Is.True);
        // When crs is empty, it should be null or empty in the asset
        Assert.That(string.IsNullOrEmpty(json.asset.crs), Is.True);
        // When useEcefTransform is true, we use boundingVolume box (like keepProjection)
        Assert.That(json.root.boundingVolume.box, Is.Not.Null);
        Assert.That(json.root.boundingVolume.region, Is.Null);

        var box = json.root.boundingVolume.box;
        Assert.That(box.Length, Is.EqualTo(12));
        
        // Transform should be the full matrix, not just translation
        Assert.That(json.root.transform.Length, Is.EqualTo(16));
        // Verify rotation components are preserved (not identity)
        Assert.That(json.root.transform[0], Is.EqualTo(fullTransform[0]));
        Assert.That(json.root.transform[1], Is.EqualTo(fullTransform[1]));
        Assert.That(json.root.transform[4], Is.EqualTo(fullTransform[4]));
        Assert.That(json.root.transform[5], Is.EqualTo(fullTransform[5]));
        // Verify translation part
        Assert.That(json.root.transform[12], Is.EqualTo(fullTransform[12]));
        Assert.That(json.root.transform[13], Is.EqualTo(fullTransform[13]));
        Assert.That(json.root.transform[14], Is.EqualTo(fullTransform[14]));
    }

    [Test]
    public void SerializeToImplicitTilingTest()
    {
        // arrange
        var t0 = new Tile(0, 0, 0 );
        var t1 = new Tile(1, 1,1);
        var translation = new double[] { -8406745.0078531764, 4744614.2577285888, 38.29 };
        var bbox = new double[] { 0, 0, 1, 1 };

        // act
        var json = TreeSerializer.ToImplicitTileset(translation, bbox, 500, 5);

        // assert
        Assert.That(json != null, Is.True);
    }

    [Test]
    public void SerializeTree()
    {
        // arrange
        var t0=new Tile(0, 0,0);
        t0.Available = true;
        t0.BoundingBox = new BoundingBox(0, 0, 10, 10).ToArray();
        t0.ZMin = 0;
        t0.ZMax = 10;

        var t1 = new Tile(1, 1, 1);
        t1.Available = true;
        t1.BoundingBox = new BoundingBox(0,0,10,10).ToArray();
        t1.ZMin = 0;
        t1.ZMax = 10;
        var tiles = new List<Tile> { t0, t1 };

        // act
        var translation = new double[] { -8406745.0078531764, 4744614.2577285888, 38.29 };
        var bbox = new double[] { 0, 0, 1, 1, 0, 10 };

        // assert
        var tileset = TreeSerializer.ToTileset(tiles, translation, bbox, 500, 2);
        Assert.That(tileset.root.children.Count == 2, Is.True);
    }


    [Test]
    public void SerializeTreeWithLods()
    {
        // arrange
        var t0 = new Tile(0,0,0);
        t0.Lod = 0;
        t0.Available = true;
        t0.BoundingBox = new BoundingBox(0, 0, 10, 10).ToArray();
        t0.ZMin = 0;
        t0.ZMax = 10;
        var t0_1 = new Tile(2,0,1);
        t0_1.Lod = 1;
        t0_1.Available = true;
        t0_1.BoundingBox = new BoundingBox(0, 0, 10, 10).ToArray();
        t0_1.ZMin = 0;
        t0_1.ZMax = 10;
        t0.Children = new List<Tile> { t0_1 };

        var tiles = new List<Tile> { t0 };

        // act
        var translation = new double[] { -8406745.0078531764, 4744614.2577285888, 38.29 };
        var bbox = new double[] { 0, 0, 1, 1, 0, 10 };

        // assert
        var tileset = TreeSerializer.ToTileset(tiles, translation, bbox, 500);
        Assert.That(tileset.root.children[0].children.Count == 1, Is.True);
        Assert.That(tileset.root.geometricError==250, Is.True);
        Assert.That(tileset.root.children[0].geometricError == 125, Is.True);
        Assert.That(tileset.root.children[0].children[0].geometricError == 15.625, Is.True);
    }
}
