namespace StringHelpers.Tests

open System
open System.Linq
open StringHelpers
open Xunit

type RandomStringHelperTests() =

    //
    // Private methods
    //

    /// Perform integer division, and then perform ceil() on the quotient and remainder.
    let ceilingDivision (dividend : int) (divisor : int) =
        // This commented out code has the same result, but the longer version below is much clearer
        //   in its meaning. Since we're only using it for tests, we don't need the absolute best performance.
        //
        // (dividend + divisor - 1) / divisor;

        let (quotient, remainder) = Math.DivRem(dividend, divisor)

        if quotient >= 0 then
            // The result was positive. Since we're rounding toward positive infinity, treat remainders of 
            //   any non-zero magnitude the same: if they're non-zero, we need to add one to the quotient.
            if remainder <> 0 then
                // Numbers didn't divide evenly. Round up to the nearest integer.
                quotient + 1
            else
                // Numbers divided evenly. Return the quotient as-is.
                quotient
        else
            // When the quotient is negative, rounding "up" to the nearest integer (i.e., towards positive
            //   infinity) means discarding any remainder and keeping just the quotient.
            quotient

    let getExpectedPaddedBase64StringLength (stringLength : int) =
        // Three 8-bit bytes of input data (3 * 8 bits = 24 bits) can be represented by four 6-bit
        //   Base64 digits (4 * 6 bits = 24 bits). So:
        //
        //   4 * ceil(input string length / 3) = number of base 64 digits required to represent the input data.
        4 * ceilingDivision stringLength 3

    let getExpectedUnpaddedBase64UrlEncodedStringLength (stringLength : int) =
        // The string is not padded with extra characters to fit an even 24 bits for 4 Base64 characters.
        //   There are 6 bits per Base64 character, so ceil(total bits / 6) is the number of characters
        //   encoded as Base64.
        let bits = 8 * stringLength;
        ceilingDivision bits 6


    //
    // Test methods
    //
    
    [<Fact>]
    member this.alphabetLowerHas26Characters() =
        Assert.Equal(26, RandomStringHelper._alphabetLower.Length)

    [<Fact>]
    member this.allAlphabetLowerCharactersAreValid() =
        RandomStringHelper._alphabetLower
        |> Seq.iter (fun alphabetChar ->
            Assert.True(alphabetChar >= 'a' && alphabetChar <= 'z', $"'{alphabetChar}' is not a valid [a,z] character.");
            )
    
    [<Fact>]
    member this.alphaLowerContainsAllLowercaseAlphabetCharacters() =
        // Make a hash out of available characters.
        let alphaLowerLookup = RandomStringHelper._alphabetLower.ToHashSet();

        [| 'a' .. 'z' |]
        |> Array.iter (fun alphaLowerChar ->
            Assert.Contains(alphaLowerChar, alphaLowerLookup)
            )

    [<Fact>]
    member this.digitsHas10Characters() =
        Assert.Equal(10, RandomStringHelper._digits.Length)

    [<Fact>]
    member this.allDigitsCharactersAreValid() =
        RandomStringHelper._digits
        |> Seq.iter (fun digitChar ->
            Assert.True(digitChar >= '0' && digitChar <= '9', $"'{digitChar}' is not a valid [0,9] character.");
            )

    [<Fact>]
    member this.digitsContainsAllDigitCharacters() =
        // Make a hash out of available characters.
        let digitsLookup = RandomStringHelper._digits.ToHashSet();

        [| '0' .. '9' |]
        |> Array.iter (fun digitChar ->
            Assert.Contains(digitChar, digitsLookup)
            )


    //
    // Ensure that we can't pass an invalid length or byte count.
    //

    [<Theory>]
    [<InlineData(-1)>]
    [<InlineData(0)>]
    member this.generateRandomBase64UrlEncodedString_InvalidByteCountThrows (invalidByteCount : int) =
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> RandomStringHelper.generateRandomBase64UrlEncodedString(invalidByteCount) |> ignore)

    [<Theory>]
    [<InlineData(-1)>]
    [<InlineData(0)>]
    member this.generateRandomBase64EncodedString_InvalidByteCountThrows (invalidByteCount : int) =
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> RandomStringHelper.generateRandomBase64EncodedString(invalidByteCount) |> ignore)

    [<Theory>]
    [<InlineData(-1)>]
    [<InlineData(0)>]
    member this.generateUppercaseAlphanumericString_InvalidLengthThrows (invalidByteCount : int) =
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> RandomStringHelper.generateUppercaseAlphanumericString(invalidByteCount) |> ignore)

    [<Theory>]
    [<InlineData(-1)>]
    [<InlineData(0)>]
    member this.generateAlphanumericString_InvalidLengthThrows (invalidByteCount : int) =
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> RandomStringHelper.generateAlphanumericString(invalidByteCount) |> ignore)

    [<Theory>]
    [<InlineData(-1)>]
    [<InlineData(0)>]
    member this.generateAlphanumericStringWithDashUnderscore_InvalidLengthThrows (invalidByteCount : int) =
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> RandomStringHelper.generateAlphanumericStringWithDashUnderscore(invalidByteCount) |> ignore)

    [<Theory>]
    [<InlineData(-1)>]
    [<InlineData(0)>]
    member this.generateRandomString_InvalidLengthThrows (invalidByteCount : int) =
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> RandomStringHelper.generateRandomString(invalidByteCount) |> ignore)


    //
    // Ensure the length of the returned string matches the expected length.
    //

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(10)>]
    [<InlineData(32)>]
    [<InlineData(63)>]
    [<InlineData(64)>]
    [<InlineData(128)>]
    [<InlineData(256)>]
    member this.generateRandomBase64UrlEncodedString_ReturnedStringLengthMatches (length : int) =
        let expectedLength = getExpectedUnpaddedBase64UrlEncodedStringLength length
        let generatedString = RandomStringHelper.generateRandomBase64UrlEncodedString length
        
        Assert.NotNull(generatedString)
        Assert.Equal(expectedLength, generatedString.Length)

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(10)>]
    [<InlineData(32)>]
    [<InlineData(63)>]
    [<InlineData(64)>]
    [<InlineData(128)>]
    [<InlineData(256)>]
    member this.generateRandomBase64EncodedString_ReturnedStringLengthMatches (length : int) =
        let expectedLength = getExpectedPaddedBase64StringLength length
        let generatedString = RandomStringHelper.generateRandomBase64EncodedString length

        Assert.NotNull(generatedString)
        Assert.Equal(expectedLength, generatedString.Length)

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(10)>]
    [<InlineData(32)>]
    [<InlineData(63)>]
    [<InlineData(64)>]
    [<InlineData(128)>]
    [<InlineData(256)>]
    member this.generateUppercaseAlphanumericString_ReturnedStringLengthMatches (length : int) =
        let generatedString = RandomStringHelper.generateUppercaseAlphanumericString length

        Assert.NotNull(generatedString)
        Assert.Equal(length, generatedString.Length)

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(10)>]
    [<InlineData(32)>]
    [<InlineData(63)>]
    [<InlineData(64)>]
    [<InlineData(128)>]
    [<InlineData(256)>]
    member this.generateAlphanumericString_ReturnedStringLengthMatches (length : int) =
        let generatedString = RandomStringHelper.generateAlphanumericString length

        Assert.NotNull(generatedString)
        Assert.Equal(length, generatedString.Length)

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(10)>]
    [<InlineData(32)>]
    [<InlineData(63)>]
    [<InlineData(64)>]
    [<InlineData(128)>]
    [<InlineData(256)>]
    member this.generateRandomString_ReturnedStringLengthMatches (length : int) =
        let randomString = RandomStringHelper.generateRandomString length

        Assert.NotNull(randomString)
        Assert.Equal(length, randomString.Length)


    //
    // Ensure the returned string has no dashes and no underscores.
    //

    [<Fact>]
    member this.generateAlphanumericString_ReturnsStringWithNoDashesAndNoUnderscores () =
        [| 0 .. 99 |]
        |> Array.iter (fun ixIteration ->
            let randomString = RandomStringHelper.generateAlphanumericString 64

            Assert.NotNull(randomString)
            Assert.DoesNotContain("-", randomString)
            Assert.DoesNotContain("_", randomString)
            )


