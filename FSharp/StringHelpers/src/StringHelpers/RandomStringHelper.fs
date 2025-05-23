﻿
namespace StringHelpers

/// Methods that generate random strings.
module RandomStringHelper =

    open System
    open System.Buffers
    open System.Runtime.CompilerServices
    open System.Security.Cryptography
    open Microsoft.AspNetCore.WebUtilities

    [<assembly:InternalsVisibleTo("StringHelpers.Tests")>]
    do
        ()


    //
    // Data
    //

    [<Literal>]
    let internal _alphabetLower = "abcdefghijklmnopqrstuvwxyz"
    [<Literal>]
    let internal _digits = "0123456789"
    [<Literal>]
    let private _symbols = "`~!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?"

    let private _alphanumeric = Array.concat [|
        _alphabetLower |> Seq.toArray;
        _alphabetLower.ToUpper() |> Seq.toArray;
        _digits |> Seq.toArray;
        |]

    let internal _alphanumericPlusDashUnderscore = Array.concat [|
        _alphanumeric;
        [| '-'; '_' |];
        |]

    // Symbols includes dash and underscore.
    let private _alphanumericPlusSymbols = Array.concat [|
        _alphanumeric;
        _symbols |> Seq.toArray;
        |]

    let private _uppercaseAlphanumeric = Array.concat [|
        _alphabetLower.ToUpper() |> Seq.toArray;
        _digits |> Seq.toArray;
        |]


    //
    // Private functions
    //

    /// Generate a cryptographically-strong sequence of bytes.
    let private generateRandomBytes (buffer : Span<byte>) (length : int) =
        // A pooled array is likely larger than the requested size. Only generate the requested number
        //   of bytes.
        RandomNumberGenerator.Fill(buffer.Slice(0, length))


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
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(byteCount, 0, (nameof(byteCount)))

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
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(byteCount, 0, (nameof(byteCount)))

        let randomBytes = ArrayPool<byte>.Shared.Rent(byteCount)
        try
            generateRandomBytes (randomBytes.AsSpan()) byteCount

            Convert.ToBase64String(randomBytes.AsSpan().Slice(0, byteCount))
        finally
            ArrayPool<byte>.Shared.Return(randomBytes, true)

    /// <summary>
    /// Decodes a string generated by the <see cref="generateRandomBase64EncodedString(int)"/> method.
    /// </summary>
    /// <param name="base64Encoded">A base64 encoded string to decode. Must not be null.</param>
    let decodeBase64EncodedString (base64Encoded : string) =
        ArgumentNullException.ThrowIfNull (base64Encoded, (nameof base64Encoded))
        Convert.FromBase64String(base64Encoded)

    /// <summary>
    /// Generate a cryptographically-strong array of random bytes and return them encoded as a string that
    /// can contain the characters in [A-Z0-9].
    /// </summary>
    /// <param name="length">The length of the random string to generate.</param>
    /// <returns>The random bytes encoded as a string that can contain the characters in [A-Z0-9].</returns>
    let generateUppercaseAlphanumericString (length: int) =
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(length, 0, (nameof length))

        RandomNumberGenerator.GetString(_uppercaseAlphanumeric, length)

    /// <summary>
    /// Generate a cryptographically-strong array of random bytes and return them encoded as a string that
    /// can contain the characters in [a-zA-Z0-9].
    /// </summary>
    /// <param name="length">The length of the random string to generate.</param>
    /// <param name="includeDashAndUnderscore">True to include '-' and '_' in the set of available characters;
    /// false to exclude them.</param>
    /// <returns>The random bytes encoded as a string that can contain the characters in [a-zA-Z0-9].</returns>
    let generateAlphanumericString (length : int) (includeDashAndUnderscore : bool) =
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(length, 0, (nameof length))
        
        let characterSet = 
            match includeDashAndUnderscore with
            | true -> _alphanumericPlusDashUnderscore
            | false -> _alphanumeric

        RandomNumberGenerator.GetString(characterSet, length)

    /// <summary>
    /// Generate a cryptographically-strong array of random bytes and return them encoded as a string that
    /// can contain the characters in [a-zA-Z0-9-_], as well as the various symbol characters.
    /// </summary>
    /// <param name="length">The length of the random string to generate.</param>
    /// <returns>The random bytes encoded as a string that can contain the characters in [a-zA-Z0-9-_], as 
    /// well as the various symbol characters.</returns>
    let generateRandomString (length : int) =
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(length, 0, (nameof length))

        RandomNumberGenerator.GetString( _alphanumericPlusSymbols, length)


