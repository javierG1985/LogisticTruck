using LogiTrack.Domain.Modules.Fleet.Trucks;
using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.UnitTests.Modules.Fleet.Trucks;

public sealed class PlateTests
{
    [Theory]
    [InlineData("ABC123")]
    [InlineData("xyz789")]
    [InlineData("  kmt045 ")]
    public void Create_WithValidFormat_ReturnsNormalizedPlate(string raw)
    {
        var plate = Plate.Create(raw);

        Assert.Equal(raw.Trim().ToUpperInvariant(), plate.Value);
    }

    [Theory]
    [InlineData("AB1234")]
    [InlineData("ABCD12")]
    [InlineData("123ABC")]
    [InlineData("ABC-123")]
    [InlineData("")]
    public void Create_WithInvalidFormat_ThrowsDomainException(string raw)
    {
        Assert.Throws<DomainException>(() => Plate.Create(raw));
    }

    [Fact]
    public void Create_WithNull_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Plate.Create(null));
    }

    [Fact]
    public void Equals_WithSameValueInDifferentCase_AreEqual()
    {
        Assert.Equal(Plate.Create("abc123"), Plate.Create("ABC123"));
    }

    [Fact]
    public void TryCreate_WithInvalidFormat_ReturnsFalseAndNoPlate()
    {
        var created = Plate.TryCreate("nope", out var plate);

        Assert.False(created);
        Assert.Null(plate);
    }
}
