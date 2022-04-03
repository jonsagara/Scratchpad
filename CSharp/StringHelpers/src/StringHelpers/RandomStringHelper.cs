using System.Buffers;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace StringHelpers;

public static class RandomStringHelper
{
    private const string AlphabetLower = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Symbols = "`~!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?";

    private static readonly char[] _availableCharactersAlphaNumericOnly;
    private static readonly char[] _availableCharactersPlusDashUnderscore;
    private static readonly char[] _availableCharacters;
    private static readonly char[] _uppercaseAlphaAndNumbers;

    static RandomStringHelper()
    {
        _availableCharactersAlphaNumericOnly = AlphabetLower
            .ToCharArray()
            .Concat(AlphabetLower.ToUpper())
            .Concat(Digits)
            .ToArray();

        _availableCharactersPlusDashUnderscore = _availableCharactersAlphaNumericOnly
            .Concat(new[] { '-', '_' })
            .ToArray();

        _availableCharacters = _availableCharactersAlphaNumericOnly
            .Concat(Symbols)
            .ToArray();

        _uppercaseAlphaAndNumbers = AlphabetLower
            .ToUpper()
            .ToCharArray()
            .Concat(Digits)
            .ToArray();
    }


    /// <summary>
    /// Generates a cryptographically-strong array of random bytes and return them as a base64 url-encoded string. 
    /// </summary>
    /// <param name="byteCount">The number of random bytes to generate.</param>
    /// <returns>The random bytes as a base64 url-encoded string.</returns>
    public static string GenerateRandomBase64UrlEncodedString(int byteCount)
    {
        if (byteCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteCount), $"Invalid {nameof(byteCount)} value '{byteCount}'. It must be greater than 0.");
        }

        var randomStringBytes = ArrayPool<byte>.Shared.Rent(byteCount);
        try
        {
            GenerateRandomBytes(randomStringBytes, byteCount);

            return WebEncoders.Base64UrlEncode(randomStringBytes.AsSpan().Slice(0, byteCount));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(randomStringBytes, clearArray: true);
        }
    }

    /// <summary>
    /// Generate a cryptographically-strong array of random bytes and return them as a base64-encoded string. 
    /// </summary>
    /// <param name="byteCount">The number of random bytes to generate.</param>
    /// <returns>The random bytes as a base64-encoded string.</returns>
    public static string GenerateRandomBase64EncodedString(int byteCount)
    {
        if (byteCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteCount), $"Invalid {nameof(byteCount)} value '{byteCount}'. It must be greater than 0.");
        }

        var randomStringBytes = ArrayPool<byte>.Shared.Rent(byteCount);
        try
        {
            GenerateRandomBytes(randomStringBytes, byteCount);

            return Convert.ToBase64String(randomStringBytes.AsSpan().Slice(0, byteCount));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(randomStringBytes, clearArray: true);
        }
    }

    /// <summary>
    /// Generates a cryptographically-strong array of random bytes and return them encoded as a string that
    /// can contain the characters in [A-Z0-9].
    /// </summary>
    /// <param name="length">The length of the random string to generate.</param>
    /// <returns>The random bytes encoded as a string that can contain the characters in [A-Z0-9].</returns>
    public static string GenerateUppercaseAlphanumericString(int length)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), $"Invalid {nameof(length)} value '{length}'. It must be greater than 0.");
        }

        return InternalGenerateString(_uppercaseAlphaAndNumbers, length);
    }

    /// <summary>
    /// Generate a cryptographically-strong array of random bytes and return them encoded as a string that
    /// can contain the characters in [a-zA-Z0-9-_].
    /// </summary>
    /// <param name="length">The length of the random string to generate.</param>
    /// <returns>The random bytes encoded as a string that can contain the characters in [a-zA-Z0-9-_].</returns>
    public static string GenerateAlphanumericString(int length, bool includeDashAndUnderscore = true)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), $"Invalid {nameof(length)} value '{length}'. It must be greater than 0.");
        }

        var availableCharacters = includeDashAndUnderscore
            ? _availableCharactersPlusDashUnderscore
            : _availableCharactersAlphaNumericOnly;

        return InternalGenerateString(availableCharacters, length);
    }

    /// <summary>
    /// Generate a cryptographically-strong array of random bytes and return them encoded as a string that
    /// can contain the characters in [a-zA-Z0-9-_], as well as the various symbol characters.
    /// </summary>
    /// <param name="length">The length of the random string to generate.</param>
    /// <returns>The random bytes encoded as a string that can contain the characters in [a-zA-Z0-9-_], as 
    /// well as the various symbol characters.</returns>
    public static string GenerateRandomString(int length)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), $"Invalid {nameof(length)} value '{length}'. It must be greater than 0.");
        }

        return InternalGenerateString(_availableCharacters, length);
    }


    //
    // Private methods
    //

    private static string InternalGenerateString(char[] availableCharacters, int length)
    {
        // We need to control for modulo bias. Since we can't set the upper limit of bytes generated by
        //   RandomNumberGenerator, we do the next best thing and generate a uint32 of random data for 
        //   each character, and then take the modulo of those bytes. This effectively eliminates the
        //   modulo bias.
        // See: https://stackoverflow.com/a/1344255

        // Generate 4 bytes of randomness for each character.
        var multiplier = sizeof(uint);

        var randomStringBytes = ArrayPool<byte>.Shared.Rent(length * multiplier);
        try
        {
            GenerateRandomBytes(randomStringBytes, length * multiplier);

            return string.Create(
                length: length,
                state: new StringCreateArgs(AvailableCharacters: availableCharacters, RandomStringBytes: randomStringBytes, Length: length, Multiplier: multiplier),
                action: EncodeBytesAsCharacters
                );
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(randomStringBytes, clearArray: true);
        }
    }

    /// <summary>
    /// Fills a span with cryptographically strong random bytes.
    /// </summary>
    private static void GenerateRandomBytes(Span<byte> buffer, int length)
    {
        using var rng = RandomNumberGenerator.Create();

        // A pooled array is likely larger than the requested size. Only generate the requested number
        //   of bytes.
        rng.GetBytes(buffer.Slice(0, length));
    }

    private static void EncodeBytesAsCharacters(Span<char> destSpan, StringCreateArgs args)
    {
        // Remember, ArrayPool returns an array with a minimum size that is likely larger than what you
        //   request, so use the requested string length as the upper bound of the for loop, not the size
        //   of the random bytes array.
        var randomBytes = args.RandomStringBytes.AsSpan();

        for (var ixByte = 0; ixByte < args.Length; ixByte++)
        {
            // Get 4 bytes at a time.
            var randomNumber = BitConverter.ToUInt32(randomBytes.Slice(start: ixByte * args.Multiplier, length: args.Multiplier));

            // Mod by the number of available characters to index into the array.
            destSpan[ixByte] = args.AvailableCharacters[randomNumber % args.AvailableCharacters.Length];
        }
    }

    private record struct StringCreateArgs(char[] AvailableCharacters, byte[] RandomStringBytes, int Length, int Multiplier);
}
