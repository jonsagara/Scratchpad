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

    let private maxSlugLength maxLengthInChars (slug: string) =
        if slug.Length <= maxLengthInChars then
            slug.Length
        else
            maxLengthInChars

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
                    .Substring(0, slug |> maxSlugLength maxLengthInChars)
                    .Trim()

            // Replace white space with hyphens.
            slug <- Regex.Replace(slug, @"\s", "-")

            slug

    /// <summary>
    /// Remove characters that are non-alphanumeric, white space, or hyphens.
    /// </summary>
    let private removeInvalidCharacters (value : string) =
        Regex.Replace(value, @"[^a-zA-Z0-9\s-]", String.Empty)

    /// <summary>
    /// Collapse multiple consecutive white space values into a single space.
    /// </summary>
    let private collapseMultipleWhiteSpaceToSingleSpace (value : string) =
        Regex.Replace(value, @"\s+", " ")

    /// <summary>
    /// If null, return null. Otherwise, trim any leading and trailing white space.
    /// </summary>
    let private nullSafeTrim (value : string) =
        match isNull(value) with
        | true -> null
        | false -> value.Trim()

    /// <summary>
    /// Limit the length of the slug to a maximum number of characters.
    /// </summary>
    let private truncateSlug (maxLengthInChars : int) (value : string) =
        let slugLength =
            if value.Length <= maxLengthInChars then
                value.Length
            else
                maxLengthInChars

        value.Substring(0, slugLength)

    /// <summary>
    /// Remove characters that are non-alphanumeric, white space, or hyphens.
    /// </summary>
    let private replaceWhiteSpaceWithHyphens (value : string) =
        Regex.Replace(value, @"\s", "-")

    /// <summary>
    /// <para>Convert a string into a URL- and file name-safe slug.</para>
    /// <para>See: https://stackoverflow.com/a/2921135 (except ours doesn't use RemoveAccent)</para>
    /// </summary>
    let private toSlugInternalFunctional (maxLengthInChars: int) (value: string) =
        if isNull (value) then
            null
        else
            value
            // We know we're passing in a non-null string, so we're going to get a non-null string back.
            |> removeDiacriticsFunctional
            // Remove invalid chars.
            |> removeInvalidCharacters
            // Convert multiple spaces into one space.
            |> collapseMultipleWhiteSpaceToSingleSpace
            // Trim any leading and trailing white space.
            |> nullSafeTrim
            // Truncate the slug at the maximum allowed characters.
            |> truncateSlug maxLengthInChars
            // Trim any leading and trailing white space from the truncated slug.
            |> nullSafeTrim
            // Replace white space with hyphens.
            |> replaceWhiteSpaceWithHyphens

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
