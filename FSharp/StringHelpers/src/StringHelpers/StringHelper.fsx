//#load "StringHelper.fs"

open System
open System.Globalization
open System.Text

[<Literal>]
let private MAX_SLUG_CHAR_LENGTH = 50

/// <summary>
/// <para>Remove diacritics from the given text.</para>
/// <para>See: https://stackoverflow.com/a/249126 </para>
/// </summary>
let removeDiacritics (text: string) =
    if isNull (text) then
        null
    else
        let normalizedString = text.Normalize(NormalizationForm.FormD)
        let stringBuilder = StringBuilder()

        // Only keep strings that are non-spacing marks.
        normalizedString
        |> Seq.iter (fun c ->
            let unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c)

            if unicodeCategory <> UnicodeCategory.NonSpacingMark then
                do stringBuilder.Append c |> ignore)

        stringBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC)

/// <summary>
/// Returns true if the character is NOT a non-spacing mark; false if it is.
/// </summary>
/// <remarks>
/// Example of a non-spacing mark: codepoint 786 (&quot;Combining grave accent&quot;).
/// </remarks>
let isNotNonSpacingMark (c : char) =
    CharUnicodeInfo.GetUnicodeCategory(c) <> UnicodeCategory.NonSpacingMark

/// <summary>
/// Normalize the string to FormD, in which an accented character is separated into different codepoints.
/// </summary>
/// <remarks>
/// For special UNICODE characters, separate them into different code points. Ex: &quot;à&quot; becomes codepoint 97 
/// (&quot;Latin small letter A&quot;) followed by codepoint 786 (&quot;Combining grave accent&quot;).
/// See: https://stackoverflow.com/a/3288164
/// </remarks>
let normalizeToFormD (s : string) = s.Normalize(NormalizationForm.FormD)

/// <summary>
/// Normalize the string to FormC, in which an accented character is represented by a single codepoint.
/// </summary>
/// <remarks>
/// For special UNICODE characters, form C uses a single letter-with-accent codepoint. For example,
/// &quot;à&quot; can be codepoint 224 (&quot;Latin small letter A with grave&quot;).
/// </remarks>
let normalizeToFormC (s : string) = s.Normalize(NormalizationForm.FormC)    

/// <summary>
/// <para>Remove diacritics from the given text.</para>
/// <para>See: https://stackoverflow.com/a/249126 </para>
/// </summary>
let removeDiacriticsFunctional (text: string) =
    if isNull (text) then
        null
    else
        text
        // Normalize accented characters into separate codepoints (base character and non-spacing mark).
        |> normalizeToFormD
        // Keep any characters that are NOT a non-spacing mark (e.g., the Combining grave accent mentioned above).
        //   IOW, filter out any non-spacing marks.
        |> Seq.filter isNotNonSpacingMark
        // Convert back to a character array and then a proper String.
        |> Seq.toArray
        |> String
        // Normalize accented characters into a single codepoint.
        |> normalizeToFormC


let dessert = "crème brûlée"
let dessertNoDiacritics = dessert |> removeDiacritics
let dessertNoDiacriticsFunctional = dessert |> removeDiacriticsFunctional

printfn $"{dessert}"
printfn $"No Diacritics: {dessertNoDiacritics}"
printfn $"No Diacritics (functional): {dessertNoDiacriticsFunctional}"
