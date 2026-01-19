using System;
using B3dm.Tileset;
using NUnit.Framework;

namespace B3dm.Tileset.Tests;

public class ZoomLevelCalculatorTests
{
    [Test]
    public void GetMinZoomLevel_LargeFeature_ReturnsZero()
    {
        // A feature that is 50% of the root tile should appear at zoom 0
        var rootTileSize = 100.0;
        var featureSize = 50.0;
        var minSizeRatio = 0.01; // 1%

        var result = ZoomLevelCalculator.GetMinZoomLevel(featureSize, rootTileSize, minSizeRatio);

        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void GetMinZoomLevel_SmallFeature_ReturnsHigherZoom()
    {
        // A feature that is 0.5% of the root tile needs higher zoom
        // At zoom 0: tile = 100, min size = 1, feature = 0.5 (too small)
        // At zoom 1: tile = 50, min size = 0.5, feature = 0.5 (fits)
        var rootTileSize = 100.0;
        var featureSize = 0.5;
        var minSizeRatio = 0.01; // 1%

        var result = ZoomLevelCalculator.GetMinZoomLevel(featureSize, rootTileSize, minSizeRatio);

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void GetMinZoomLevel_VerySmallFeature_ReturnsEvenHigherZoom()
    {
        // A very small feature needs much higher zoom
        var rootTileSize = 100.0;
        var featureSize = 0.1;
        var minSizeRatio = 0.01; // 1%

        var result = ZoomLevelCalculator.GetMinZoomLevel(featureSize, rootTileSize, minSizeRatio);

        // At zoom 0: need 1, have 0.1 -> too small (ratio = 10)
        // log2(10) ≈ 3.32, ceil = 4
        Assert.That(result, Is.GreaterThanOrEqualTo(3));
    }

    [Test]
    public void GetMinZoomLevel_ZeroSize_ReturnsZero()
    {
        var result = ZoomLevelCalculator.GetMinZoomLevel(0, 100, 0.01);
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void GetMinZoomLevel_ZeroRootSize_ReturnsZero()
    {
        var result = ZoomLevelCalculator.GetMinZoomLevel(10, 0, 0.01);
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void GetTileSizeAtZoom_ZoomZero_ReturnsRootSize()
    {
        var result = ZoomLevelCalculator.GetTileSizeAtZoom(100.0, 0);
        Assert.That(result, Is.EqualTo(100.0));
    }

    [Test]
    public void GetTileSizeAtZoom_ZoomOne_ReturnsHalfSize()
    {
        var result = ZoomLevelCalculator.GetTileSizeAtZoom(100.0, 1);
        Assert.That(result, Is.EqualTo(50.0));
    }

    [Test]
    public void GetTileSizeAtZoom_ZoomTwo_ReturnsQuarterSize()
    {
        var result = ZoomLevelCalculator.GetTileSizeAtZoom(100.0, 2);
        Assert.That(result, Is.EqualTo(25.0));
    }

    [Test]
    public void ShouldIncludeAtZoom_FeatureLargeEnough_ReturnsTrue()
    {
        var featureSize = 10.0;
        var tileDiagonal = 100.0;
        var minSizeRatio = 0.01; // 1%

        var result = ZoomLevelCalculator.ShouldIncludeAtZoom(featureSize, tileDiagonal, minSizeRatio);

        Assert.That(result, Is.True);
    }

    [Test]
    public void ShouldIncludeAtZoom_FeatureTooSmall_ReturnsFalse()
    {
        var featureSize = 0.5;
        var tileDiagonal = 100.0;
        var minSizeRatio = 0.01; // 1%

        var result = ZoomLevelCalculator.ShouldIncludeAtZoom(featureSize, tileDiagonal, minSizeRatio);

        Assert.That(result, Is.False);
    }

    [Test]
    public void ShouldIncludeAtZoom_FeatureExactlyAtThreshold_ReturnsTrue()
    {
        var featureSize = 1.0;
        var tileDiagonal = 100.0;
        var minSizeRatio = 0.01; // 1%

        var result = ZoomLevelCalculator.ShouldIncludeAtZoom(featureSize, tileDiagonal, minSizeRatio);

        Assert.That(result, Is.True);
    }

    [Test]
    public void CalculateDiagonal_UnitSquare_ReturnsSqrt2()
    {
        var result = ZoomLevelCalculator.CalculateDiagonal(0, 0, 1, 1);
        Assert.That(result, Is.EqualTo(Math.Sqrt(2)).Within(0.0001));
    }

    [Test]
    public void CalculateDiagonal_Rectangle_ReturnsCorrectValue()
    {
        // 3-4-5 triangle
        var result = ZoomLevelCalculator.CalculateDiagonal(0, 0, 3, 4);
        Assert.That(result, Is.EqualTo(5.0).Within(0.0001));
    }

    [Test]
    public void CalculateDiagonal_ZeroSize_ReturnsZero()
    {
        var result = ZoomLevelCalculator.CalculateDiagonal(5, 5, 5, 5);
        Assert.That(result, Is.EqualTo(0.0));
    }
}
