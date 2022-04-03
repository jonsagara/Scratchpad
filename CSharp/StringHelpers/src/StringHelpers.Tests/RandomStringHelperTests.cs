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
}
