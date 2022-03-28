namespace RandomStringHelper

/// Methods that generate random strings.
module RandomStringHelper =

    open System
    open System.Buffers
    open System.Linq
    open System.Security.Cryptography


    //
    // Types
    //

    [<Struct>]
    type private StringCreateArgs = {
        AvailableCharacters : char[]
        RandomBytes : byte[]
        Length : int
        }

    //[<IsReadOnly; Struct>]
    //type private StringCreateArgs(availableCharacters: char[], randomBytes: byte[], length: int) =
    //    member this.AvailableCharacters = availableCharacters
    //    member this.RandomBytes = randomBytes
    //    member this.Length = length


    //
    // Data
    //

    let private alphabetLower = "abcdefghijklmnopqrstovwxyz"
    let private digits = "0123456789"
    let private symbols = "`~!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?"

    let availableCharactersAlphaNumericOnly = 
        alphabetLower
            .ToCharArray()
            .Concat(alphabetLower.ToUpper())
            .Concat(digits)
            .ToArray()

    let availableCharactersPlusDashUnderscore =
        availableCharactersAlphaNumericOnly
            .Concat([| '-'; '_' |])
            .ToArray()

    let availableCharacters =
        availableCharactersAlphaNumericOnly
            .Concat(symbols)
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
        // Remember, ArrayPool returns an array with a minimum size that is likely larger than what you
        //   request, so use the requested string length as the upper bound of the for loop, not the size
        //   of the random bytes array.

        // Module arithmetic on each byte requires the character set count to also be a byte. We control
        //   the character sets, so we know that this count will fit within a byte.
        let availableCharCount = Convert.ToByte(args.AvailableCharacters.Length)

        // Loop through and encode each byte. We can't use collection functions because Span<T> can't
        //   be captured in a closure.
        let mutable ixByte = 0
        while ixByte < args.Length do

            let randomByte = args.RandomBytes[ixByte]
            randomString[ixByte] <- args.AvailableCharacters[int(randomByte % availableCharCount)]

            ixByte <- ixByte + 1


    /// Generate a cryptographically-strong array of random bytes and return them encoded as a string that
    /// can contain the characters in [a-zA-Z0-9], and optionally '-' and '_'.
    let private internalGenerateAlphanumericString (includeDashAndUnderscore: bool) (length: int) =
        ()


    //
    // Public functions
    //

    /// <summary>
    /// Generate a cryptographically-strong array of random bytes and return them encoded as a string that
    /// can contain the characters in [a-zA-Z0-9].
    /// </summary>
    /// <param name="length">The length of the random string to generate.</param>
    /// <returns>The random bytes encoded as a string that can contain the characters in [a-zA-Z0-9].</returns>
    let generateAlphanumericString (length: int) =
        internalGenerateAlphanumericString false length

    /// <summary>
    /// Generate a cryptographically-strong array of random bytes and return them encoded as a string that
    /// can contain the characters in [a-zA-Z0-9-_].
    /// </summary>
    /// <param name="length">The length of the random string to generate.</param>
    /// <returns>The random bytes encoded as a string that can contain the characters in [a-zA-Z0-9-_].</returns>
    let generateAlphanumericStringWithDashUnderscore (length: int) =
        internalGenerateAlphanumericString true length

    /// <summary>
    /// Generate a cryptographically-strong array of random bytes and return them encoded as a string that
    /// can contain the characters in [a-zA-Z0-9-_], as well as the various symbol characters.
    /// </summary>
    /// <param name="length">The length of the random string to generate.</param>
    /// <returns>The random bytes encoded as a string that can contain the characters in [a-zA-Z0-9-_], as 
    /// well as the various symbol characters.</returns>
    let generateRandomString (length: int) =
        if length <= 0 then
            invalidArg (nameof length) (sprintf $"Invalid length value '%d{length}'. It must be greater than 0")

        let randomBytes = ArrayPool<byte>.Shared.Rent(length)
        try
            generateRandomBytes (randomBytes.AsSpan()) length

            String.Create(
                length, 
                { AvailableCharacters = availableCharacters; RandomBytes = randomBytes; Length = length}, 
                encodeBytesAsCharacters
                )
        finally
            ArrayPool<byte>.Shared.Return(randomBytes)


