using LogiTrack.Application.Common.Search;

namespace LogiTrack.Application.UnitTests.Common.Search;

public sealed class TextSearchTests
{
    [Fact]
    public void ToContainsPattern_LowercasesAndWrapsTheSearch()
    {
        Assert.Equal("%volvo%", TextSearch.ToContainsPattern("VoLvO"));
    }

    [Theory]
    [InlineData("50%", "%50\\%%")]
    [InlineData("a_b", "%a\\_b%")]
    [InlineData("a\\b", "%a\\\\b%")]
    public void ToContainsPattern_EscapesLikeWildcardsAndBackslash(string search, string expected)
    {
        Assert.Equal(expected, TextSearch.ToContainsPattern(search));
    }

    [Fact]
    public void ToLowerText_OutsideAQuery_LowercasesInvariant()
    {
        Assert.Equal("ñandú", TextSearch.ToLowerText("ÑANDÚ"));
    }
}
