open StringHelpers

let writeSample (functionName : string) (generatedString : string) =
    printfn $"{functionName}:"
    printfn $"  Value: {generatedString}"
    printfn $"  Length: {generatedString.Length}"
    printfn ""

writeSample (nameof RandomStringHelper.generateRandomBase64UrlEncodedString) (RandomStringHelper.generateRandomBase64UrlEncodedString 64)
writeSample (nameof RandomStringHelper.generateRandomBase64EncodedString) (RandomStringHelper.generateRandomBase64EncodedString 64)
writeSample (nameof RandomStringHelper.generateUppercaseAlphanumericString) (RandomStringHelper.generateUppercaseAlphanumericString 64)
writeSample (nameof RandomStringHelper.generateAlphanumericString) (RandomStringHelper.generateAlphanumericString 64)
writeSample (nameof RandomStringHelper.generateAlphanumericStringWithDashUnderscore) (RandomStringHelper.generateAlphanumericStringWithDashUnderscore 64)
writeSample (nameof RandomStringHelper.generateRandomString) (RandomStringHelper.generateRandomString 64)
