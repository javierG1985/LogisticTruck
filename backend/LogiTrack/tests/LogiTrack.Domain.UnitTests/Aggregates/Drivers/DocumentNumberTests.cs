using LogiTrack.Domain.Aggregates.Drivers;
using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.UnitTests.Aggregates.Drivers;

public sealed class DocumentNumberTests
{
    [Theory]
    [InlineData("123456")]
    [InlineData("1234567890")]
    [InlineData("80123456")]
    public void Create_DigitsBetweenSixAndTen_IsAccepted(string raw)
    {
        Assert.Equal(raw, DocumentNumber.Create(raw).Value);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("12345678901")]
    [InlineData("12.345.678")]
    [InlineData("ABC12345")]
    [InlineData("")]
    [InlineData(null)]
    public void Create_InvalidFormat_ThrowsDomainException(string? raw)
    {
        Assert.Throws<DomainException>(() => DocumentNumber.Create(raw));
    }

    [Fact]
    public void TryCreate_WithSurroundingSpaces_TrimsAndAccepts()
    {
        var accepted = DocumentNumber.TryCreate(" 1234567 ", out var documentNumber);

        Assert.True(accepted);
        Assert.Equal("1234567", documentNumber!.Value);
    }
}
