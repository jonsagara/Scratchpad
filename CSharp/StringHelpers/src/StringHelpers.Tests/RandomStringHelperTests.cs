using System.Buffers.Text;
using Xunit;

namespace StringHelpers.Tests;

public class RandomStringHelperTests
{
    [Fact]
    public void AlphabetLowerHas26Characters()
    {
        Assert.Equal(26, RandomStringHelper.AlphabetLower.Length);
    }

    [Fact]
    public void AllAlphabetLowerCharactersAreValid()
    {
        foreach (var alphabetChar in RandomStringHelper.AlphabetLower)
        {
            Assert.True(alphabetChar >= 'a' && alphabetChar <= 'z', $"'{alphabetChar}' is not a valid [a,z] character.");
        }
    }

    [Fact]
    public void AlphaLowerContainsAllLowercaseAlphabetCharacters()
    {
        // Make a hash out of available characters.
        var alphaLowerLookup = RandomStringHelper.AlphabetLower.ToHashSet();

        for (var alphaLowerChar = 'a'; alphaLowerChar <= 'z'; alphaLowerChar++)
        {
            Assert.Contains(alphaLowerChar, alphaLowerLookup);
        }
    }

    [Fact]
    public void DigitsHas10Characters()
    {
        Assert.Equal(10, RandomStringHelper.Digits.Length);
    }

    [Fact]
    public void AllDigitsCharactersAreValid()
    {
        foreach (var digitChar in RandomStringHelper.Digits)
        {
            Assert.True(digitChar >= '0' && digitChar <= '9', $"'{digitChar}' is not a valid [0,9] character.");
        }
    }

    [Fact]
    public void DigitsContainsAllDigitCharacters()
    {
        // Make a hash out of available characters.
        var digitsLookup = RandomStringHelper.Digits.ToHashSet();

        for (var digitChar = '0'; digitChar <= '9'; digitChar++)
        {
            Assert.Contains(digitChar, digitsLookup);
        }
    }


    //
    // Ensure that we can't pass an invalid length or byte count.
    //

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void GenerateRandomBase64UrlEncodedString_InvalidByteCountThrows(int invalidByteCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RandomStringHelper.GenerateRandomBase64UrlEncodedString(invalidByteCount));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void GenerateRandomBase64EncodedString_InvalidByteCountThrows(int invalidByteCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RandomStringHelper.GenerateRandomBase64EncodedString(invalidByteCount));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void GenerateUppercaseAlphanumericString_InvalidLengthThrows(int invalidLength)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RandomStringHelper.GenerateUppercaseAlphanumericString(invalidLength));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void GenerateAlphanumericString_InvalidLengthThrows(int invalidLength)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RandomStringHelper.GenerateAlphanumericString(invalidLength));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void GenerateRandomString_InvalidLengthThrows(int invalidLength)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RandomStringHelper.GenerateRandomString(invalidLength));
    }


    //
    // Ensure the length of the returned string matches the expected length.
    //

    //[Theory]
    //[InlineData(1)]
    //[InlineData(10)]
    //[InlineData(32)]
    //[InlineData(63)]
    //[InlineData(64)]
    //[InlineData(128)]
    //[InlineData(256)]
    //public void GenerateRandomBase64UrlEncodedString_ReturnedStringLengthMatches(int length)
    //{
    //    var generatedString = RandomStringHelper.GenerateRandomBase64UrlEncodedString(length);

    //    Assert.NotNull(generatedString);
    //    Assert.Equal(length, generatedString.Length);
    //}

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(32)]
    [InlineData(63)]
    [InlineData(64)]
    [InlineData(128)]
    [InlineData(256)]
    public void GenerateRandomBase64EncodedString_ReturnedStringLengthMatches(int length)
    {
        var expectedLength = Base64.GetMaxEncodedToUtf8Length(length);
        var generatedString = RandomStringHelper.GenerateRandomBase64EncodedString(length);

        Assert.NotNull(generatedString);
        Assert.Equal(expectedLength, generatedString.Length);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(32)]
    [InlineData(63)]
    [InlineData(64)]
    [InlineData(128)]
    [InlineData(256)]
    public void GenerateUppercaseAlphanumericString_ReturnedStringLengthMatches(int length)
    {
        var generatedString = RandomStringHelper.GenerateUppercaseAlphanumericString(length);

        Assert.NotNull(generatedString);
        Assert.Equal(length, generatedString.Length);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(32)]
    [InlineData(63)]
    [InlineData(64)]
    [InlineData(128)]
    [InlineData(256)]
    public void GenerateAlphanumericString_ReturnedStringLengthMatches(int length)
    {
        var generatedString = RandomStringHelper.GenerateAlphanumericString(length);

        Assert.NotNull(generatedString);
        Assert.Equal(length, generatedString.Length);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(32)]
    [InlineData(63)]
    [InlineData(64)]
    [InlineData(128)]
    [InlineData(256)]
    public void GenerateRandomString_ReturnedStringLengthMatches(int length)
    {
        var randomString = RandomStringHelper.GenerateRandomString(length);

        Assert.NotNull(randomString);
        Assert.Equal(length, randomString.Length);
    }


    //
    // Ensure the returned string has no dashes and no underscores.
    //

    [Fact]
    public void GenerateAlphanumericString_ReturnsStringWithNoDashesAndNoUnderscores()
    {
        for (var ixIteration = 0; ixIteration < 100; ixIteration++)
        {
            var randomString = RandomStringHelper.GenerateAlphanumericString(length: 64, includeDashAndUnderscore: false);

            Assert.NotNull(randomString);
            Assert.DoesNotContain("-", randomString);
            Assert.DoesNotContain("_", randomString);
        }
    }
}
