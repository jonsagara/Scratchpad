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
let removeDiacritics (text : string) =
    if isNull(text) then
        null
    else
        let normalizedString = text.Normalize(NormalizationForm.FormD)
        let stringBuilder = StringBuilder()

        // Only keep strings that are non-spacing marks.
        normalizedString
        |> Seq.iter (fun c -> 
            let unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c)
            if unicodeCategory <> UnicodeCategory.NonSpacingMark then do
                stringBuilder.Append c |> ignore
            )

        stringBuilder.ToString().Normalize(NormalizationForm.FormC)

let removeDiacriticsFunctional (text : string) =
    if isNull(text) then
        null
    else
        let nonDiacriticsChars =
            text.Normalize(NormalizationForm.FormD)
            |> Seq.filter (fun c ->
                // Filter out diacritics (non-spacing characters that indiciate modifiations of a base character).
                let unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c)
                unicodeCategory <> UnicodeCategory.NonSpacingMark
                )
            |> Seq.toArray
            |> String

        nonDiacriticsChars.Normalize(NormalizationForm.FormC)


let dessert = "crème brûlée"
let dessertNoDiacritics = dessert |> removeDiacritics
let dessertNoDiacriticsFunctional = dessert |> removeDiacriticsFunctional

printfn $"{dessert}"
printfn $"No Diacritics: {dessertNoDiacritics}"
printfn $"No Diacritics (functional): {dessertNoDiacriticsFunctional}"
