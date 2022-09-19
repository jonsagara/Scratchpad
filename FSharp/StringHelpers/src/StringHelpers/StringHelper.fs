namespace StringHelpers

module StringHelper =

    open System
    open System.Globalization
    open System.Text
    open System.Text.RegularExpressions

    [<Literal>]
    let private MAX_SLUG_CHAR_LENGTH = 50

    /// <summary>
    /// <para>Remove diacritics from the given text.</para>
    /// <para>See: https://stackoverflow.com/a/249126 </para>
    /// </summary>
    let private removeDiacritics (text: string) =
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
    /// <para>Convert a string into a URL- and file name-safe slug.</para>
    /// <para>See: https://stackoverflow.com/a/2921135 (except ours doesn't use RemoveAccent)</para>
    /// </summary>
    let private toSlugInternal (maxLengthInChars: int) (value: string) =
        if isNull (value) then
            null
        else
            // We know we're passing in a non-null string, so we're going to get a non-null string back.
            let mutable slug = removeDiacritics value

            // Remove invalid chars.
            slug <- Regex.Replace(slug, @"[^a-zA-Z0-9\s-]", String.Empty)

            // Convert multiple spaces into one space.
            slug <- Regex.Replace(slug, @"\s+", " ").Trim()

            // Limit the slug length to the max specified characters.
            slug <-
                slug
                    .Substring(
                        0,
                        if slug.Length <= maxLengthInChars then
                            slug.Length
                        else
                            maxLengthInChars
                    )
                    .Trim()

            // Replace white space with hyphens.
            slug <- Regex.Replace(slug, @"\s", "-")

            slug

    /// <summary>
    /// Convert a string into a URL- and file name-safe slug. Constrain the resulting string to the specified maximum length.
    /// </summary>
    let toSlugMaxLength (maxLengthInChars: int) (value: string) =
        let maxLength =
            match maxLengthInChars < 1 with
            | true -> MAX_SLUG_CHAR_LENGTH
            | false -> maxLengthInChars

        toSlugInternal maxLength value

    /// <summary>
    /// Convert a string into a URL- and file name-safe slug. Constrain to a maximum length of 50 characters.
    /// </summary>
    let toSlug (value: string) =
        toSlugMaxLength MAX_SLUG_CHAR_LENGTH value
