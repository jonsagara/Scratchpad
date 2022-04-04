﻿
namespace StringHelpers

/// Methods that generate random strings.
module RandomStringHelper =

    open System
    open System.Buffers
    open System.Linq
    open System.Runtime.CompilerServices
    open System.Security.Cryptography
    open Microsoft.AspNetCore.WebUtilities

    [<assembly:InternalsVisibleTo("StringHelpers.Tests")>]
    do
        ()


    //
    // Types
    //

    [<Struct>]
    type private StringCreateArgs = {
        AvailableCharacters : char[]
        RandomBytes : byte[]
        Length : int
        NumberSize: int
        }


    //
    // Data
    //

    [<Literal>]
    let internal _alphabetLower = "abcdefghijklmnopqrstuvwxyz"
    [<Literal>]
    let internal _digits = "0123456789"
    let private _symbols = "`~!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?"

    let private _alphanumericMixedCase = 
        _alphabetLower
            .ToCharArray()
            .Concat(_alphabetLower.ToUpper())
            .Concat(_digits)
            .ToArray()

    let internal _alphanumericMixedCasePlusDashUnderscore =
        _alphanumericMixedCase
            .Concat([| '-'; '_' |])
            .ToArray()

    let private _alphanumericMixedCasePlusSymbols =
        _alphanumericMixedCase
            .Concat(_symbols)
            .ToArray()

    let private _alphanumericUppercase = 
        _alphabetLower
            .ToUpper()
            .ToCharArray()
            .Concat(_digits)
            .ToArray()


    //
    // Private functions
    //

    /// Generate a cryptographically-strong sequence of bytes.
    let private generateRandomBytes (buffer : Span<byte>) (length : int) =
        use rng = RandomNumberGenerator.Create()

        // A pooled array is likely larger than the requested size. Only generate the requested number
        //   of bytes.
        rng.GetBytes(buffer.Slice(0, length))

    /// Turn the bytes into a readable string from a predefined array of characters.
    let private encodeBytesAsCharacters (randomString : Span<char>) (args : StringCreateArgs) =

        // Modulo arithmetic on each byte requires the character set count to also be a byte. We control
        //   the character sets, so we know that this count will fit within a byte.
        let availableCharCount = Convert.ToUInt32(args.AvailableCharacters.Length)

        // Remember, ArrayPool returns an array with a minimum size that is likely larger than what you
        //   request, so use the requested string length as the upper bound of the while loop, not the size
        //   of the random bytes array.
        // Loop through and encode each byte. We can't use collection functions because Span<T> can't
        //   be captured in a closure.
        let randomBytes = args.RandomBytes.AsSpan()

        let mutable ixByte = 0
        while ixByte < args.Length do

            // Get 4 bytes at a time.
            let randomNumber = BitConverter.ToUInt32(randomBytes.Slice(ixByte * args.NumberSize, args.NumberSize))

            // Mod by the number of available characters to index into the array.
            randomString[ixByte] <- args.AvailableCharacters[int(randomNumber % availableCharCount)]

            ixByte <- ixByte + 1

    /// Generate a random string, encoding the bytes with the characters in the specified character set.
    let private internalGenerateString (availableCharacters : char[]) (length : int) =

        // We need to control for modulo bias. Since we can't set the upper limit of bytes generated by
        //   RandomNumberGenerator, we do the next best thing and generate a uint32 of random data for 
        //   each character, and then take the modulo of those bytes. This effectively eliminates the
        //   modulo bias.
        // See: https://stackoverflow.com/a/1344255

        // Generate 4 bytes of randomness for each character.
        let numberSize = sizeof<uint32>

        let randomBytes = ArrayPool<byte>.Shared.Rent(length * numberSize)
        try
            generateRandomBytes (randomBytes.AsSpan()) (length * numberSize)

            String.Create(
                length, 
                { AvailableCharacters = availableCharacters; RandomBytes = randomBytes; Length = length; NumberSize = numberSize }, 
                encodeBytesAsCharacters
                )
        finally
            ArrayPool<byte>.Shared.Return(randomBytes, true)

    /// Generate a cryptographically-strong array of random bytes and return them encoded as a string that
    /// can contain the characters in [a-zA-Z0-9], and optionally '-' and '_'.
    let private internalGenerateAlphanumericString (includeDashAndUnderscore : bool) (length : int) =
        
        let characterSet = 
            match includeDashAndUnderscore with
            | true -> _alphanumericMixedCasePlusDashUnderscore
            | false -> _alphanumericMixedCase

        internalGenerateString characterSet length


    //
    // Public functions
    //

    /// <summary>
    /// Generates a cryptographically-strong array of random bytes and return them as a base64 url-encoded string. 
    /// </summary>
    /// <remarks>The underlying <see cref="WebEncoders.Base64UrlEncode(ReadOnlySpan{byte})"/> removes any padding characters.</remarks>
    /// <param name="byteCount">The number of random bytes to generate.</param>
    /// <returns>The random bytes as a base64 url-encoded string.</returns>
    let generateRandomBase64UrlEncodedString (byteCount: int) =
        if byteCount <= 0 then
            raise (ArgumentOutOfRangeException(nameof byteCount, byteCount, $"Invalid {nameof byteCount} value '{byteCount}'. It must be greater than 0."))

        let randomBytes = ArrayPool<byte>.Shared.Rent(byteCount)
        try
            generateRandomBytes (randomBytes.AsSpan()) byteCount

            WebEncoders.Base64UrlEncode(randomBytes.AsSpan().Slice(0, byteCount));
        finally
            ArrayPool<byte>.Shared.Return(randomBytes, true)

    /// <summary>
    /// Decodes a string generated by the <see cref="generateRandomBase64UrlEncodedString(int)"/> method.
    /// </summary>
    /// <param name="base64UrlEncoded">A base64 url-encoded string to decode. Must not be null.</param>
    let decodeBase64UrlEncodedString (base64UrlEncoded : string) =
        ArgumentNullException.ThrowIfNull (nameof base64UrlEncoded)
        WebEncoders.Base64UrlDecode(base64UrlEncoded)

    /// <summary>
    /// Generate a cryptographically-strong array of random bytes and return them as a base64-encoded string. 
    /// </summary>
    /// <remarks>The underlying <see cref="Convert.ToBase64String(ReadOnlySpan{byte}, Base64FormattingOptions)"/> leaves
    /// any padding intact.</remarks>
    /// <param name="byteCount">The number of random bytes to generate.</param>
    /// <returns>The random bytes as a base64-encoded string.</returns>
    let generateRandomBase64EncodedString (byteCount: int) =
        if byteCount <= 0 then
            raise (ArgumentOutOfRangeException(nameof byteCount, byteCount, $"Invalid {nameof byteCount} value '{byteCount}'. It must be greater than 0."))

        let randomBytes = ArrayPool<byte>.Shared.Rent(byteCount)
        try
            generateRandomBytes (randomBytes.AsSpan()) byteCount

            Convert.ToBase64String(randomBytes.AsSpan().Slice(0, byteCount))
        finally
            ArrayPool<byte>.Shared.Return(randomBytes, true)

    /// <summary>
    /// Decodes a string generated by the <see cref="generateRandomBase64EncodedString(int)"/> method.
    /// </summary>
    /// <param name="base64UrlEncoded">A base64 encoded string to decode. Must not be null.</param>
    let decodeBase64EncodedString (base64Encoded : string) =
        ArgumentNullException.ThrowIfNull (nameof base64Encoded)
        Convert.FromBase64String(base64Encoded)

    /// <summary>
    /// Generate a cryptographically-strong array of random bytes and return them encoded as a string that
    /// can contain the characters in [A-Z0-9].
    /// </summary>
    /// <param name="length">The length of the random string to generate.</param>
    /// <returns>The random bytes encoded as a string that can contain the characters in [A-Z0-9].</returns>
    let generateUppercaseAlphanumericString (length: int) =
        if length <= 0 then
            raise (ArgumentOutOfRangeException(nameof length, length, $"Invalid {nameof length} value '{length}'. It must be greater than 0"))

        internalGenerateString _alphanumericUppercase length

    /// <summary>
    /// Generate a cryptographically-strong array of random bytes and return them encoded as a string that
    /// can contain the characters in [a-zA-Z0-9].
    /// </summary>
    /// <param name="length">The length of the random string to generate.</param>
    /// <returns>The random bytes encoded as a string that can contain the characters in [a-zA-Z0-9].</returns>
    let generateAlphanumericString (length: int) =
        if length <= 0 then
            raise (ArgumentOutOfRangeException(nameof length, length, $"Invalid {nameof length} value '{length}'. It must be greater than 0"))

        internalGenerateAlphanumericString false length

    /// <summary>
    /// Generate a cryptographically-strong array of random bytes and return them encoded as a string that
    /// can contain the characters in [a-zA-Z0-9-_].
    /// </summary>
    /// <param name="length">The length of the random string to generate.</param>
    /// <returns>The random bytes encoded as a string that can contain the characters in [a-zA-Z0-9-_].</returns>
    let generateAlphanumericStringWithDashUnderscore (length: int) =
        if length <= 0 then
            raise (ArgumentOutOfRangeException(nameof length, length, $"Invalid {nameof length} value '{length}'. It must be greater than 0"))

        internalGenerateAlphanumericString true length

    /// <summary>
    /// Generate a cryptographically-strong array of random bytes and return them encoded as a string that
    /// can contain the characters in [a-zA-Z0-9-_], as well as the various symbol characters.
    /// </summary>
    /// <param name="length">The length of the random string to generate.</param>
    /// <returns>The random bytes encoded as a string that can contain the characters in [a-zA-Z0-9-_], as 
    /// well as the various symbol characters.</returns>
    let generateRandomString (length : int) =
        if length <= 0 then
            raise (ArgumentOutOfRangeException(nameof length, length, $"Invalid {nameof length} value '{length}'. It must be greater than 0"))

        internalGenerateString _alphanumericMixedCasePlusSymbols length


