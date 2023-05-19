namespace StringHelpers

open System
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

[<Extension>]
type ByteExtensions =

    /// <summary>
    /// Convert each byte to its equivalent hexadecimal string representation, and return them all as a string.
    /// </summary>
    /// <remarks>
    /// The extra parameter attributes are necessary so that C# sees includeDashes as an optional bool, not an FSharpOption.
    /// See: https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/parameters-and-arguments#optional-parameters
    /// </remarks>
    [<Extension>]
    static member ToHexString (bytes: byte[], [<Optional; DefaultParameterValue(false)>] includeDashes: bool) =

        ArgumentNullException.ThrowIfNull(bytes)

        match includeDashes with
        | true -> BitConverter.ToString(bytes)
        | false -> Convert.ToHexString(bytes)